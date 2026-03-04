using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Instancer of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomEventInstancer&lt;Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/ConnectionPhase Event Instancer")]
    public class ConnectionPhaseEventInstancer : AtomEventInstancer<Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseEvent> { }
}
