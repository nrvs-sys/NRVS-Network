#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomDrawer&lt;ConnectionPhaseVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConnectionPhaseVariable))]
    public class ConnectionPhaseVariableDrawer : VariableDrawer<ConnectionPhaseVariable> { }
}
#endif
