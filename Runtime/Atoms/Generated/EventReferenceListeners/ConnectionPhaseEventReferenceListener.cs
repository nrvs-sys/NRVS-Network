using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Reference Listener of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomEventReferenceListener&lt;Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseEvent, ConnectionPhaseEventReference, ConnectionPhaseUnityEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-orange")]
    [AddComponentMenu("Unity Atoms/Listeners/ConnectionPhase Event Reference Listener")]
    public sealed class ConnectionPhaseEventReferenceListener : AtomEventReferenceListener<
        Network.ConnectionToggleBehaviour.ConnectionPhase,
        ConnectionPhaseEvent,
        ConnectionPhaseEventReference,
        ConnectionPhaseUnityEvent>
    { }
}
