#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Constant property drawer of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomDrawer&lt;ConnectionPhaseConstant&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConnectionPhaseConstant))]
    public class ConnectionPhaseConstantDrawer : VariableDrawer<ConnectionPhaseConstant> { }
}
#endif
