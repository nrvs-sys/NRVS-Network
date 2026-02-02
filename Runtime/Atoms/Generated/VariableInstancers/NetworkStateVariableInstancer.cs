using UnityEngine;
using UnityAtoms.BaseAtoms;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable Instancer of type `Network.NetworkState`. Inherits from `AtomVariableInstancer&lt;NetworkStateVariable, NetworkStatePair, Network.NetworkState, NetworkStateEvent, NetworkStatePairEvent, NetworkStateNetworkStateFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-hotpink")]
    [AddComponentMenu("Unity Atoms/Variable Instancers/NetworkState Variable Instancer")]
    public class NetworkStateVariableInstancer : AtomVariableInstancer<
        NetworkStateVariable,
        NetworkStatePair,
        Network.NetworkState,
        NetworkStateEvent,
        NetworkStatePairEvent,
        NetworkStateNetworkStateFunction>
    { }
}
