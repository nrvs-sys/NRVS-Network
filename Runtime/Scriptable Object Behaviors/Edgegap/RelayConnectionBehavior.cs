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

                var relayService = relayManager.GetRelayService();

                Debug.Log("Relay Connection: Creating session");
                var response = await relayService.CreateSessionAsync(clientIps);

                // Store session centrally
                relayManager.SetSession(response);

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

                onRelaySessionIdCreated?.Invoke(relayManager.RelaySessionId);
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
                    var relayManager = Ref.Get<RelayManager>();

                    if (string.IsNullOrEmpty(relayManager.RelaySessionId))
                    {
                        Debug.LogError("Relay Connection: No relay session ID available. Cannot start client.");
                        return;
                    }

                    // Check if this client is already authorized; if not, wait for host to authorize
                    if (!await WaitForAuthorizationAsync(relayManager))
                    {
                        Debug.LogError("Relay Connection: Client was not authorized on the relay session. Cannot connect.");
                        return;
                    }

                    var relayService = relayManager.GetRelayService();
                    var response = await relayService.JoinSessionAsync(relayManager.RelaySessionId);

                    //Convert uint? to uint
                    uint sessionAuthorizationToken = response.authorization_token ?? 0;
                    uint userAuthorizationToken = GetLocalSessionUser(response)?.authorization_token ?? 0;

                    if (userAuthorizationToken == 0)
                    {
                        Debug.LogError("Relay Connection: Client has no valid user authorization token. The host may not have authorized this client yet.");
                        return;
                    }

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

        /// <summary>
        /// Polls the relay session until the local player's IP appears as an authorized user, or until timeout.
        /// </summary>
        async Task<bool> WaitForAuthorizationAsync(RelayManager relayManager, float timeoutSeconds = 15f)
        {
            if (!Ref.TryGet(out Services.UGS.LobbyManager lobbyManager))
                return false;

            var localPlayer = lobbyManager.GetLocalPlayer();
            if (localPlayer == null) return false;

            var localIP = lobbyManager.GetPlayerDataValue(localPlayer, Constants.Services.Edgegap.LobbyPlayerDataKeys.PublicIp);
            if (string.IsNullOrEmpty(localIP))
            {
                Debug.LogError("Relay Connection: Local player has no public IP in lobby data.");
                return false;
            }

            // If the session is already cached and user is in it, skip polling
            if (relayManager.IsUserAuthorized(localIP))
                return true;

            // Poll the session from the API until authorized or timeout
            var relayService = relayManager.GetRelayService();
            float elapsed = 0f;
            float pollInterval = 1.5f;

            while (elapsed < timeoutSeconds)
            {
                await Task.Delay((int)(pollInterval * 1000));
                elapsed += pollInterval;

                try
                {
                    var session = await relayService.GetSessionAsync(relayManager.RelaySessionId);
                    relayManager.SetSession(session);

                    if (relayManager.IsUserAuthorized(localIP))
                    {
                        Debug.Log("Relay Connection: Client authorized on relay session.");
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Relay Connection: Error polling session for authorization: {e.Message}");
                }
            }

            return false;
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
