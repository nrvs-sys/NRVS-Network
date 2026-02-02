using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DespawnAfterDelay : NetworkBehaviour
{
    [Header("Settings")]
    public float delay = 5.0f;

    public bool useUnscaledTime = false;

    private Coroutine delayCoroutine;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);

        delayCoroutine = StartCoroutine(DoDelay());
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (delayCoroutine != null)
            StopCoroutine(delayCoroutine);

        delayCoroutine = null;
    }

    private IEnumerator DoDelay()
    {
        if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
        else yield return new WaitForSeconds(delay);

        if (IsServerInitialized)
        {
            base.Despawn();
        }
    }
}
