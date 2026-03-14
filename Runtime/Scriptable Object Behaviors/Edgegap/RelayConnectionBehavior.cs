using FishNet;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;
using Services.Edgegap;
using FishNet.Transporting.KCP.Edgegap;
using FishNet.Transporting;


namespace Network.Edgegap
{
    /// <summary>
    /// Behavior to start and stop network connections through the Edgegap Relay.
    /// Relies on <see cref="RelayManager"/> (via <see cref="Ref"/>) for session state and authorization.
    /// </summary>
    [CreateAssetMenu(fileName = "Connection_ Relay_ Edgegap_ New", menuName = "Behaviors/Network/Connection/Relay/Edgegap")]
    public class RelayConnectionBehaviour : ConnectionBehaviour
    {
        [Tooltip("Invoked on the Lobby host when they create a new session.")]
        public UnityEvent<string> onRelaySessionIdCreated;

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

        public async override void StartServer() => await StartServerAsync();

        public async Task StartServerAsync()
        {
            CancelAndResetCts(ref serverCts);
            var ct = serverCts.Token;

            serverConnectionState = ConnectionState.Starting;

            var transportManager = InstanceFinder.TransportManager;
            var transport = (EdgegapKcpTransport)transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                var lobbyIpLookup = Ref.Get<LobbyIpLookup>();
                var relayManager = Ref.Get<RelayManager>();

                Debug.Log("Relay Connection: Lobby host, getting ips");

                var result = await lobbyIpLookup.GetLobbyIpsAsync();
                if (ct.IsCancellationRequested) return;

                var clientIps = result.lobbyIps;

                Debug.Log("Relay Connection: Creating session");
                var response = await relayManager.CreateSessionAsync(clientIps);
                if (ct.IsCancellationRequested) return;

                var relayData = BuildRelayData(relayManager, response);

                Debug.Log("Relay Connection: Relay Session Started. Starting server connection");

                transport.SetEdgegapRelayData(relayData);
                transport.StartConnection(true);

                onRelaySessionIdCreated?.Invoke(relayManager.RelaySessionId);
            }

            // wait for the connection to start or fail
            while (!ct.IsCancellationRequested && transport.GetConnectionState(true) == LocalConnectionState.Starting)
            {
                await Task.Yield();
            }

            if (!ct.IsCancellationRequested)
                serverConnectionState = ConnectionState.None;
        }

        public async override void StopServer(bool sendDisconnectMessage = true) => await StopServerAsync(sendDisconnectMessage);

        public async Task StopServerAsync(bool sendDisconnectMessage = true)
        {
            CancelCts(ref serverCts);

            serverConnectionState = ConnectionState.Stopping;

            var serverManager = InstanceFinder.ServerManager;

            if (serverManager == null || !serverManager.Started)
            {
                serverConnectionState = ConnectionState.None;
                return;
            }

            var transportManager = InstanceFinder.TransportManager;
            var transport = transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                Debug.Log("Stopping Server Connection");
                transportManager.GetTransport<Multipass>()?.StopServerConnection(sendDisconnectMessage, transportIndex);
            }

            if (Ref.TryGet<RelayManager>(out var relayManager))
            {
                Debug.Log("Stopping Relay Session");
                await relayManager.DeleteSessionIfHostAsync();
            }

            // wait for the connection to stop
            while (!serverManager.Started)
            {
                await Task.Yield();
            }

            serverConnectionState = ConnectionState.None;
        }

        public async override void StartClient() => await StartClientAsync();

        public async Task StartClientAsync()
        {
            CancelAndResetCts(ref clientCts);
            var ct = clientCts.Token;

            clientConnectionState = ConnectionState.Starting;

            var transportManager = InstanceFinder.TransportManager;
            var transport = (EdgegapKcpTransport)transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                transportManager.GetTransport<Multipass>()?.SetClientTransport(transport);

                var serverManager = InstanceFinder.ServerManager;

                // No need to set relay data if connecting as host (ServerManager already started)
                if (serverManager == null || !serverManager.Started)
                {
                    var relayManager = Ref.Get<RelayManager>();

                    if (string.IsNullOrEmpty(relayManager.RelaySessionId))
                    {
                        Debug.LogError("Relay Connection: No relay session ID available. Cannot start client.");
                        clientConnectionState = ConnectionState.None;
                        return;
                    }

                    var response = await relayManager.JoinSessionAsync();
                    if (ct.IsCancellationRequested)
                    {
                        Debug.Log("Relay Connection: Client start cancelled after JoinSessionAsync.");
                        return;
                    }

                    if (response == null)
                    {
                        Debug.LogError("Relay Connection: Failed to join relay session.");
                        clientConnectionState = ConnectionState.None;
                        return;
                    }

                    uint userAuthorizationToken = relayManager.GetLocalSessionUser(response)?.authorization_token ?? 0;

                    if (userAuthorizationToken == 0)
                    {
                        Debug.LogError("Relay Connection: Client has no valid user authorization token. The host may not have authorized this client yet.");
                        clientConnectionState = ConnectionState.None;
                        return;
                    }

                    var relayData = BuildRelayData(relayManager, response);
                    transport.SetEdgegapRelayData(relayData);
                }

                if (ct.IsCancellationRequested)
                {
                    Debug.Log("Relay Connection: Client start cancelled before transport connection.");
                    return;
                }

                transport.StartConnection(false);

                // wait for the connection to start or fail
                while (!ct.IsCancellationRequested && transport.GetConnectionState(false) == LocalConnectionState.Starting)
                {
                    await Task.Yield();
                }

                if (!ct.IsCancellationRequested)
                    clientConnectionState = ConnectionState.None;
            }
        }

        public override void StopClient()
        {
            CancelCts(ref clientCts);

            clientConnectionState = ConnectionState.Stopping;

            var clientManager = InstanceFinder.ClientManager;

            if (clientManager != null && clientManager.Started)
                clientManager.StopConnection();

            clientConnectionState = ConnectionState.None;
        }

        /// <summary>
        /// Builds <see cref="EdgegapRelayData"/> from a session response using the local player's tokens.
        /// </summary>
        EdgegapRelayData BuildRelayData(RelayManager relayManager, RelayService.SessionResponse response)
        {
            uint sessionAuthorizationToken = response.authorization_token ?? 0;
            uint userAuthorizationToken = relayManager.GetLocalSessionUser(response)?.authorization_token ?? 0;

            var relay = response.relay;
            string address = relay.ip;
            ushort serverPort = relay.ports.server.port;
            ushort clientPort = relay.ports.client.port;

            return new EdgegapRelayData(address, serverPort, clientPort, userAuthorizationToken, sessionAuthorizationToken);
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
