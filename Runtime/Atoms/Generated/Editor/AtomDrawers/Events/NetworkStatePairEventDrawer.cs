#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `NetworkStatePair`. Inherits from `AtomDrawer&lt;NetworkStatePairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(NetworkStatePairEvent))]
    public class NetworkStatePairEventDrawer : AtomDrawer<NetworkStatePairEvent> { }
}
#endif
