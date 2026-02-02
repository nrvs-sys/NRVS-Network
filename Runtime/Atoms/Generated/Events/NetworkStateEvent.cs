using UnityEngine;
using Network;
using System;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `Network.NetworkState`. Inherits from `AtomEvent&lt;Network.NetworkState&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/NetworkState", fileName = "NetworkStateEvent")]
    public sealed class NetworkStateEvent : AtomEvent<Network.NetworkState>
    {
        public void RegisterListener(object onNetworkStateChanged)
        {
            throw new NotImplementedException();
        }
    }
}
