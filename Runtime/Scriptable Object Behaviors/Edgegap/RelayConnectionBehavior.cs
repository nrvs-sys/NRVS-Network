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
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;


namespace Network.Edgegap
{
    /// <summary>
    /// Behavior to start and stop network connections through the UGS (Unity Game Service) Relay.
    /// </summary>
    [CreateAssetMenu(fileName = "Connection_ Relay_ Edgegap_ New", menuName = "Behaviors/Network/Connection/Relay/Edgegap")]
    public class RelayConnectionBehaviour : ConnectionBehaviour
    {
        [SerializeField]
        string relayProfileToken;

        [SerializeField]
        StringVariable relaySessionId;

        [Tooltip("Invoked when on the Lobby host when they create a new session.")]
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

                Debug.Log("Relay Connection: Lobby host, getting ips");

                var result = await lobbyIpLookup.GetLobbyIpsAsync();
                var clientIps = result.lobbyIps;

                // Debug.Log($"Relay Connection: Client IPs: {string.Join(", ", clientIps)}");

                var relayService = new RelayService(relayProfileToken);
                var relayManager = Ref.Get<RelayManager>();

                Debug.Log("Relay Connection: Creating session");
                var response = await relayService.CreateSessionAsync(clientIps);

                //Convert uint? to uint
                uint sessionAuthorizationToken = response.authorization_token ?? 0;
                uint userAuthorizationToken = GetLocalSessionUser(response)?.authorization_token ?? 0;

                var relay = response.relay;
                string address = relay.ip;
                ushort serverPort = relay.ports.server.port;
                ushort clientPort = relay.ports.client.port;
                var relayData = new EdgegapRelayData(address, serverPort, clientPort, userAuthorizationToken, sessionAuthorizationToken);

                Debug.Log("Relay Connection: Relay Session Started. Starting server connection");

                transport.SetEdgegapRelayData(relayData);

                transport.StartConnection(true);

                relaySessionId.Value = response.session_id;

                relayManager.StartRelaySessionOnClients(relaySessionId.Value);

                onRelaySessionIdCreated?.Invoke(relaySessionId.Value);
            }
        }

        public override void StopServer(bool sendDisconnectMessage = true)
        {
            var serverManager = InstanceFinder.ServerManager;

            if (serverManager == null || !serverManager.Started)
                return;

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

                // No need to Set Relay Server Data if connecting as Host (IE if ServerManager is already started)
                if (serverManager == null || !serverManager.Started)
                {
                    var relayService = new RelayService(relayProfileToken);

                    var response = await relayService.JoinSessionAsync(relaySessionId.Value);

                    //Convert uint? to uint
                    uint sessionAuthorizationToken = response.authorization_token ?? 0;
                    uint userAuthorizationToken = GetLocalSessionUser(response)?.authorization_token ?? 0;

                    var relay = response.relay;
                    string address = relay.ip;
                    ushort serverPort = relay.ports.server.port;
                    ushort clientPort = relay.ports.client.port;
                    var relayData = new EdgegapRelayData(address, serverPort, clientPort, userAuthorizationToken, sessionAuthorizationToken);

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

        RelayService.SessionUser GetLocalSessionUser(RelayService.SessionResponse response)
        {
            if (response.session_users == null || response.session_users.Count == 0)
            {
                Debug.LogError("Relay Connection: No session users found in the response.");
                return null;
            }

            if (Ref.TryGet(out Services.UGS.LobbyManager lobbyManager))
            {
                // Get the local player's public IP from the lobby data
                var localIP = lobbyManager.GetPlayerDataValue(lobbyManager.GetLocalPlayer(), Constants.Services.Edgegap.LobbyPlayerDataKeys.PublicIp);
                // Use it to find the local player's index in the session users
                int localPlayerIndex = response.session_users.FindIndex(user => user.ip_address == localIP);
                if (localPlayerIndex >= 0)
                {
                    // Set the local player's authorization token
                    return response.session_users[localPlayerIndex];
                }
                else
                {
                    Debug.LogError("Relay Connection: Local player not found in session users.");
                }
            }

            return null;
        }
    }
}
