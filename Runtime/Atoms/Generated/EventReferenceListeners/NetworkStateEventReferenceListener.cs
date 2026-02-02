using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Reference Listener of type `Network.NetworkState`. Inherits from `AtomEventReferenceListener&lt;Network.NetworkState, NetworkStateEvent, NetworkStateEventReference, NetworkStateUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/NetworkState Event Reference Listener")]
    public sealed class NetworkStateEventReferenceListener : AtomEventReferenceListener<
        Network.NetworkState,
        NetworkStateEvent,
        NetworkStateEventReference,
        NetworkStateUnityEvent>
    { }
}
