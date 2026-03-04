using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomEvent&lt;Network.ConnectionToggleBehaviour.ConnectionPhase&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/ConnectionPhase", fileName = "ConnectionPhaseEvent")]
    public sealed class ConnectionPhaseEvent : AtomEvent<Network.ConnectionToggleBehaviour.ConnectionPhase>
    {
    }
}
