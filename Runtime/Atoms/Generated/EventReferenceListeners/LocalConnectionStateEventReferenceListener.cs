using UnityEngine;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Reference Listener of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomEventReferenceListener&lt;FishNet.Transporting.LocalConnectionState, LocalConnectionStateEvent, LocalConnectionStateEventReference, LocalConnectionStateUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/LocalConnectionState Event Reference Listener")]
    public sealed class LocalConnectionStateEventReferenceListener : AtomEventReferenceListener<
        FishNet.Transporting.LocalConnectionState,
        LocalConnectionStateEvent,
        LocalConnectionStateEventReference,
        LocalConnectionStateUnityEvent>
    { }
}
