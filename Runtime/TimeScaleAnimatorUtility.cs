using System;
using UnityEngine;

namespace Network
{
    public class TimeScaleAnimatorUtility : MonoBehaviour
    {
        Animator anim;
        float lastTimeScale = 1f;
        TimeScaleManager timeScaleManager;

        void Awake()
        {
            anim = GetComponent<Animator>();

            Ref.Instance.OnRegistered += Ref_OnRegistered;
            Ref.Instance.OnUnregistered += Ref_OnUnregistered;

            if (Ref.TryGet(out TimeScaleManager timeScaleManager))
            {
                this.timeScaleManager = timeScaleManager;
                anim.speed = timeScaleManager.timeScale;
            }
        }

        void OnDestroy()
        {
            if (Ref.Instance != null)
            {
                Ref.Instance.OnRegistered -= Ref_OnRegistered;
                Ref.Instance.OnUnregistered -= Ref_OnUnregistered;
            }
        }

        void Update()
        {
            if (timeScaleManager != null && timeScaleManager.timeScale != lastTimeScale)
            {
                anim.speed = lastTimeScale = timeScaleManager.timeScale;
            }
        }

        void Ref_OnRegistered(Type type, object instance)
        {
            TimeScaleManager timeScaleManager = instance as TimeScaleManager;
            if (timeScaleManager != null)
            {
                this.timeScaleManager = timeScaleManager;
                anim.speed = timeScaleManager.timeScale;
            }
        }

        void Ref_OnUnregistered(Type type, object instance)
        {
            if (instance as TimeScaleManager == timeScaleManager)
            {
                timeScaleManager = null;
                lastTimeScale = 1f;
                anim.speed = 1f;
            }
        }
    }
}
