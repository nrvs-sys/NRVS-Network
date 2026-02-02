using System;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Reference of type `Network.NetworkState`. Inherits from `AtomEventReference&lt;Network.NetworkState, NetworkStateVariable, NetworkStateEvent, NetworkStateVariableInstancer, NetworkStateEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class NetworkStateEventReference : AtomEventReference<
        Network.NetworkState,
        NetworkStateVariable,
        NetworkStateEvent,
        NetworkStateVariableInstancer,
        NetworkStateEventInstancer>, IGetEvent 
    { }
}
