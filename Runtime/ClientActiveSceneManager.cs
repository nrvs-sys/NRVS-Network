using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Network
{
    /// <summary>
    /// Keeps track of the active scene on the server, and syncs it with the client.
    /// </summary>
    public class ClientActiveSceneManager : NetworkBehaviour
    {
         readonly SyncVar<string> serverActiveScene = new(new(sendRate: 0f));

        void OnEnable()
        {
            serverActiveScene.OnChange += OnServerActiveSceneChanged;

            UnitySceneManager.activeSceneChanged += UnitySceneManager_activeSceneChanged;
            UnitySceneManager.sceneLoaded += UnitySceneManager_sceneLoaded;
        }

        void OnDisable()
        {
            UnitySceneManager.activeSceneChanged -= UnitySceneManager_activeSceneChanged;
        }

        public override void OnStartServer()
        {
            serverActiveScene.Value = UnitySceneManager.GetActiveScene().name;

            base.OnStartServer();
        }

        void OnServerActiveSceneChanged(string prev, string next, bool asServer)
        {
            var newServerActiveScene = UnitySceneManager.GetSceneByName(next);

            if (!asServer && newServerActiveScene != null && newServerActiveScene.isLoaded)
            {
                Debug.Log($"[ClientActiveSceneManager] Changing active scene to {next}");
                UnitySceneManager.SetActiveScene(newServerActiveScene);
            }
        }

        void UnitySceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            if (IsServerInitialized)
                serverActiveScene.Value = arg1.name;
        }

        void UnitySceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {   
            if (IsClientOnlyInitialized && arg0.name == serverActiveScene.Value)
            {
                Debug.Log($"[ClientActiveSceneManager] Setting active scene to {arg0.name}");
                UnitySceneManager.SetActiveScene(arg0);
            }
        }
    }
}
