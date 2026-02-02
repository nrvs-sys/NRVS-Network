using FishNet;
using FishNet.Connection;
using FishNet.Object;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class NetworkEvents : NetworkBehaviour
{
	[Header("Events")]
	public UnityEvent onDespawnServer;
    public UnityEvent onSpawnServer;

    [Space(10)]

	public UnityEventBool onOwnershipClient;
	public UnityEvent onOwnershipServer;

    [Space(10)]

    public UnityEvent onStartClient;
	public UnityEvent onStartClientOnly;
	public UnityEvent onStartNetwork;
	public UnityEvent onStartServer;

	[Space(10)]

	public UnityEvent onStopClient;
	public UnityEvent onStopNetwork;
	public UnityEvent onStopServer;

	[Space(10)]

	public UnityEvent onSceneLoadEnd;
	public UnityEvent<NetworkConnection, bool> onClientLoadedStartScenes;

    public UnityEvent onSceneVisibleToAllClients;

    void Awake()
	{
		var sceneManager = InstanceFinder.SceneManager;
		if (sceneManager != null)
		{
			sceneManager.OnLoadEnd += SceneManager_OnLoadEnd;
            sceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        }

		if (Ref.TryGet(out NetworkSceneManager networkSceneManager))
		{
            networkSceneManager.OnSceneVisibleForAllClients.AddListener(NetworkSceneManager_OnSceneVisibleForAllClients);
			
			var sceneName = gameObject.scene.name;

            // If Scene is already Visible, Invoke immediately
            if (networkSceneManager.IsSceneVisibleToAllClients(sceneName))
                onSceneVisibleToAllClients?.Invoke();
        }
    }

    void OnDestroy()
    {
        var sceneManager = InstanceFinder.SceneManager;
        if (sceneManager != null)
        {
            sceneManager.OnLoadEnd -= SceneManager_OnLoadEnd;
            sceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        }

        if (Ref.TryGet(out NetworkSceneManager networkSceneManager))
        {
            networkSceneManager.OnSceneVisibleForAllClients.RemoveListener(NetworkSceneManager_OnSceneVisibleForAllClients);
        }
    }

    void SceneManager_OnLoadEnd(FishNet.Managing.Scened.SceneLoadEndEventArgs obj)
    {
		if (obj.LoadedScenes.Length > 0 && obj.LoadedScenes.First().name == gameObject.scene.name)
			onSceneLoadEnd?.Invoke();
    }

    void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer)
    {
        onClientLoadedStartScenes?.Invoke(conn, asServer);
    }

    void NetworkSceneManager_OnSceneVisibleForAllClients(Scene scene)
    {
        if (scene.name == gameObject.scene.name)
        {
            onSceneVisibleToAllClients?.Invoke();
        }
    }

    public override void OnDespawnServer(NetworkConnection connection)
	{
		base.OnDespawnServer(connection);

		onDespawnServer?.Invoke();
	}

	public override void OnOwnershipClient(NetworkConnection prevOwner)
	{
		base.OnOwnershipClient(prevOwner);

		onOwnershipClient?.Invoke(IsOwner);
	}

	public override void OnOwnershipServer(NetworkConnection prevOwner)
	{
		base.OnOwnershipServer(prevOwner);

		onOwnershipServer?.Invoke();
	}

	public override void OnSpawnServer(NetworkConnection connection)
	{
		base.OnSpawnServer(connection);

		onSpawnServer?.Invoke();
	}

	public override void OnStartClient()
	{
		base.OnStartClient();

		onStartClient?.Invoke();

		if (IsClientOnlyInitialized)
            onStartClientOnly?.Invoke();
	}

	public override void OnStartNetwork()
	{
		base.OnStartNetwork();

		onStartNetwork?.Invoke();
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		onStartServer?.Invoke();
	}

	public override void OnStopClient()
	{
		base.OnStopClient();

		onStopClient?.Invoke();
	}

	public override void OnStopNetwork()
	{
		base.OnStopNetwork();

		onStopNetwork?.Invoke();
	}

	public override void OnStopServer()
	{
		base.OnStopServer();

		onStopServer?.Invoke();
	}
}