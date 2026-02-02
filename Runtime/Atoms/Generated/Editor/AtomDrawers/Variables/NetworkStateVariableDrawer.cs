#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable property drawer of type `Network.NetworkState`. Inherits from `AtomDrawer&lt;NetworkStateVariable&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(NetworkStateVariable))]
    public class NetworkStateVariableDrawer : VariableDrawer<NetworkStateVariable> { }
}
#endif
