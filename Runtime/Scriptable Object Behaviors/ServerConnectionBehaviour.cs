using FishNet;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
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

        public override async void StartHost()
        {
            await StartServerAsync();
            await StartClientAsync();
        }

        public override void StartServer() => _ = StartServerAsync();

        public override void StopServer(bool sendDisconnectMessage = true)
        {
            var transportManager = InstanceFinder.TransportManager;
            var transport = transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                transportManager.GetTransport<Multipass>()?.StopServerConnection(sendDisconnectMessage, transportIndex);
            }
        }

        public override void StartClient() => _ = StartClientAsync();

        public override void StopClient()
        {
            var clientManager = InstanceFinder.ClientManager;

            if (clientManager != null && clientManager.Started)
                clientManager.StopConnection();
        }

        async Task StartServerAsync()
        {
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
            while (serverManager == null || !serverManager.Started)
                await System.Threading.Tasks.Task.Yield();
        }

        async Task StartClientAsync()
        {
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
            while (clientManager == null || !clientManager.Started)
                await System.Threading.Tasks.Task.Yield();
        }
    }
}
