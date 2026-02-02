#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityAtoms.Editor;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Network.NetworkState`. Inherits from `AtomDrawer&lt;NetworkStateEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomPropertyDrawer(typeof(NetworkStateEvent))]
    public class NetworkStateEventDrawer : AtomDrawer<NetworkStateEvent> { }
}
#endif
