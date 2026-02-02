#if UNITY_2019_1_OR_NEWER
using UnityEditor;
using UnityEngine.UIElements;
using UnityAtoms.Editor;
using Network;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Event property drawer of type `Network.NetworkState`. Inherits from `AtomEventEditor&lt;Network.NetworkState, NetworkStateEvent&gt;`. Only availble in `UNITY_2019_1_OR_NEWER`.
    /// </summary>
    [CustomEditor(typeof(NetworkStateEvent))]
    public sealed class NetworkStateEventEditor : AtomEventEditor<Network.NetworkState, NetworkStateEvent> { }
}
#endif
