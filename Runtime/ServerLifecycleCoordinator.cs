using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Transporting;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Coordinates server lifetime policies:
    /// - Tracks connected players (via FishNet events).
    /// - Starts a grace countdown when empty.
    /// - On manual user request, uses DisconnectManager to fan-out and waits.
    /// - Finally calls an IServerStopper (Edgegap / Local) to end the process/deployment.
    /// </summary>
    [DisallowMultipleComponent]
    public class ServerLifecycleCoordinator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        NetworkStateVariable networkState;

        [SerializeField] ConnectionToggleBehaviour connectionToggle;

        [Header("Policy")]
        [Tooltip("Minimum concurrent players to keep the server alive.")]
        [SerializeField] int minPlayersToKeepAlive = 1;

        [Tooltip("Seconds to wait after the server becomes empty (reconnect window).")]
        [SerializeField] float emptyServerGraceSeconds = 30f;

        [Tooltip("Delay (seconds) after a manual shutdown is requested, for messages/RPCs to flush.")]
        [SerializeField] float manualShutdownDelaySeconds = 2f;

        [Header("Logging")]
        [SerializeField] bool verboseLogs = true;

        // Internal
        readonly HashSet<int> _connected = new HashSet<int>();
        Coroutine _emptyCountdown;
        bool _shuttingDown;

        // Stopper resolution (Edgegap if env vars exist, otherwise Local)
        IServerStopper _stopper;

        void Awake()
        {
            // Choose a stopper implementation at runtime.
            _stopper = EdgegapServerStopper.CanUseEdgegapEnv()
                ? (IServerStopper)new EdgegapServerStopper()
                : new LocalServerStopper(connectionToggle);
        }

        void OnEnable()
        {
            // Only track connections in online mode
            if (networkState.Value != NetworkState.Online)
                return;

            if (InstanceFinder.ServerManager != null)
                InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;

            if (InstanceFinder.IsServerStarted)
                SubscribeRemoteConnectionEvents(true);
        }

        void OnDisable()
        {
            if (InstanceFinder.ServerManager != null)
                InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;

            SubscribeRemoteConnectionEvents(false);
            _connected.Clear();
        }

        void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                SubscribeRemoteConnectionEvents(true);
                RebuildFromServer();
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                SubscribeRemoteConnectionEvents(false);
                _connected.Clear();
            }
        }

        void SubscribeRemoteConnectionEvents(bool subscribe)
        {
            var sm = InstanceFinder.ServerManager;
            if (sm == null) return;

            if (subscribe) sm.OnRemoteConnectionState += OnRemoteConnectionState;
            else sm.OnRemoteConnectionState -= OnRemoteConnectionState;
        }

        void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    _connected.Add(conn.ClientId);
                    VLog($"Client {conn.ClientId} joined. Now: {_connected.Count}.");
                    CancelEmptyCountdown();
                    break;

                case RemoteConnectionState.Stopped:
                    _connected.Remove(conn.ClientId);
                    VLog($"Client {conn.ClientId} left. Now: {_connected.Count}.");
                    MaybeBeginEmptyCountdown();
                    break;
            }
        }

        void RebuildFromServer()
        {
            _connected.Clear();
            foreach (var kvp in InstanceFinder.ServerManager.Clients)
                _connected.Add(kvp.Key);

            VLog($"[Rebuild] {_connected.Count} client(s) online.");
            MaybeBeginEmptyCountdown();
        }

        void MaybeBeginEmptyCountdown()
        {
            if (_shuttingDown) return;

            if (_connected.Count >= minPlayersToKeepAlive)
            {
                CancelEmptyCountdown();
                return;
            }

            if (_emptyCountdown != null)
                StopCoroutine(_emptyCountdown);

            _emptyCountdown = StartCoroutine(EmptyCountdown());
        }

        void CancelEmptyCountdown()
        {
            if (_emptyCountdown != null)
            {
                StopCoroutine(_emptyCountdown);
                _emptyCountdown = null;
                VLog("[Grace] Canceled empty-server countdown (player rejoined).");
            }
        }

        IEnumerator EmptyCountdown()
        {
            VLog($"[Grace] Below threshold; shutting down in {emptyServerGraceSeconds:0}s unless players reconnect.");
            float t = 0f;
            while (t < emptyServerGraceSeconds)
            {
                if (_connected.Count >= minPlayersToKeepAlive)
                {
                    VLog("[Grace] Abort shutdown; player rejoined.");
                    _emptyCountdown = null;

                    yield break;
                }
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            _emptyCountdown = null;
            yield return BeginShutdownFlow(0, "Empty Server Shutdown", disconnectAllFirst: false);
        }

        public void RequestManualShutdown()
        {
            if (_shuttingDown) 
                return;

            StartCoroutine(BeginShutdownFlow(0, "User Requested Shutdown", disconnectAllFirst: true));
        }

        IEnumerator BeginShutdownFlow(int disconnectCode, string message, bool disconnectAllFirst)
        {
            if (_shuttingDown) yield break;
            _shuttingDown = true;

            VLog($"[Shutdown] Reason: {message}");

            // Optional short delay so any “server closing” messages can flush to clients/UI.
            if (manualShutdownDelaySeconds > 0f)
                yield return new WaitForSeconds(manualShutdownDelaySeconds);

            // 1) Manual flow: ask all clients to disconnect using your manager (shared logic)
            if (disconnectAllFirst && Ref.TryGet(out DisconnectManager disconnectManager))
            {
                bool done = false;
                disconnectManager.Disconnect(
                    disconnectCode: disconnectCode,
                    message: message,
                    stopServer: false,                       // We stop/quit below
                    onComplete: () => done = true
                );

                // Wait until DisconnectManager reports no more clients (reuses your logic)
                while (!done) yield return null;
            }

            // 2) Politely stop the server locally (if applicable—won’t quit the process)
            //    This keeps local testing behavior tidy.
            connectionToggle?.GoOffline();

            // 3) Shared terminal step: stop the deployment / quit process
            yield return _stopper.StopAsync(message);
        }

        void VLog(string msg)
        {
            if (verboseLogs) Debug.Log($"Server Lifecycle Coordinator - {msg}");
        }
    }
}
