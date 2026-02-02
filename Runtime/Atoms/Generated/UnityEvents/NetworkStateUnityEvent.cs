using System;
using UnityEngine.Events;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// None generic Unity Event of type `Network.NetworkState`. Inherits from `UnityEvent&lt;Network.NetworkState&gt;`.
    /// </summary>
    [Serializable]
    public sealed class NetworkStateUnityEvent : UnityEvent<Network.NetworkState> { }
}
