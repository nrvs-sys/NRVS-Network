#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Network;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomEventEditor&lt;Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(ConnectionPhaseEvent))]
    public sealed class ConnectionPhaseEventEditor : AtomEventEditor<Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseEvent> { }
}
#endif
