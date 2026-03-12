using FishNet;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace Network
{
    /// <summary>
    /// Behavior to start and stop network connections to a standard Server.
    /// </summary>
    [CreateAssetMenu(fileName = "Connection_ Server_ New", menuName = "Behaviors/Network/Connection/Server")]
    public class ServerConnectionBehaviour : ConnectionBehaviour
    {
        [SerializeField, Expandable]
        private ServerConnectionSettings connectionSettings;

        private CancellationTokenSource serverCts;
        private CancellationTokenSource clientCts;

        private ConnectionState serverConnectionState;
        private ConnectionState clientConnectionState;

        public override ConnectionState GetServerConnectionState() => serverConnectionState;
        public override ConnectionState GetClientConnectionState() => clientConnectionState;

        public override async void StartHost()
        {
            await StartServerAsync();
            await StartClientAsync();
        }

        public override void StartServer() => _ = StartServerAsync();

        public override void StopServer(bool sendDisconnectMessage = true)
        {
            CancelCts(ref serverCts);

            serverConnectionState = ConnectionState.Stopping;

            var transportManager = InstanceFinder.TransportManager;
            var transport = transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                transportManager.GetTransport<Multipass>()?.StopServerConnection(sendDisconnectMessage, transportIndex);
            }

            serverConnectionState = ConnectionState.None;
        }

        public override void StartClient() => _ = StartClientAsync();

        public override void StopClient()
        {
            CancelCts(ref clientCts);

            clientConnectionState = ConnectionState.Stopping;

            var clientManager = InstanceFinder.ClientManager;

            if (clientManager != null && clientManager.Started)
                clientManager.StopConnection();

            clientConnectionState = ConnectionState.None;
        }

        async Task StartServerAsync()
        {
            CancelAndResetCts(ref serverCts);
            var ct = serverCts.Token;

            serverConnectionState = ConnectionState.Starting;

            // Start the Server connection
            var transportManager = InstanceFinder.TransportManager;
            var transport = transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                transport.SetPort(connectionSettings.port);
                transport.StartConnection(true);
            }

            // Wait until the server is started
            var serverManager = InstanceFinder.ServerManager;
            while ((serverManager == null || !serverManager.Started) && !ct.IsCancellationRequested)
                await System.Threading.Tasks.Task.Yield();

            if (!ct.IsCancellationRequested)
                serverConnectionState = ConnectionState.None;
        }

        async Task StartClientAsync()
        {
            CancelAndResetCts(ref clientCts);
            var ct = clientCts.Token;

            clientConnectionState = ConnectionState.Starting;

            var transportManager = InstanceFinder.TransportManager;
            var transport = transportManager?.GetTransport(transportIndex);
            if (transport != null)
            {
                transportManager.GetTransport<Multipass>()?.SetClientTransport(transport);
                transport.SetClientAddress(connectionSettings.address);
                transport.SetPort(connectionSettings.port);
                transport.StartConnection(false);
            }

            // Wait until the client is started
            var clientManager = InstanceFinder.ClientManager;
            while ((clientManager == null || !clientManager.Started) && !ct.IsCancellationRequested)
                await System.Threading.Tasks.Task.Yield();

            if (!ct.IsCancellationRequested)
                clientConnectionState = ConnectionState.None;
        }

        /// <summary>
        /// Cancels the existing CTS and creates a fresh one.
        /// </summary>
        private static void CancelAndResetCts(ref CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancels and disposes the CTS without creating a new one.
        /// </summary>
        private static void CancelCts(ref CancellationTokenSource cts)
        {
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }
    }
}
