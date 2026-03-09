using FishNet.Transporting;
using NaughtyAttributes;
using System.Collections;
using System.Threading.Tasks;
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
        [System.Serializable]
        public enum ConnectionPhase
        {
            None,
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

        /// <summary>
        /// Determines how the system responds when a connection fails or is lost.
        /// </summary>
        public enum ConnectionFailureResponse
        {
            /// <summary>
            /// End all connections and revert to ConnectionPhase.None / NetworkState.None.
            /// </summary>
            EndConnection,
            /// <summary>
            /// Automatically fall back to the Offline connection (starts the offline host).
            /// </summary>
            FallbackToOffline
        }

        #region Serialized Fields

        [Header("Settings")]
        [SerializeField, Tooltip("The number of times to retry starting the online client connection if it fails.")]
        private int onlineConnectionRetryCount = 3;
        [SerializeField, Tooltip("The delay between retry attempts when starting the online client connection.")]
        private float onlineConnectionRetryDelay = 2f;

        [Space(10)]

        [SerializeField, Tooltip("Determines what happens when a connection fails or is lost.\n\n" +
            "FallbackToOffline: Automatically toggle back to the Offline connection.\n" +
            "EndConnection: End all connections, reverting to ConnectionPhase.None.")]
        private ConnectionFailureResponse connectionFailureResponse = ConnectionFailureResponse.EndConnection;

        [Header("References")]

        [SerializeField]
        private ConnectionPhaseVariable connectionPhase;

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

        [Tooltip("Event called when a connection is lost and the connection failure response is being executed.")]
        public UnityEvent onConnectionLost;

        [Header("Debug")]
        [SerializeField, Tooltip("Determines how clients will connect when a ParrelSync Clone is active. If set to `Local Network`, the editor instances will all connect using the `Parrel Sync Local Connection` connection settings. Otherwise the clones will connect through the default Online connection settings.")]
        public ParrelSyncMode parrelSyncMode = ParrelSyncMode.LocalNetwork;

        private bool isParrelSyncOnlineNetwork => parrelSyncMode == ParrelSyncMode.OnlineNetwork;

        [SerializeField, DisableIf(nameof(isParrelSyncOnlineNetwork))]
        private ConnectionBehaviour parrelSyncLocalConnection;

        private Coroutine connectionCoroutine;
        private Coroutine connectionFailureWatchCoroutine;

        #endregion

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
            connectionFailureWatchCoroutine = null;
        }

        protected override void Cleanup()
        {
            if (connectionCoroutine != null)
                CoWorker.Stop(connectionCoroutine);

            if (connectionFailureWatchCoroutine != null)
                CoWorker.Stop(connectionFailureWatchCoroutine);

            connectionCoroutine = null;
            connectionFailureWatchCoroutine = null;

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

            StopConnectionFailureWatch();

            connectionCoroutine = CoWorker.Work(DoGoOffline());
        }

        /// <summary>
        /// Ends any active connections, without going through the normal connection toggling process. This sets the NetworkState to None.
        /// </summary>
        public async Task EndConnection()
        {
            if (connectionCoroutine != null)
            {
                CoWorker.Stop(connectionCoroutine);
                connectionCoroutine = null;
            }

            StopConnectionFailureWatch();

            offlineConnection?.StopHost();
            parrelSyncLocalConnection?.StopHost();
            onlineConnection?.StopHost();

            while (!connectionsStopped)
                await System.Threading.Tasks.Task.Yield();

            connectionPhase.Value = ConnectionPhase.None;

            networkState.Value = NetworkState.None;

            Debug.Log("Connection Toggle - Network connection ended.");

            onConnectionPhaseChanged?.Invoke(connectionPhase.Value);
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

            StopConnectionFailureWatch();

            offlineConnection?.StopHost();
            parrelSyncLocalConnection?.StopHost();
            onlineConnection?.StopHost();

            connectionPhase.Value = ConnectionPhase.None;

            networkState.Value = NetworkState.None;
            Debug.Log("Connection Toggle - Network connection ended.");

            onConnectionPhaseChanged?.Invoke(connectionPhase.Value);
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
                connectionPhase.Value = ConnectionPhase.StartingHost;
            else if (asServer)
                connectionPhase.Value = ConnectionPhase.StartingServer;
            else
                connectionPhase.Value = ConnectionPhase.StartingClient;

            onConnectionPhaseChanged?.Invoke(connectionPhase.Value);

            // If starting as client, run a retry loop before proceeding.
            if (startAsClient && targetConnection != null)
            {
                int attempts = 0;
                while (networkState.Value == NetworkState.Online && connectionPhase.Value == ConnectionPhase.StartingClient)
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
                    while (networkState.Value == NetworkState.Online && connectionPhase.Value == ConnectionPhase.StartingClient)
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
                        while (connectionStartedDelay < 1f && networkState.Value == NetworkState.Online && connectionPhase.Value == ConnectionPhase.StartingClient && clientConnectionState.Value == LocalConnectionState.Started)
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
                           connectionPhase.Value == ConnectionPhase.StartingClient &&
                           clientConnectionState.Value != LocalConnectionState.Started)
                    {
                        t += Time.unscaledDeltaTime;
                        yield return null;
                    }
                }

                // If still not connected after all attempts, handle based on the configured failure response.
                if (clientConnectionState.Value != LocalConnectionState.Started)
                {
                    Debug.LogWarning("Connection Toggle - Client failed to connect after multiple attempts.");

                    connectionCoroutine = null;

                    HandleConnectionFailure();

                    onOnlineConnectionFailed?.Invoke();

                    yield break;
                }
            }

            while (networkState.Value == NetworkState.Online &&
                   (connectionPhase.Value == ConnectionPhase.StartingClient && clientConnectionState.Value != LocalConnectionState.Started ||
                    connectionPhase.Value == ConnectionPhase.StartingServer && serverConnectionState.Value != LocalConnectionState.Started ||
                    connectionPhase.Value == ConnectionPhase.StartingHost && (serverConnectionState.Value != LocalConnectionState.Started || clientConnectionState.Value != LocalConnectionState.Started)))
            {
                yield return null;
            }

            if (asHost)
                connectionPhase.Value = ConnectionPhase.OnlineAsHost;
            else if (asServer)
                connectionPhase.Value = ConnectionPhase.OnlineAsServer;
            else
                connectionPhase.Value = ConnectionPhase.OnlineAsClient;

            onConnectionPhaseChanged?.Invoke(connectionPhase.Value);

            StartConnectionFailureWatch();

            connectionCoroutine = null;
        }

        private IEnumerator DoGoOffline()
        {
            switch (connectionPhase.Value)
            {
                case ConnectionPhase.OnlineAsHost:
                    connectionPhase.Value = ConnectionPhase.StoppingHost;
                    break;
                case ConnectionPhase.OnlineAsServer:
                    connectionPhase.Value = ConnectionPhase.StoppingServer;
                    break;
                case ConnectionPhase.OnlineAsClient:
                    connectionPhase.Value = ConnectionPhase.StoppingClient;
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

            connectionPhase.Value = ConnectionPhase.Offline;

            networkState.Value = NetworkState.Offline;

            Debug.Log("Connection Toggle - Network toggled to Offline.");

            onNetworkOffline?.Invoke();

            onConnectionPhaseChanged?.Invoke(connectionPhase.Value);
                
            offlineConnection?.StartHost();

            StartConnectionFailureWatch();

            connectionCoroutine = null;
        }

        /// <summary>
        /// Starts the connection failure watcher coroutine, stopping any existing one first.
        /// </summary>
        private void StartConnectionFailureWatch()
        {
            StopConnectionFailureWatch();
            connectionFailureWatchCoroutine = CoWorker.Work(DoConnectionFailureWatch());
        }

        /// <summary>
        /// Stops the connection failure watcher coroutine if one is running.
        /// </summary>
        private void StopConnectionFailureWatch()
        {
            if (connectionFailureWatchCoroutine != null)
            {
                CoWorker.Stop(connectionFailureWatchCoroutine);
                connectionFailureWatchCoroutine = null;
            }
        }

        /// <summary>
        /// Monitors the active connection and triggers the configured failure response if the connection is lost.
        /// Watches both Online and Offline states.
        /// </summary>
        private IEnumerator DoConnectionFailureWatch()
        {
            while (networkState.Value == NetworkState.Online || networkState.Value == NetworkState.Offline)
            {
                bool connectionLost = false;

                switch (connectionPhase.Value)
                {
                    // Online client - lost if client stopped or internet unavailable
                    case ConnectionPhase.OnlineAsClient:
                        connectionLost = clientConnectionState.Value == LocalConnectionState.Stopped
                            || ApplicationInfo.internetAvailabilityStatus != ApplicationInfo.InternetAvailabilityStatus.Online;
                        break;

                    // Online server - lost if server stopped
                    case ConnectionPhase.OnlineAsServer:
                        connectionLost = serverConnectionState.Value == LocalConnectionState.Stopped;
                        break;

                    // Online host - lost if either server or client stopped
                    case ConnectionPhase.OnlineAsHost:
                        connectionLost = serverConnectionState.Value == LocalConnectionState.Stopped
                            || clientConnectionState.Value == LocalConnectionState.Stopped;
                        break;

                    // Offline (local host) - lost if both connections stopped unexpectedly
                    case ConnectionPhase.Offline:
                        connectionLost = connectionsStopped;
                        break;
                }

                if (connectionLost)
                {
                    Debug.LogWarning($"Connection Toggle - Connection lost during {connectionPhase.Value}.");

                    connectionCoroutine = null;

                    HandleConnectionFailure();

                    onConnectionLost?.Invoke();

                    yield break;
                }

                yield return null;
            }

            connectionFailureWatchCoroutine = null;
        }

        /// <summary>
        /// Executes the configured connection failure response.
        /// </summary>
        private void HandleConnectionFailure()
        {
            switch (connectionFailureResponse)
            {
                case ConnectionFailureResponse.FallbackToOffline:
                    if (networkState.Value == NetworkState.Offline)
                    {
                        // Already offline and the offline connection failed — revert to None.
                        Debug.Log("Connection Toggle - Offline connection failed, ending all connections.");
                        EndConnection();
                    }
                    else
                    {
                        Debug.Log("Connection Toggle - Falling back to Offline mode.");
                        GoOffline();
                    }
                    break;

                case ConnectionFailureResponse.EndConnection:
                    Debug.Log("Connection Toggle - Ending all connections.");
                    EndConnection();
                    break;
            }
        }
    }
}

