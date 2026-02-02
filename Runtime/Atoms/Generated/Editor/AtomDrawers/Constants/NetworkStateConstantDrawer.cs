#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Constant property drawer of type `Network.NetworkState`. Inherits from `AtomDrawer&lt;NetworkStateConstant&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(NetworkStateConstant))]
    public class NetworkStateConstantDrawer : VariableDrawer<NetworkStateConstant> { }
}
#endif
