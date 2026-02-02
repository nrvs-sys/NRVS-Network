using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    public interface IPredictedEvent
    {
        void Reset();
    }

    [System.Serializable]
    public class PredictedEvent : IPredictedEvent
    {
        public UnityEvent onInitialInvoke;
        public UnityEvent onInvoke;
        public UnityEvent onInvokeReplayed;

        bool hasInvoked = false;
        public void Invoke()
        {
            if (!hasInvoked)
            {
                onInitialInvoke.Invoke();
                hasInvoked = true;
            }
            else
            {
                onInvokeReplayed.Invoke();
            }
            onInvoke.Invoke();
        }

        public void Reset()
        {
            hasInvoked = false;
        }
    }

    [System.Serializable]
    public class PredictedEvent<T> : IPredictedEvent
    {
        public UnityEvent<T> onInitialInvoke;
        public UnityEvent<T> onInvoke;
        public UnityEvent<T> onInvokeReplayed;

        bool hasInvoked = false;

        public void Invoke(T value)
        {
            if (!hasInvoked)
            {
                onInitialInvoke.Invoke(value);
                hasInvoked = true;
            }
            else
            {
                onInvokeReplayed.Invoke(value);
            }

            onInvoke.Invoke(value);
        }

        public void Reset()
        {
            hasInvoked = false;
        }
    }
}
