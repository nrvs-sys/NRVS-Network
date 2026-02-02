using UnityEditor;
using UnityAtoms.Editor;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms.Editor
{
    /// <summary>
    /// Variable Inspector of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomVariableEditor`
    /// </summary>
    [CustomEditor(typeof(LocalConnectionStateVariable))]
    public sealed class LocalConnectionStateVariableEditor : AtomVariableEditor<FishNet.Transporting.LocalConnectionState, LocalConnectionStatePair> { }
}
