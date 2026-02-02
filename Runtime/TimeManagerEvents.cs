using FishNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeManagerEvents : MonoBehaviour
{
	[Header("Events")]
	public UnityEvent onPreTick;
	public UnityEvent onTick;
	public UnityEvent onPostTick;
	public UnityEvent onPostReplicateReplay;

	private void Start()
	{
		var timeManager = InstanceFinder.TimeManager;
		if (timeManager != null)
		{
			timeManager.OnPreTick += TimeManager_OnPreTick;
			timeManager.OnTick += TimeManager_OnTick;
			timeManager.OnPostTick += TimeManager_OnPostTick;
		}

		var predictionManager = InstanceFinder.PredictionManager;
		if (predictionManager != null)
		{
            predictionManager.OnPostReplicateReplay += PredictionManager_OnPostReplicateReplay;
        }
	}

	private void OnDestroy()
	{
		var timeManager = InstanceFinder.TimeManager;
		if (timeManager != null)
		{
			timeManager.OnPreTick -= TimeManager_OnPreTick;
			timeManager.OnTick -= TimeManager_OnTick;
			timeManager.OnPostTick -= TimeManager_OnPostTick;
		}

        var predictionManager = InstanceFinder.PredictionManager;
        if (predictionManager != null)
        {
            predictionManager.OnPostReplicateReplay -= PredictionManager_OnPostReplicateReplay;
        }
    }

    #region Time Manager Callbacks

    private void TimeManager_OnPreTick() => onPreTick?.Invoke();

	private void TimeManager_OnTick() => onTick?.Invoke();

	private void TimeManager_OnPostTick() => onPostTick?.Invoke();

	private void PredictionManager_OnPostReplicateReplay(uint clientTick, uint serverTick) => onPostReplicateReplay?.Invoke();

    #endregion
}
