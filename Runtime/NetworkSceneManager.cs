using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using Scene = UnityEngine.SceneManagement.Scene;

namespace Network
{
    /// <summary>
    /// Tracks networked scenes and per-client visibility/load state.
    /// On server stop, all remaining local networked scenes are unloaded.
    /// </summary>
    public class NetworkSceneManager : NetworkBehaviour
    {
        readonly SyncList<string> serverNetworkedSceneNames = new(new SyncTypeSettings(0f));

        readonly List<Scene> localNetworkedScenes = new();

        /// <summary> Scene visibility per client (keyed by OwnerId). </summary>
        readonly SyncDictionary<int, List<string>> clientSceneVisibilities = new(new SyncTypeSettings(0f));

        /// <summary> Scene loaded-state per client (keyed by OwnerId). </summary>
        readonly SyncDictionary<int, List<string>> clientSceneLoaded = new(new SyncTypeSettings(0f));

        [Header("Server Events")]
        [Tooltip("Called when a client has scene visibility. Server only.")]
        public UnityEvent<Scene, NetworkConnection> OnSceneVisibleForClient;
        [Tooltip("Called when a client has finished loading a scene. Server only.")]
        public UnityEvent<Scene, NetworkConnection> OnSceneLoadedForClient;

        [Tooltip("Called when all clients have visibility for a scene. Server only.")]
        public UnityEvent<Scene> OnSceneVisibleForAllClients;
        [Tooltip("Called when all clients have loaded a scene. Server only.")]
        public UnityEvent<Scene> OnSceneLoadedForAllClients;

        #region Unity Methods

        void Awake()
        {
            serverNetworkedSceneNames.OnChange += NetworkSceneNames_OnChange;
            Ref.Register<NetworkSceneManager>(this);
        }

        void OnDestroy()
        {
            serverNetworkedSceneNames.OnChange -= NetworkSceneNames_OnChange;
            Ref.Unregister<NetworkSceneManager>(this);
            UnloadLocalNetworkedScenes();
        }

        #endregion

        #region NetworkBehaviour Methods

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            SceneManager.OnLoadEnd += Network_SceneManager_OnLoadEnd;
            SceneManager.OnUnloadEnd += Network_SceneManager_OnUnloadEnd;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            UnloadLocalNetworkedScenes();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            // Seed existing clients
            foreach (var kv in ServerManager.Clients)
                AddClient(kv.Value);

            ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

            SceneManager.OnLoadEnd += Server_SceneManager_OnLoadEnd;
            SceneManager.OnUnloadEnd += Server_SceneManager_OnUnloadEnd;
            SceneManager.OnClientPresenceChangeStart += Server_SceneManager_OnClientPresenceChangeStart;

            UnitySceneManager.sceneUnloaded += Server_UnitySceneManager_sceneUnloaded;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

            ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;

            SceneManager.OnLoadEnd -= Server_SceneManager_OnLoadEnd;
            SceneManager.OnUnloadEnd -= Server_SceneManager_OnUnloadEnd;
            SceneManager.OnClientPresenceChangeStart -= Server_SceneManager_OnClientPresenceChangeStart;

            UnitySceneManager.sceneUnloaded -= Server_UnitySceneManager_sceneUnloaded;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SceneManager.OnLoadEnd += Client_SceneManager_OnLoadEnd;
            SceneManager.OnUnloadEnd += Client_SceneManager_OnUnloadEnd;

            UnitySceneManager.sceneUnloaded += Client_UnitySceneManager_sceneUnloaded;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            SceneManager.OnLoadEnd -= Client_SceneManager_OnLoadEnd;
            SceneManager.OnUnloadEnd -= Client_SceneManager_OnUnloadEnd;

            UnitySceneManager.sceneUnloaded -= Client_UnitySceneManager_sceneUnloaded;
        }

        #endregion

        #region RPC Methods

