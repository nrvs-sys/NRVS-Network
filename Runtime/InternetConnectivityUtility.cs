using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InternetConnectivityUtility : MonoBehaviour
{
	public UnityEvent<bool> onInternetAvailabilityChanged;


	private void OnEnable()
	{
		ApplicationInfo.OnInternetAvailabilityChanged += ApplicationInfo_OnInternetAvailabilityChanged;

		ApplicationInfo_OnInternetAvailabilityChanged(ApplicationInfo.internetAvailabilityStatus);
	}

	private void OnDisable()
	{
		ApplicationInfo.OnInternetAvailabilityChanged -= ApplicationInfo_OnInternetAvailabilityChanged;
	}


	private void ApplicationInfo_OnInternetAvailabilityChanged(ApplicationInfo.InternetAvailabilityStatus status)
	{
		bool isInternetAvailable = status == ApplicationInfo.InternetAvailabilityStatus.Online;

		onInternetAvailabilityChanged?.Invoke(isInternetAvailable);
	}
}