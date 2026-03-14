using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NRVS.Network
{
    public class SpawnOwnedObjectUtility : MonoBehaviour
    {
        [SerializeField]
        NetworkObject networkObjectPrefab;

        [SerializeField]
        SpawnOwnedObjectBehavior.SpawnSceneBehavior spawnSceneBehavior = SpawnOwnedObjectBehavior.SpawnSceneBehavior.Default;

        [SerializeField]
        List<Transform> spawnTransforms = new();

        NetworkManager networkManager;
        int nextSpawnIndex = 0;

        void Awake()
        {
            networkManager = InstanceFinder.NetworkManager;

            if (networkManager == null)
            {
                Debug.LogWarning($"Network Manager was not found so Object will not be spawned for connections.");
                return;
            }

            networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }

        private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;

            if (networkObjectPrefab == null)
            {
                Debug.LogWarning($"Network Object Prefab was not assigned so Object will not be spawned for connection {conn.ClientId}.");
                return;
            }

            Transform spawnTransform = spawnTransforms.Count > 0 ? spawnTransforms[nextSpawnIndex] : null;
            Vector3 spawnPosition = spawnTransform != null ? spawnTransform.position : Vector3.zero;
            Quaternion spawnRotation = spawnTransform != null ? spawnTransform.rotation : Quaternion.identity;

            SpawnOwnedObjectBehavior.Spawn(conn, networkObjectPrefab, spawnPosition, spawnRotation, parent: null, spawnSceneBehavior: spawnSceneBehavior, scene: gameObject.scene);

            if (spawnTransforms.Count > 0)
                nextSpawnIndex = (nextSpawnIndex + 1) % spawnTransforms.Count;
        }
    }
}
