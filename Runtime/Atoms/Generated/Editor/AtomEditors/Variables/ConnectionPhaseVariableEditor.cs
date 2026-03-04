using UnityEditor;
using UnityAtoms.Editor;
using Network;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(ConnectionPhaseVariable))]
    public sealed class ConnectionPhaseVariableEditor : AtomVariableEditor<Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhasePair> { }
}
