using FishNet.Managing;
using FishNet.Managing.Client;
using FishNet.Managing.Server;
using FishNet.Connection;
using FishNet;

namespace Network
{
    /// <summary>
    /// Fast, allocation-free helpers to resolve NetworkConnection from OwnerId and vice-versa.
    /// </summary>
    public static class ConnectionLookup
    {
        /// <summary>
        /// Get the NetworkConnection for an ownerId.
        /// </summary>
        public static NetworkConnection GetConnection(int ownerId)
        {
            if (ownerId < 0) 
                return null;

            var nm = InstanceFinder.NetworkManager;

            if (nm == null) 
                return null;

            if (nm.IsServerStarted && nm.ServerManager != null)
            {
                nm.ServerManager.Clients.TryGetValue(ownerId, out var conn);
                return conn;
            }

            if (nm.IsClientStarted && nm.ClientManager != null)
            {
                nm.ClientManager.Clients.TryGetValue(ownerId, out var conn);
                return conn;
            }

            return null;
        }

        public static bool TryGetConnection(int ownerId, out NetworkConnection conn)
        {
            conn = GetConnection(ownerId);
            return conn != null;
        }

        public static int GetOwnerId(NetworkConnection conn) => (conn == null) ? -1 : conn.ClientId;
    }
}
