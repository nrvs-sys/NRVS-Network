#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Network;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `ConnectionPhasePair`. Inherits from `AtomEventEditor&lt;ConnectionPhasePair, ConnectionPhasePairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(ConnectionPhasePairEvent))]
    public sealed class ConnectionPhasePairEventEditor : AtomEventEditor<ConnectionPhasePair, ConnectionPhasePairEvent> { }
}
#endif
