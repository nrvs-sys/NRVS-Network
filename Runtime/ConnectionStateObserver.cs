using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using System;
using FishNet.Managing.Scened;
using UnityAtoms.BaseAtoms;
using UnityEngine.Events;

namespace Network
{
    /// <summary>
    /// Observes the connection state of the server and client and updates the connection state variables.
    /// </summary>
    public class ConnectionStateObserver : MonoBehaviour
    {
        [SerializeField]
        private LocalConnectionStateVariable serverConnectionState;

        [SerializeField]
        private LocalConnectionStateVariable clientConnectionState;

        private NetworkManager networkManager;

        private void Awake()
        {
            networkManager = InstanceFinder.NetworkManager;

            networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }

        private void OnDestroy()
        {
            networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
            networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
        }

        private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
        {
            switch (obj.ConnectionState)
            {
                case LocalConnectionState.Stopped:
                    break;
                case LocalConnectionState.Starting:
                    break;
                case LocalConnectionState.Started:
                    break;
                case LocalConnectionState.Stopping:
                    break;
            }

            serverConnectionState?.SetValue(obj.ConnectionState);
        }

        private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
        {
            switch (obj.ConnectionState)
            {
                case LocalConnectionState.Stopped:
                    break;
                case LocalConnectionState.Starting:
                    break;
                case LocalConnectionState.Started:
                    break;
                case LocalConnectionState.Stopping:
                    break;
            }

            clientConnectionState?.SetValue(obj.ConnectionState);
        }
    }
}
