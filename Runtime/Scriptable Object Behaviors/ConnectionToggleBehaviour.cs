using FishNet.Transporting;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    /// <summary>
    /// Toggles the Application between two connection states - Online and Offline.
    /// 
    /// Inlcudes some debug options when developing locally with ParrelSync.
    /// </summary>
    [CreateAssetMenu(fileName = "Connection Toggle_ New", menuName = "Behaviors/Connection Toggle")]
    public class ConnectionToggleBehaviour : ManagedObject
    {
        public enum ConnectionPhase
        {
            Offline,
            StartingServer,
            StartingClient,
            StartingHost,
            OnlineAsServer,
            OnlineAsClient,
            OnlineAsHost,
            StoppingServer,
            StoppingClient,
            StoppingHost
        }

        public enum ParrelSyncMode
        {
            LocalNetwork,
            OnlineNetwork
        }

        #region Serialized Fields

        [Header("Settings")]
        [SerializeField, Tooltip("The number of times to retry starting the online client connection if it fails.")]
        private int onlineConnectionRetryCount = 3;
        [SerializeField, Tooltip("The delay between retry attempts when starting the online client connection.")]
        private float onlineConnectionRetryDelay = 2f;

        [Space(10)]

        [SerializeField, Tooltip("If marked as true, whenever a client's connection is lost while in Online mode, the connection will automatically toggle back to the Offline connection.")]
        private bool fallbackToOfflineIfOnlineConnectionFails;

        [Header("References")]

        [SerializeField]
        private NetworkStateReference networkState;

        [SerializeField]
        private LocalConnectionStateVariable serverConnectionState;

        [SerializeField]
        private LocalConnectionStateVariable clientConnectionState;

        [Space(10)]

        [SerializeField, Tooltip("The Connection to be started when the Application goes into Offline mode. This should either have no connection at all (leave this empty), or be used to start a local server connection.")]
        private ConnectionBehaviour offlineConnection;

        [SerializeField, Tooltip("The Connection to be started when the Application goes into Online mode.")]
        private ConnectionBehaviour onlineConnection;

        [Header("Events")]

        public UnityEvent<ConnectionPhase> onConnectionPhaseChanged;

        [Tooltip("Event called when the Network is toggled to Online. This event occurs after any Offline Connection has been closed, but right before the Online Connection is started.")]
        public UnityEvent onNetworkOnline;

        [Tooltip("Event called when the Network is toggled to Offline. This event occurs after any Online Connection has been closed, but right before the Offline Connection is started.")]
        public UnityEvent onNetworkOffline;

        [Tooltip("Event called when a client's connection continues to fail through all retry attempts.")]
        public UnityEvent onOnlineConnectionFailed;

        [Tooltip("Event called when a client's connection is lost while in Online mode, and the connection is falling back to Offline mode.")]
        public UnityEvent onFallingBackToOffline;

        [Header("Debug")]
        [SerializeField, Tooltip("Determines how clients will connect when a ParrelSync Clone is active. If set to `Local Network`, the editor instances will all connect using the `Parrel Sync Local Connection` connection settings. Otherwise the clones will connect through the default Online connection settings.")]
        public ParrelSyncMode parrelSyncMode = ParrelSyncMode.LocalNetwork;

        private bool isParrelSyncOnlineNetwork => parrelSyncMode == ParrelSyncMode.OnlineNetwork;

        [SerializeField, DisableIf(nameof(isParrelSyncOnlineNetwork))]
        private ConnectionBehaviour parrelSyncLocalConnection;

        private Coroutine connectionCoroutine;
        private Coroutine offlineFallbackCoroutine;

        #endregion

        public ConnectionPhase connectionPhase { get; private set; } = ConnectionPhase.Offline;

        public NetworkState NetworkState => networkState.Value;
        public LocalConnectionState ServerConnectionState => serverConnectionState.Value;
        public LocalConnectionState ClientConnectionState => clientConnectionState.Value;

        public ConnectionBehaviour OnlineConnection => useParrelSyncLocalConnection ? parrelSyncLocalConnection : onlineConnection;
        public ConnectionBehaviour OfflineConnection => offlineConnection;

        bool connectionsStopped => clientConnectionState.Value == LocalConnectionState.Stopped && serverConnectionState.Value == LocalConnectionState.Stopped;
        bool useParrelSyncLocalConnection => parrelSyncMode == ParrelSyncMode.LocalNetwork && ParrelSyncManager.IsAnyCloneRunning() && parrelSyncLocalConnection != null;

        #region ManagedObject Methods

        protected override void Initialize()
        {
            connectionCoroutine = null;
            offlineFallbackCoroutine = null;
        }

        protected override void Cleanup()
        {
            if (connectionCoroutine != null)
                CoWorker.Stop(connectionCoroutine);

            if (offlineFallbackCoroutine != null)
                CoWorker.Stop(offlineFallbackCoroutine);

            connectionCoroutine = null;
            offlineFallbackCoroutine = null;

            offlineConnection?.StopHost();
            parrelSyncLocalConnection?.StopHost();
            onlineConnection?.StopHost();
        }

        #endregion

        /// <summary>
        /// Toggle the connection state between Online and Offline.
        /// </summary>
        public void Toggle(bool asHost = false)
        {
            if (connectionCoroutine != null)
                return;

            switch (networkState.Value)
            {
                case NetworkState.Offline:
                    if (asHost)
                        GoOnlineAsHost();
                    else
                        GoOnline();
                    break;
                case NetworkState.Online:
                    GoOffline();
                    break;
            }
        }

        public void GoOnline()
        {
            if (connectionCoroutine != null)
            {
                Debug.LogWarning("Connection Toggle cannot go Online - it is currently processing a connection change.");
                return;
            }

            if (networkState.Value == NetworkState.Online)
            {
                Debug.LogWarning("Connection Toggle cannot go Online - it is already Online.");
                return;
            }

            connectionCoroutine = CoWorker.Work(DoGoOnline(false));
        }

        public void GoOnlineAsHost()
        {
            if (connectionCoroutine != null)
            {
                Debug.LogWarning("Connection Toggle cannot go Online as Host - it is currently processing a connection change.");
                return;
            }

            if (networkState.Value == NetworkState.Online)
            {
                Debug.LogWarning("Connection Toggle cannot go Online as Host - it is already Online.");
                return;
            }

            connectionCoroutine = CoWorker.Work(DoGoOnline(true));
        }

        public void GoOffline()
        {
            if (connectionCoroutine != null)
            {
                Debug.LogWarning("Connection Toggle cannot go Offline - it is currently processing a connection change.");
                return;
            }

            if (networkState.Value == NetworkState.Offline)
            {
                Debug.LogWarning("Connection Toggle cannot go Offline - it is already Offline.");
                return;
            }

            if (offlineFallbackCoroutine != null)
            {
                CoWorker.Stop(offlineFallbackCoroutine);
                offlineFallbackCoroutine = null;
            }

            connectionCoroutine = CoWorker.Work(DoGoOffline());
        }

        /// <summary>
        /// Ends any active connections immediately, without going through the normal connection toggling process. This sets the NetworkState to None.
        /// </summary>
        public void EndConnectionImmediately()
        {
            if (connectionCoroutine != null)
            {
                CoWorker.Stop(connectionCoroutine);
                connectionCoroutine = null;
            }
            if (offlineFallbackCoroutine != null)
            {
                CoWorker.Stop(offlineFallbackCoroutine);
                offlineFallbackCoroutine = null;
            }

            offlineConnection?.StopHost();
            parrelSyncLocalConnection?.StopHost();
            onlineConnection?.StopHost();

            connectionPhase = ConnectionPhase.Offline;

            networkState.Value = NetworkState.None;
            Debug.Log("Connection Toggle - Network connection ended.");

            onConnectionPhaseChanged?.Invoke(connectionPhase);
        }

        private IEnumerator DoGoOnline(bool asHost)
        {
            // Disable any existing offline connections
            offlineConnection?.StopHost();

            // Wait for all connections to stop
            while (!connectionsStopped)
                yield return null;

            networkState.Value = NetworkState.Online;

            Debug.Log("Connection Toggle - Network toggled to Online.");

            onNetworkOnline?.Invoke();

            var asServer = false;
            var startAsClient = false;
            ConnectionBehaviour targetConnection = null;

            // If any ParrelSync clones are running and we want to run the defined local connection between them (bypassing the online connection), handle that here
            if (useParrelSyncLocalConnection)
            {
                switch (ParrelSyncManager.type)
                {
                    case ParrelSyncManager.ParrelInstanceType.Server:
                        parrelSyncLocalConnection?.StartServer();
                        asServer = true;
                        break;
                    case ParrelSyncManager.ParrelInstanceType.Main:
                    case ParrelSyncManager.ParrelInstanceType.Client:
                        targetConnection = parrelSyncLocalConnection;
                        if (asHost)
                            targetConnection?.StartHost();
                        else
                            startAsClient = true;
                        break;
                }
            }
            else
            {
#if UNITY_SERVER
                onlineConnection?.StartServer(); asServer = true; 
#else
                targetConnection = onlineConnection;
                if (asHost)
                    targetConnection?.StartHost();
                else
                    startAsClient = true;
#endif
            }

            if (asHost)
                connectionPhase = ConnectionPhase.StartingHost;
            else if (asServer)
                connectionPhase = ConnectionPhase.StartingServer;
            else
                connectionPhase = ConnectionPhase.StartingClient;

            onConnectionPhaseChanged?.Invoke(connectionPhase);

            // If starting as client, run a retry loop before proceeding.
            if (startAsClient && targetConnection != null)
            {
                int attempts = 0;
                while (networkState.Value == NetworkState.Online && connectionPhase == ConnectionPhase.StartingClient)
                {
                    attempts++;

                    Debug.Log($"Connection Toggle - Starting Client Connection Attempt {attempts} of {onlineConnectionRetryCount}...");

                    // Ensure previous attempt is fully stopped before retrying.
                    while (clientConnectionState.Value == LocalConnectionState.Stopping)
                        yield return null;

                    if (clientConnectionState.Value != LocalConnectionState.Stopped)
                    {
                        targetConnection.StopClient();
                        while (clientConnectionState.Value != LocalConnectionState.Stopped)
                            yield return null;
                    }

                    targetConnection.StartClient();

                    // Wait until either started or failed back to stopped.
                    while (networkState.Value == NetworkState.Online && connectionPhase == ConnectionPhase.StartingClient)
                    {
                        var s = clientConnectionState.Value;
                        if (s == LocalConnectionState.Started)
                            break;

                        if (s == LocalConnectionState.Stopped)
                            break;

                        yield return null;
                    }

                    if (clientConnectionState.Value == LocalConnectionState.Started)
                    {
                        // Wait for a short moment to ensure connection is stable
                        float connectionStartedDelay = 0f;
                        while (connectionStartedDelay < 1f && networkState.Value == NetworkState.Online && connectionPhase == ConnectionPhase.StartingClient && clientConnectionState.Value == LocalConnectionState.Started)
                        {
                            connectionStartedDelay += Time.unscaledDeltaTime;
                            yield return null;
                        }

                        if (connectionStartedDelay >= 1f)
                            break; // connection is stable

                        else
                        {
                            Debug.LogWarning("Connection Toggle - Client connection unstable, retrying...");
                            while (clientConnectionState.Value != LocalConnectionState.Stopped)
                                yield return null;
                        }
                    }

                    if (attempts > onlineConnectionRetryCount)
                        break;

                    // Delay before next attempt.
                    float t = 0f;
                    while (t < onlineConnectionRetryDelay &&
                           networkState.Value == NetworkState.Online &&
                           connectionPhase == ConnectionPhase.StartingClient &&
                           clientConnectionState.Value != LocalConnectionState.Started)
                    {
                        t += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }

                // If still not connected after all attempts, go offline (prevents hanging in StartingClient).
                if (clientConnectionState.Value != LocalConnectionState.Started)
                {
                    Debug.LogWarning("Connection Toggle - Client failed to connect after multiple attempts, returning to Offline mode.");

                    connectionCoroutine = null;
                    GoOffline();

                    onOnlineConnectionFailed?.Invoke();

                    yield break;
                }
            }

            while (networkState.Value == NetworkState.Online &&
                   (connectionPhase == ConnectionPhase.StartingClient && clientConnectionState.Value != LocalConnectionState.Started ||
                    connectionPhase == ConnectionPhase.StartingServer && serverConnectionState.Value != LocalConnectionState.Started ||
                    connectionPhase == ConnectionPhase.StartingHost && (serverConnectionState.Value != LocalConnectionState.Started || clientConnectionState.Value != LocalConnectionState.Started)))
            {
                yield return null;
            }

            if (asHost)
                connectionPhase = ConnectionPhase.OnlineAsHost;
            else if (asServer)
                connectionPhase = ConnectionPhase.OnlineAsServer;
            else
                connectionPhase = ConnectionPhase.OnlineAsClient;

            onConnectionPhaseChanged?.Invoke(connectionPhase);

            if (fallbackToOfflineIfOnlineConnectionFails && !asHost && !asServer && offlineFallbackCoroutine == null)
            {
                offlineFallbackCoroutine = CoWorker.Work(DoGoOfflineFallback());
            }

            connectionCoroutine = null;
        }

        private IEnumerator DoGoOffline()
        {
            switch (connectionPhase)
            {
                case ConnectionPhase.OnlineAsHost:
                    connectionPhase = ConnectionPhase.StoppingHost;
                    break;
                case ConnectionPhase.OnlineAsServer:
                    connectionPhase = ConnectionPhase.StoppingServer;
                    break;
                case ConnectionPhase.OnlineAsClient:
                    connectionPhase = ConnectionPhase.StoppingClient;
                    break;
            }

            if (useParrelSyncLocalConnection)
                parrelSyncLocalConnection?.StopHost();
            else
                onlineConnection?.StopHost();

            while (!connectionsStopped)
                yield return null;


            // TEMP - Ensure the Start scene is unloaded BEFORE offline mode starts (as it potentially could remain loaded after disconnecting from a Server)
            // Unload the Start scene if it is loaded
            var startScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Start");
            if (startScene.isLoaded)
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(startScene);

            while (UnityEngine.SceneManagement.SceneManager.GetSceneByName("Start").isLoaded)
                yield return null;

            yield return null;

            connectionPhase = ConnectionPhase.Offline;

            networkState.Value = NetworkState.Offline;

            Debug.Log("Connection Toggle - Network toggled to Offline.");

            onNetworkOffline?.Invoke();

            onConnectionPhaseChanged?.Invoke(connectionPhase);

            // If this is a dedicated server (or parrel sync server), we dont want to start any connections for offline mode
            var startOfflineConnection = true;

#if UNITY_SERVER
            startOfflineConnection = false; 
#else
            if (ParrelSyncManager.type == ParrelSyncManager.ParrelInstanceType.Server) 
                startOfflineConnection = false;
#endif

            if (startOfflineConnection)
                offlineConnection?.StartHost();

            connectionCoroutine = null;
        }

        private IEnumerator DoGoOfflineFallback()
        {
            while (networkState.Value == NetworkState.Online)
            {
                // if the client connection fails, return to offline mode
                if (clientConnectionState.Value == LocalConnectionState.Stopped || ApplicationInfo.internetAvailabilityStatus != ApplicationInfo.InternetAvailabilityStatus.Online)
                {
                    Debug.LogWarning("Connection Toggle - Client connection lost, returning to Offline mode.");

                    connectionCoroutine = null;
                    GoOffline();

                    onFallingBackToOffline?.Invoke();

                    yield break;
                }

                yield return null;
            }

            offlineFallbackCoroutine = null;
        }
    }
}

