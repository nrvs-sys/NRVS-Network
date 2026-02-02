using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using FishNet.Transporting;
using Dissonance;
using Dissonance.Integrations.FishNet;

public class VoipManager : MonoBehaviour
{
    [SerializeField]
    private GameObject dissonanceCommsPrefab;

    [Space(10)]

    [SerializeField]
    private LocalConnectionStateEvent serverConnectionStateChangedEvent;

    [SerializeField]
    private LocalConnectionStateEvent clientConnectionStateChangedEvent;

    public DissonanceComms dissonanceComms;
    public bool IsMuted
    {
        get => dissonanceComms == null ? true : dissonanceComms.IsMuted;
        set
        {
            if (dissonanceComms != null)
                dissonanceComms.IsMuted = value;
        }
    }

    public bool isInitialized
	{
        get;
        private set;
	}

    public event InitializedDelegate OnInitialized;
    public delegate void InitializedDelegate();

    private Coroutine initializationCoroutine;

    private void Awake()
    {
        Ref.Register<VoipManager>(this);
    }

    private void Start()
    {
        serverConnectionStateChangedEvent?.Register(ServerConnectionState_Changed);
        clientConnectionStateChangedEvent?.Register(ClientConnectionState_Changed);
    }

    private void OnDestroy()
    {
        StopServer();
        StopClient();

        serverConnectionStateChangedEvent?.Unregister(ServerConnectionState_Changed);
        clientConnectionStateChangedEvent?.Unregister(ClientConnectionState_Changed);

        Ref.Unregister<VoipManager>(this);
    }

    private void StartServer() 
    {
        InitializeDissonance();
    }

    private void StartClient()
    {
        InitializeDissonance();
    }

    private void StopServer()
    {
        CleanupDissonanace();
    }

    private void StopClient()
    {
        CleanupDissonanace();
    }

    private void InitializeDissonance()
    {
        if (dissonanceComms == null && dissonanceCommsPrefab != null && initializationCoroutine == null)
		{
            initializationCoroutine = StartCoroutine(DoInitializeDissonance());
        }
    }

    private IEnumerator DoInitializeDissonance()
	{
        // Wait for the leaderboard platform to initialize so we can get a unique player id
        ILeaderboardPlatform leaderboardPlatform;

        while (!Ref.TryGet(out leaderboardPlatform))
            yield return null;

        while (!leaderboardPlatform.isLoggedIn && !leaderboardPlatform.hasLoginError)
            yield return null;

        // Instantiate the dissonance comms prefab and set the local player name to the player id.
        // IMPORTANT: dissonance requires a unique local player name, and the player name can only be set before DissonanceComms.Start is called
        var dissonanceGameObject = Instantiate(dissonanceCommsPrefab);
        if (dissonanceGameObject.TryGetComponent(out DissonanceComms comms))
		{
            comms.LocalPlayerName = leaderboardPlatform.playerID;

            dissonanceComms = comms;
        }
        else
		{
            Debug.LogError($"Voip Manager: There was no DissonanceComms object on the dissonance comms prefab! Unable to set its local player name.");
		}

        isInitialized = true;

        OnInitialized?.Invoke();

        initializationCoroutine = null;
    }

    private void CleanupDissonanace()
    {
        if (dissonanceComms != null)
            Destroy(dissonanceComms.gameObject);
    }

    private void ServerConnectionState_Changed(LocalConnectionState obj)
    {
        switch (obj)
        {
            case LocalConnectionState.Stopped:
                break;
            case LocalConnectionState.Starting:
                break;
            case LocalConnectionState.Started:
                StartServer();
                break;
            case LocalConnectionState.Stopping:
                StopServer();
                break;
        }
    }

    private void ClientConnectionState_Changed(LocalConnectionState obj)
    {
        switch (obj)
        {
            case LocalConnectionState.Stopped:
                break;
            case LocalConnectionState.Starting:
                break;
            case LocalConnectionState.Started:
                StartClient();
                break;
            case LocalConnectionState.Stopping:
                StopClient();
                break;
        }
    }
}
