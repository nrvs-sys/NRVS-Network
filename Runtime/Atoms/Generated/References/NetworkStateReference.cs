using System;
using UnityAtoms.BaseAtoms;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Reference of type `Network.NetworkState`. Inherits from `AtomReference&lt;Network.NetworkState, NetworkStatePair, NetworkStateConstant, NetworkStateVariable, NetworkStateEvent, NetworkStatePairEvent, NetworkStateNetworkStateFunction, NetworkStateVariableInstancer, AtomCollection, AtomList&gt;`.
    /// </summary>
    [Serializable]
    public sealed class NetworkStateReference : AtomReference<
        Network.NetworkState,
        NetworkStatePair,
        NetworkStateConstant,
        NetworkStateVariable,
        NetworkStateEvent,
        NetworkStatePairEvent,
        NetworkStateNetworkStateFunction,
        NetworkStateVariableInstancer>, IEquatable<NetworkStateReference>
    {
        public NetworkStateReference() : base() { }
        public NetworkStateReference(Network.NetworkState value) : base(value) { }
        public bool Equals(NetworkStateReference other) { return base.Equals(other); }
        protected override bool ValueEquals(Network.NetworkState other)
        {
            throw new NotImplementedException();
        }
    }
}
