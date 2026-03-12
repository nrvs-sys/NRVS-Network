using FishNet;
using FishNet.Managing.Transporting;
using FishNet.Transporting.Multipass;
using FishNet.Transporting.UTP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
            var transport = (FishyUnityTransport)transportManager?.GetTransport(transportIndex);

            if (transport != null)
            {
                var hostAllocation = await CreateAllocation(maxPlayers);
                if (ct.IsCancellationRequested) return;

                var result = await GetJoinCode(hostAllocation.AllocationId);
                if (ct.IsCancellationRequested) return;

                joinCode.Value = result;

                onJoinCodeReceived?.Invoke(joinCode.Value);

                transport.SetRelayServerData(new RelayServerData(hostAllocation, "dtls"));

                transport.StartConnection(true);
            }

            if (!ct.IsCancellationRequested)
                serverConnectionState = ConnectionState.None;
        }

        public override void StopServer(bool sendDisconnectMessage = true)
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

            serverConnectionState = ConnectionState.None;
        }

        public async override void StartClient() => await StartClientAsync();

        public async Task StartClientAsync()
        {
            CancelAndResetCts(ref clientCts);
            var ct = clientCts.Token;

            clientConnectionState = ConnectionState.Starting;

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
                    if (ct.IsCancellationRequested) return;

                    transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
                }

                if (ct.IsCancellationRequested) return;

                transport.StartConnection(false);
            }

            if (!ct.IsCancellationRequested)
                clientConnectionState = ConnectionState.None;
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
