using FishNet;
using FishNet.Connection;
using FishNet.Transporting;
using System;
using System.Collections.Generic;

namespace Network
{
    /// <summary>
    /// Server-side collection keyed by OwnerId (ClientId). Automatically adds/removes
    /// entries as remote connections start/stop. Ideal when you’ve standardized on int IDs.
    /// </summary>
    public class ClientCollectionById<T> : IDisposable
    {
        public readonly Dictionary<int, T> byId;
        private readonly Func<T> _createClientData;

        public ClientCollectionById(Func<T> createClientData = null)
        {
            byId = new Dictionary<int, T>();
            _createClientData = createClientData;

            var serverManager = InstanceFinder.ServerManager;
            if (serverManager != null)
            {
                serverManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

                // Seed existing clients
                foreach (var conn in serverManager.Clients.Values)
                    AddByConnection(conn, _createClientData != null ? _createClientData() : default);
            }
        }

        private static int IdOf(NetworkConnection conn) => conn == null ? -1 : conn.ClientId;

        private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    AddByConnection(conn, _createClientData != null ? _createClientData() : default);
                    break;
                case RemoteConnectionState.Stopped:
                    RemoveById(args.ConnectionId);
                    break;
            }
        }

        private void AddByConnection(NetworkConnection conn, T data)
        {
            int id = IdOf(conn);
            if (id >= 0) byId[id] = data; // idempotent add/update
        }

        private void RemoveByConnection(NetworkConnection conn)
        {
            int id = IdOf(conn);
            if (id >= 0) byId.Remove(id);
        }

        private void RemoveById(int ownerId)
        {
            if (ownerId >= 0) byId.Remove(ownerId);
        }

        public void Set(int ownerId, T data)
        {
            if (ownerId >= 0) byId[ownerId] = data;
        }

        public bool TryGet(int ownerId, out T data)
        {
            return byId.TryGetValue(ownerId, out data);
        }

        public T Get(int ownerId)
        {
            return byId.TryGetValue(ownerId, out var data) ? data : default;
        }

        public bool Contains(int ownerId) => byId.ContainsKey(ownerId);

        public void Dispose()
        {
            byId.Clear();
            var serverManager = InstanceFinder.ServerManager;
            if (serverManager != null)
                serverManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }
    }
}
