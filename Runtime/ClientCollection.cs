using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public class ClientCollection<T> : IDisposable
    {
        public Dictionary<NetworkConnection, T> clients;

        Func<T> createClientData;

        public ClientCollection(Func<T> createClientData = null)
        {
            clients = new Dictionary<NetworkConnection, T>();
            
            this.createClientData = createClientData;

            var serverManager = InstanceFinder.ServerManager;

            if (serverManager != null)
            {
                serverManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

                foreach (var conn in serverManager.Clients.Values)
                {
                    AddClient(conn, createClientData != null ? createClientData.Invoke() : default);
                }
            }
        }

        private void ServerManager_OnRemoteConnectionState(NetworkConnection arg1, RemoteConnectionStateArgs arg2)
        {
            switch (arg2.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    AddClient(arg1, createClientData != null ? createClientData.Invoke() : default);
                    break;
                case RemoteConnectionState.Stopped:
                    RemoveClient(arg1);
                    break;
            }
        }

        void AddClient(NetworkConnection client, T data) => clients[client] = data;

        void RemoveClient(NetworkConnection client) => clients.Remove(client);

        public void Set(NetworkConnection client, T data)
        {
            if (clients.ContainsKey(client))
            {
                clients[client] = data;
            }
            else
            {
                AddClient(client, data);
            }
        }

        public T Get(NetworkConnection client)
        {
            if (clients.ContainsKey(client))
            {
                return clients[client];
            }
            else
            {
                return default;
            }
        }

        public bool TryGet(NetworkConnection client, out T data)
        {
            if (clients.ContainsKey(client))
            {
                data = clients[client];
                return true;
            }
            else
            {
                data = default;
                return false;
            }
        }

        public void Dispose()
        {
            clients.Clear();

            var serverManager = InstanceFinder.ServerManager;

            if (serverManager != null)
            {
                serverManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
            }
        }
    }
}
