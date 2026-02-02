#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomDrawer&lt;LocalConnectionStateEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalConnectionStateEvent))]
    public class LocalConnectionStateEventDrawer : AtomDrawer<LocalConnectionStateEvent> { }
}
#endif
