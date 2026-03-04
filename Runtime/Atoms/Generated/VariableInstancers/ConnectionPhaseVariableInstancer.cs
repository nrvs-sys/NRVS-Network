using UnityEngine;
using UnityAtoms.BaseAtoms;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable Instancer of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomVariableInstancer&lt;ConnectionPhaseVariable, ConnectionPhasePair, Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseEvent, ConnectionPhasePairEvent, ConnectionPhaseConnectionPhaseFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-hotpink")]
    [AddComponentMenu("Unity Atoms/Variable Instancers/ConnectionPhase Variable Instancer")]
    public class ConnectionPhaseVariableInstancer : AtomVariableInstancer<
        ConnectionPhaseVariable,
        ConnectionPhasePair,
        Network.ConnectionToggleBehaviour.ConnectionPhase,
        ConnectionPhaseEvent,
        ConnectionPhasePairEvent,
        ConnectionPhaseConnectionPhaseFunction>
    { }
}
