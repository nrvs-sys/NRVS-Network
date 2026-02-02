using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    [CreateAssetMenu(fileName = "Time Scale_ New", menuName = "Behaviors/Time Scale")]
    public class TimeScaleBehaviour : ScriptableObject
    {
        [SerializeField]
        float timeScale = 1.0f;
        [SerializeField]
        float transitionDuration = 0.0f;

        [Button]
        public void SetTimeScale() => SetTimeScale(timeScale);
        [Button]
        public void TransitionTimeScale() => TransitionTimeScale(timeScale, transitionDuration);

        public void SetTimeScale(float timeScale) => Ref.Get<TimeScaleManager>()?.SetTimeScale(timeScale);
        public void TransitionTimeScale(float timeScale, float duration) => Ref.Get<TimeScaleManager>()?.TransitionTimeScale(timeScale, duration);

        [Button]
        public void BeginSystemPause() => Ref.Get<TimeScaleManager>()?.BeginSystemPause();

        [Button]
        public void EndSystemPause() => Ref.Get<TimeScaleManager>()?.EndSystemPause();

        public void ToggleSystemPause() => Ref.Get<TimeScaleManager>()?.ToggleSystemPause();
    }
}
