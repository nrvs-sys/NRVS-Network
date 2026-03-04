#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomDrawer&lt;ConnectionPhaseEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConnectionPhaseEvent))]
    public class ConnectionPhaseEventDrawer : AtomDrawer<ConnectionPhaseEvent> { }
}
#endif
