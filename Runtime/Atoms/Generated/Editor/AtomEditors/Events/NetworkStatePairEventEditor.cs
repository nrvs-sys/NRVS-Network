#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Network;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `NetworkStatePair`. Inherits from `AtomEventEditor&lt;NetworkStatePair, NetworkStatePairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(NetworkStatePairEvent))]
    public sealed class NetworkStatePairEventEditor : AtomEventEditor<NetworkStatePair, NetworkStatePairEvent> { }
}
#endif
