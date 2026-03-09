using FishNet;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public override async void StartHost()
        {
            await StartServerAsync();
            await StartClientAsync();
        }

        public async override void StartServer() => await StartServerAsync();

        public async Task StartServerAsync()
        {
            var transportManager = InstanceFinder.TransportManager;
            var transport = (EdgegapKcpTransport)transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                var lobbyIpLookup = Ref.Get<LobbyIpLookup>();
                var relayManager = Ref.Get<RelayManager>();

                Debug.Log("Relay Connection: Lobby host, getting ips");

                var result = await lobbyIpLookup.GetLobbyIpsAsync();
                var clientIps = result.lobbyIps;

                Debug.Log("Relay Connection: Creating session");
                var response = await relayManager.CreateSessionAsync(clientIps);

                var relayData = BuildRelayData(relayManager, response);

                Debug.Log("Relay Connection: Relay Session Started. Starting server connection");

                transport.SetEdgegapRelayData(relayData);
                transport.StartConnection(true);

                onRelaySessionIdCreated?.Invoke(relayManager.RelaySessionId);
            }
        }

        public async override void StopServer(bool sendDisconnectMessage = true)  => await StopServerAsync(sendDisconnectMessage);

        public async Task StopServerAsync(bool sendDisconnectMessage = true)
        {
            var serverManager = InstanceFinder.ServerManager;

            if (serverManager == null || !serverManager.Started)
                return;

            if (Ref.TryGet<RelayManager>(out var relayManager))
            {
                Debug.Log("Stopping Relay Session");
                await relayManager.DeleteSessionIfHostAsync();
            }

            var transportManager = InstanceFinder.TransportManager;
            var transport = transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                Debug.Log("Stopping Server Connection");
                transportManager.GetTransport<Multipass>()?.StopServerConnection(sendDisconnectMessage, transportIndex);
            }
        }

        public async override void StartClient() => await StartClientAsync();

        public async Task StartClientAsync()
        {
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
                        return;
                    }

                    var response = await relayManager.JoinSessionAsync();

                    if (response == null)
                    {
                        Debug.LogError("Relay Connection: Failed to join relay session.");
                        return;
                    }

                    uint userAuthorizationToken = relayManager.GetLocalSessionUser(response)?.authorization_token ?? 0;

                    if (userAuthorizationToken == 0)
                    {
                        Debug.LogError("Relay Connection: Client has no valid user authorization token. The host may not have authorized this client yet.");
                        return;
                    }

                    var relayData = BuildRelayData(relayManager, response);
                    transport.SetEdgegapRelayData(relayData);
                }

                transport.StartConnection(false);
            }
        }

        public override void StopClient()
        {
            var clientManager = InstanceFinder.ClientManager;

            if (clientManager != null && clientManager.Started)
                clientManager.StopConnection();
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
    }
}
