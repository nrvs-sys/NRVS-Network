#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `ConnectionPhasePair`. Inherits from `AtomDrawer&lt;ConnectionPhasePairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(ConnectionPhasePairEvent))]
    public class ConnectionPhasePairEventDrawer : AtomDrawer<ConnectionPhasePairEvent> { }
}
#endif
