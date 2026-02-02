using UnityEngine;
using System;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable of type `Network.NetworkState`. Inherits from `AtomVariable&lt;Network.NetworkState, NetworkStatePair, NetworkStateEvent, NetworkStatePairEvent, NetworkStateNetworkStateFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/NetworkState", fileName = "NetworkStateVariable")]
    public sealed class NetworkStateVariable : AtomVariable<Network.NetworkState, NetworkStatePair, NetworkStateEvent, NetworkStatePairEvent, NetworkStateNetworkStateFunction>
    {
        protected override bool ValueEquals(Network.NetworkState other) => Value == other;
    }
}
