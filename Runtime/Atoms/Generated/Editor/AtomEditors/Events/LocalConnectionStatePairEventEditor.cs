#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `LocalConnectionStatePair`. Inherits from `AtomEventEditor&lt;LocalConnectionStatePair, LocalConnectionStatePairEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(LocalConnectionStatePairEvent))]
    public sealed class LocalConnectionStatePairEventEditor : AtomEventEditor<LocalConnectionStatePair, LocalConnectionStatePairEvent> { }
}
#endif
