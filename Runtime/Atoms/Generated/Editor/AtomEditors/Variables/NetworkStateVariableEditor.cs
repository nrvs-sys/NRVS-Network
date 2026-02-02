using UnityEditor;
using UnityAtoms.Editor;
using Network;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `Network.NetworkState`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(NetworkStateVariable))]
    public sealed class NetworkStateVariableEditor : AtomVariableEditor<Network.NetworkState, NetworkStatePair> { }
}
