#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomEventEditor&lt;FishNet.Transporting.LocalConnectionState, LocalConnectionStateEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(LocalConnectionStateEvent))]
    public sealed class LocalConnectionStateEventEditor : AtomEventEditor<FishNet.Transporting.LocalConnectionState, LocalConnectionStateEvent> { }
}
#endif
