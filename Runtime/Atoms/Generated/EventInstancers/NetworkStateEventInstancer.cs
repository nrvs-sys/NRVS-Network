using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Instancer of type `Network.NetworkState`. Inherits from `AtomEventInstancer&lt;Network.NetworkState, NetworkStateEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/NetworkState Event Instancer")]
    public class NetworkStateEventInstancer : AtomEventInstancer<Network.NetworkState, NetworkStateEvent> { }
}
