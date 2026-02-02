using FishNet;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.UTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;


namespace Network.UGS
{
    /// <summary>
    /// Behavior to start and stop network connections through the UGS (Unity Game Service) Relay.
    /// </summary>
    [CreateAssetMenu(fileName = "Connection_ Relay_ UGS_ New", menuName = "Behaviors/Network/Connection/Relay/UGS")]
    public class RelayConnectionBehaviour : ConnectionBehaviour
    {
        [SerializeField]
        StringVariable joinCode;

        [SerializeField]
        int maxPlayers = 4;

        public UnityEvent<string> onJoinCodeReceived;

        public override async void StartHost()
        {
            await StartServerAsync();
            await StartClientAsync();
        }

        public async override void StartServer() => await StartServerAsync();

        public async Task StartServerAsync()
        {
            var transportManager = InstanceFinder.TransportManager;
            var transport = (FishyUnityTransport)transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                var hostAllocation = await CreateAllocation(maxPlayers);

                var result = await GetJoinCode(hostAllocation.AllocationId);

                joinCode.Value = result;

                onJoinCodeReceived?.Invoke(joinCode.Value);

                transport.SetRelayServerData(new RelayServerData(hostAllocation, "dtls"));

                transport.StartConnection(true);
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
            var transport = (FishyUnityTransport)transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                transportManager.GetTransport<Multipass>()?.SetClientTransport(transport);

                var serverManager = InstanceFinder.ServerManager;

                // No need to Set Relay Server Data if connecting as Host (IE if ServerManager is already started)
                if (serverManager == null || !serverManager.Started)
                {
                    JoinAllocation joinAllocation = await JoinAllocation(joinCode.Value);
                    transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
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

        #region Relay Service API

        async Task<Allocation> CreateAllocation(int maxConnections, string region = null)
        {
            try
            {
                Debug.Log($"Creating Relay Allocation");

                return await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }

            return null;
        }

        async Task<JoinAllocation> JoinAllocation(string joinCode)
        {
            try
            {
                Debug.Log($"Joining Relay Allocation: {joinCode}");

                return await RelayService.Instance.JoinAllocationAsync(joinCode);
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }

            return null;
        }

        async Task<string> GetJoinCode(Guid allocationID)
        {
            try
            {
                Debug.Log("Getting Relay Join Code");

                return await RelayService.Instance.GetJoinCodeAsync(allocationID);
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
            }

            return null;
        }

        #endregion
    }
}
