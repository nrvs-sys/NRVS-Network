#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomDrawer&lt;LocalConnectionStateVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalConnectionStateVariable))]
    public class LocalConnectionStateVariableDrawer : VariableDrawer<LocalConnectionStateVariable> { }
}
#endif
