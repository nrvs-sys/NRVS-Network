using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[DefaultExecutionOrder(-1000)]
public class InternetConnectivityManager : MonoBehaviour
{
    [Header("Probe Settings")]
    [Tooltip("Seconds between checks.")]
    [SerializeField] private float checkIntervalSeconds = 10f;

    [Tooltip("HTTP timeout per request.")]
    [SerializeField] private int timeoutSeconds = 3;

    [Tooltip("If set, we'll also probe this service and (optionally) require it.")]
    [SerializeField] private string serviceHealthUrl = "";

    [Tooltip("If true, internet is considered available ONLY if serviceHealthUrl returns 2xx.")]
    [SerializeField] private bool requireServiceReachable = false;

    [Header("Debug Tools")]

    [Tooltip("Log state changes.")]
    [SerializeField] private bool logChanges = false;

    [SerializeField] private bool simulateOffline = false;

    // well-known 204 endpoints; add your own domain if you prefer
    private readonly string[] internetProbes = new[]
    {
        "https://www.gstatic.com/generate_204",
        "https://cp.cloudflare.com/generate_204",
    };

    private Coroutine monitorLoop;
    private bool? lastReported; // null = unknown

    private void Start()
    {
        if (monitorLoop == null) monitorLoop = StartCoroutine(Monitor());
    }

    private void OnDestroy()
    {
        if (monitorLoop != null) { StopCoroutine(monitorLoop); monitorLoop = null; }
    }

    // Recheck internet status when the app regains focus in case wifi settings were changed
	private void OnApplicationFocus(bool focus)
	{
        if (focus)
            CheckNow();
	}

	/// <summary>Runs a check immediately (without waiting for the interval).</summary>
	public void CheckNow()
    {
        StartCoroutine(CheckAndPublish());
    }

    private IEnumerator Monitor()
    {
        // do an immediate check on boot
        yield return CheckAndPublish();

        var wait = new WaitForSeconds(checkIntervalSeconds);
        while (true)
        {
            yield return wait;
            yield return CheckAndPublish();
        }
    }

    private IEnumerator CheckAndPublish()
    {
#if UNITY_EDITOR
        if (simulateOffline)
        {
            Publish(false);
            yield break;
        }
#endif

        // 0) quick hint
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Publish(false);
            yield break;
        }

        // 1) general internet probe (expect HTTP 204)
        bool hasInternet = false;
        bool captivePortalSuspected = false;

        for (int i = 0; i < internetProbes.Length; i++)
        {
            using (var req = UnityWebRequest.Get(internetProbes[i]))
            {
                req.timeout = timeoutSeconds;
                req.downloadHandler = new DownloadHandlerBuffer();
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    if (req.responseCode == 204)
                    {
                        hasInternet = true;
                        break;
                    }
                    else
                    {
                        // A 200/3xx from a 204 endpoint often means captive portal/walled garden
                        captivePortalSuspected = true;
                    }
                }
                // on network/SSL error, try next probe
            }
        }

        if (!hasInternet)
        {
            // we don’t expose walled-garden separately; treat as offline for app logic
            Publish(false);
            yield break;
        }

        // 2) optional service probe
        if (!string.IsNullOrEmpty(serviceHealthUrl))
        {
            using (var req = UnityWebRequest.Get(serviceHealthUrl))
            {
                req.timeout = timeoutSeconds;
                req.downloadHandler = new DownloadHandlerBuffer();
                yield return req.SendWebRequest();

                bool serviceOk = req.result == UnityWebRequest.Result.Success && (req.responseCode >= 200 && req.responseCode < 300);
                if (requireServiceReachable)
                {
                    Publish(serviceOk);
                    yield break;
                }
                else
                {
                    // internet is up regardless; we ignore service failure for the boolean,
                    // but you could log or expose a separate flag if needed.
                    Publish(true);
                    yield break;
                }
            }
        }

        Publish(true);
    }

    private void Publish(bool available)
    {
        if (lastReported.HasValue && lastReported.Value == available)
            return;

        lastReported = available;

        ApplicationInfo.internetAvailabilityStatus = available ? ApplicationInfo.InternetAvailabilityStatus.Online : ApplicationInfo.InternetAvailabilityStatus.Offline;

        if (logChanges)
            Debug.Log($"InternetConnectivityManager: InternetAvailable = {available}");
    }
}