        [ServerRpc(RequireOwnership = false)]
        void RpcClientConfirmSceneLoaded(string sceneName, NetworkConnection conn = null)
        {
            int id = conn?.ClientId ?? -1;
            if (id < 0) return;

            var scene = UnitySceneManager.GetSceneByName(sceneName);

            if (!clientSceneLoaded.ContainsKey(id))
            {
                clientSceneLoaded.Add(id, new List<string>());
                OnSceneLoadedForClient?.Invoke(scene, conn);
            }

            Debug.Log($"Networked Scene Manager: Client {id} loaded networked scene: {sceneName}");

            var list = clientSceneLoaded[id];
            if (!list.Contains(sceneName))
            {
                list.Add(sceneName);
                clientSceneLoaded.Dirty(id);
            }

            if (IsSceneLoadedOnAllClients(sceneName))
            {
                Debug.Log($"Networked Scene Manager: All Clients have loaded networked scene: {sceneName}");
                OnSceneLoadedForAllClients?.Invoke(scene);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void RpcClientSceneConfirmUnloaded(string sceneName, NetworkConnection conn = null)
        {
            int id = conn?.ClientId ?? -1;
            if (id < 0) return;

            if (clientSceneLoaded.TryGetValue(id, out var list))
            {
                if (list.Remove(sceneName))
                {
                    Debug.Log($"Networked Scene Manager: Client {id} unloaded networked scene: {sceneName}");
                    clientSceneLoaded.Dirty(id);
                }
            }
        }

        #endregion

        #region Queries / QOL

        public bool IsSceneVisibleToClient(string sceneName, int ownerId)
        {
            return clientSceneVisibilities.TryGetValue(ownerId, out var visList)
                && visList != null
                && visList.Contains(sceneName);
        }

        // Shim for existing call sites
        public bool IsSceneVisibleToClient(string sceneName, NetworkConnection conn)
            => IsSceneVisibleToClient(sceneName, conn == null ? -1 : conn.ClientId);

        [Client]
        public bool IsSceneVisibleOnLocalClient(string sceneName)
            => IsClientStarted && IsSceneVisibleToClient(sceneName, ClientManager.Connection);

        public bool IsSceneLoadedOnClient(string sceneName, int ownerId)
        {
            return clientSceneLoaded.TryGetValue(ownerId, out var loadList)
                && loadList != null
                && loadList.Contains(sceneName);
        }

        // Shim
        public bool IsSceneLoadedOnClient(string sceneName, NetworkConnection conn)
            => IsSceneLoadedOnClient(sceneName, conn == null ? -1 : conn.ClientId);

        public bool IsSceneVisibleToAllClients(string sceneName)
        {
            if (!IsServerInitialized && !IsClientInitialized)
                return false;

            if (IsServerInitialized)
            {
                // Expect exactly the connected clients
                if (clientSceneVisibilities.Count != ServerManager.Clients.Count)
                    return false;

                foreach (var kv in clientSceneVisibilities)
                    if (!kv.Value.Contains(sceneName)) return false;

                return true;
            }
            else // client-side check (ShareIds may be on)
            {
                int expected = ClientManager.Clients.Count;
                int actual = 0;

                foreach (var kv in clientSceneVisibilities)
                    if (ClientManager.Clients.ContainsKey(kv.Key)) actual++;

                if (actual != expected) 
                    return false;

                foreach (var kv in clientSceneVisibilities)
                {
                    if (!ClientManager.Clients.ContainsKey(kv.Key)) continue;
                    if (!kv.Value.Contains(sceneName)) return false;
                }
                return true;
            }
        }

        public bool IsSceneLoadedOnAllClients(string sceneName)
        {
            if (!IsServerInitialized && !IsClientInitialized)
                return false;

            if (IsServerInitialized)
            {
                if (clientSceneLoaded.Count != ServerManager.Clients.Count)
                    return false;

                foreach (var kv in clientSceneLoaded)
                {
                    var list = kv.Value;
                    if (list == null || !list.Contains(sceneName))
                    {
                        var warn = $"Networked Scene Manager: Scene {sceneName} not loaded on client: {kv.Key}. Scenes Loaded on client:";
                        if (list != null)
                            for (int i = 0; i < list.Count; i++) warn += $"\n* {list[i]}";
                        Debug.LogWarning(warn);
                        return false;
                    }
                }
                return true;
            }
            else // client-side expectation
            {
                int expected = ClientManager.Clients.Count;
                int actual = 0;

                foreach (var kv in clientSceneLoaded)
                    if (ClientManager.Clients.ContainsKey(kv.Key)) actual++;

                if (actual != expected) return false;

                foreach (var kv in clientSceneLoaded)
                {
                    if (!ClientManager.Clients.ContainsKey(kv.Key)) continue;

                    var list = kv.Value;
                    if (list == null || !list.Contains(sceneName)) return false;
                }
                return true;
            }
        }

        public bool IsNetworkedScene(Scene scene) => localNetworkedScenes.Contains(scene);
        public Scene[] GetNetworkedScenes() => localNetworkedScenes.ToArray();

        #endregion

        #region Client Registration

        void AddClient(NetworkConnection conn)
        {
            if (conn == null) return;
            int id = conn.ClientId;
            if (id < 0) return;

            if (!clientSceneVisibilities.ContainsKey(id))
                clientSceneVisibilities.Add(id, new List<string>());

            if (!clientSceneLoaded.ContainsKey(id))
                clientSceneLoaded.Add(id, new List<string>());
        }

        void RemoveClient(NetworkConnection conn)
        {
            if (conn == null) return;
            int id = conn.ClientId;
            if (id < 0) return;

            clientSceneVisibilities.Remove(id);
            clientSceneLoaded.Remove(id);
        }

        #endregion

        void UnloadLocalNetworkedScenes()
        {
            SceneManager.OnLoadEnd -= Network_SceneManager_OnLoadEnd;
            SceneManager.OnUnloadEnd -= Network_SceneManager_OnUnloadEnd;

            for (int i = 0; i < localNetworkedScenes.Count; i++)
            {
                var s = localNetworkedScenes[i];
                if (s.IsValid() && s.isLoaded)
                {
                    Debug.Log($"Networked Scene Manager: Unloading local networked scene: {s.name}");
                    UnitySceneManager.UnloadSceneAsync(s);
                }
            }

            localNetworkedScenes.Clear();
        }

        void NetworkSceneNames_OnChange(SyncListOperation op, int index, string oldItem, string newItem, bool asServer)
        {
            switch (op)
            {
                case SyncListOperation.Add:
                    Debug.Log($"Networked Scene Manager: Networked Scene Loaded on Server: {newItem}");
                    break;
                case SyncListOperation.RemoveAt:
                    Debug.Log($"Networked Scene Manager: Networked Scene Unloaded on Server: {oldItem}");
                    break;
                case SyncListOperation.Clear:
                    Debug.Log("Networked Scene Manager: All Networked Scenes Unloaded on Server");
                    break;
            }
        }

        #region Server Events

        void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            switch (args.ConnectionState)
            {
                case RemoteConnectionState.Started:
                    AddClient(conn);
                    break;
                case RemoteConnectionState.Stopped:
                    RemoveClient(conn);
                    break;
            }
        }

        void Server_SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
        {
            foreach (var scene in args.LoadedScenes)
                if (!serverNetworkedSceneNames.Contains(scene.name))
                    serverNetworkedSceneNames.Add(scene.name);
        }

        void Server_SceneManager_OnUnloadEnd(SceneUnloadEndEventArgs args)
        {
            foreach (var unloaded in args.UnloadedScenesV2)
                serverNetworkedSceneNames.Remove(unloaded.Name);
        }

        void Server_UnitySceneManager_sceneUnloaded(Scene scene)
        {
            serverNetworkedSceneNames.Remove(scene.name);
        }

        /// <summary>
        /// Called when a client's scene visibility changes on the server.
        /// </summary>
        void Server_SceneManager_OnClientPresenceChangeStart(ClientPresenceChangeEventArgs args)
        {
            var connection = args.Connection;
            var scene = args.Scene;
            var sceneName = scene.name;

            int id = connection.ClientId;

            if (id < 0) 
                return;

            if (!clientSceneVisibilities.ContainsKey(id))
                clientSceneVisibilities.Add(id, new List<string>());

            if (args.Added)
            {
                Debug.Log($"Networked Scene Manager: Client {id} gained visibility in networked scene: {sceneName}");
                var list = clientSceneVisibilities[id];
                if (!list.Contains(sceneName))
                {
                    list.Add(sceneName);
                    clientSceneVisibilities.Dirty(id);
                }

                OnSceneVisibleForClient?.Invoke(scene, connection);
            }
            else
            {
                Debug.Log($"Networked Scene Manager: Client {id} lost visibility in networked scene: {sceneName}");
                if (clientSceneVisibilities.TryGetValue(id, out var list))
                {
                    if (list.Remove(sceneName))
                        clientSceneVisibilities.Dirty(id);
                }
            }

            if (IsSceneVisibleToAllClients(sceneName))
            {
                Debug.Log($"Networked Scene Manager: All Clients have visibility in networked scene: {sceneName}");
                OnSceneVisibleForAllClients?.Invoke(scene);
            }
        }

        #endregion

        #region Scene Tracking Methods

        void Network_SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
        {
            foreach (var scene in args.LoadedScenes)
                if (!localNetworkedScenes.Contains(scene))
                    localNetworkedScenes.Add(scene);
        }

        void Network_SceneManager_OnUnloadEnd(SceneUnloadEndEventArgs args)
        {
            foreach (var unloaded in args.UnloadedScenesV2)
                localNetworkedScenes.Remove(unloaded.GetScene());
        }

        void Client_SceneManager_OnLoadEnd(SceneLoadEndEventArgs args)
        {
            foreach (var scene in args.LoadedScenes)
            {
                Debug.Log($"Networked Scene Manager: Local client loaded networked scene: {scene.name}");
                RpcClientConfirmSceneLoaded(scene.name);
            }
        }

        void Client_SceneManager_OnUnloadEnd(SceneUnloadEndEventArgs args)
        {
            foreach (var unloaded in args.UnloadedScenesV2)
            {
                Debug.Log($"Networked Scene Manager: Local client unloaded networked scene: {unloaded.Name}");
                RpcClientSceneConfirmUnloaded(unloaded.Name);
            }
        }

        void Client_UnitySceneManager_sceneUnloaded(Scene scene)
        {
            RpcClientSceneConfirmUnloaded(scene.name);
        }

        #endregion
    }
}
