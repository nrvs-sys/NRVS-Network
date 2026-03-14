using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Spawn Owned Object_ New", menuName = "Behaviors/Spawning/Spawn Owned Object")]
public class SpawnOwnedObjectBehavior : ScriptableObject
{
    public enum SpawnSceneBehavior
    {
        /// <summary>
        /// Adds the spawned object to the active scene if no global scenes are specified through the SceneManager. If global scenes are specified, the object will be added to the first global scene. This uses the SceneManager.AddOwnerToDefaultScene() method.
        /// </summary>
        Default,
        /// <summary>
        /// Adds the spawned object to the same scene as the interacted object. If the interacted object is not in a valid scene, it will fall back to the Default behavior and add the spawned object to the active scene if no global scenes are specified through the SceneManager, or to the first global scene if global scenes are specified.
        /// </summary>
        SameAsInteractedObject,
    }

    [SerializeField]
    NetworkObject objectPrefab;

    [SerializeField, Tooltip("Determines which scene the object is spawned into.")]
    private SpawnSceneBehavior spawnSceneBehavior = SpawnSceneBehavior.Default;

    [SerializeField, Tooltip("True to spawn the object with the same transform components as the passed in object (For Network Objects")]
    bool copyTransform = true;

    [Space(10)]

    public UnityEvent<NetworkObject> onSpawned;


    public static NetworkObject Spawn(NetworkConnection conn, NetworkObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, SpawnSceneBehavior spawnSceneBehavior = SpawnSceneBehavior.Default, Scene scene = default)
    {
        var networkManager = InstanceFinder.NetworkManager;

        if (networkManager == null)
        {
            Debug.LogWarning($"Network Manager was not found so Object will not be spawned for connection {conn.ClientId}.");
            return null;
        }

        NetworkObject nob = networkManager.GetPooledInstantiated(
            prefab.PrefabId, 
            prefab.SpawnableCollectionId, 
            asServer: true, 
            position: position, 
            rotation: rotation, 
            scale: Vector3.one, 
            parent: parent, 
            options: FishNet.Utility.Performance.ObjectPoolRetrieveOption.MakeActive
            );

        Debug.Log($"Prefab spawned for client connection {conn.ClientId}.");

        switch (spawnSceneBehavior)
        {
            case SpawnSceneBehavior.Default:
                networkManager.ServerManager.Spawn(nob, conn, scene);
                networkManager.SceneManager.AddOwnerToDefaultScene(nob);
                break;
            case SpawnSceneBehavior.SameAsInteractedObject:
                if (scene.IsValid())
                    networkManager.ServerManager.Spawn(nob, conn, scene);
                else
                {
                    networkManager.ServerManager.Spawn(nob, conn);
                    networkManager.SceneManager.AddOwnerToDefaultScene(nob);
                }
                break;
        }
            

        return nob;
    }

    // TODO - support predicted spawning on clients
    public void Spawn()
    {
        var serverManager = InstanceFinder.ServerManager;
        
        if (serverManager == null)
        {
            Debug.LogWarning($"Server Manager was not found so Objects will not be spawned.");
            return;
        }

        foreach (var conn in serverManager.Clients.Values)
        {
            Spawn(conn);
        }
    }

    public void Spawn(NetworkConnection conn, bool asServer)
    {
        if (!asServer)
            return;

        Spawn(conn);
    }


    public void Spawn(NetworkConnection conn)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            //Debug.LogWarning($"Server is not started, cannot spawn object for {networkObject.name}.");
            return;
        }

        if (objectPrefab == null)
        {
            Debug.LogWarning($"Object Prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        var nob = Spawn(conn, objectPrefab, Vector3.zero, Quaternion.identity, spawnSceneBehavior: spawnSceneBehavior);
        
        if (nob != null)
            onSpawned?.Invoke(nob);
    }

    public void Spawn(NetworkConnection conn, Vector3 position, Quaternion rotation)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            //Debug.LogWarning($"Server is not started, cannot spawn object for {networkObject.name}.");
            return;
        }

        if (objectPrefab == null)
        {
            Debug.LogWarning($"Object Prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        var nob = Spawn(conn, objectPrefab, position, rotation, spawnSceneBehavior: spawnSceneBehavior);

        if (nob != null)
            onSpawned?.Invoke(nob);
    }

    public void Spawn(NetworkObject networkObject)
    {
        if (!InstanceFinder.IsServerStarted)
        {
            //Debug.LogWarning($"Server is not started, cannot spawn object for {networkObject.name}.");
            return;
        }

        if (networkObject == null)
        {
            Debug.LogWarning($"NetworkObject is null, cannot spawn object.");
            return;
        }

        var connection = networkObject.Owner;
        if (connection != null)
        {
            var nob = Spawn(connection, objectPrefab, copyTransform ? networkObject.transform.position : Vector3.zero, copyTransform ? networkObject.transform.rotation : Quaternion.identity, spawnSceneBehavior: spawnSceneBehavior, scene: networkObject.gameObject.scene);

            if (nob != null)
                onSpawned?.Invoke(nob);
        }
    }

    public void Spawn(DamageInfo damageInfo) => Spawn(damageInfo.interactionInfo.interactedGameObject.GetComponent<NetworkObject>());
}
