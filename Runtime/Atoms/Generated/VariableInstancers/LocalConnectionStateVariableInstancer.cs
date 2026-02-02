using UnityEngine;
using UnityAtoms.BaseAtoms;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable Instancer of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomVariableInstancer&lt;LocalConnectionStateVariable, LocalConnectionStatePair, FishNet.Transporting.LocalConnectionState, LocalConnectionStateEvent, LocalConnectionStatePairEvent, LocalConnectionStateLocalConnectionStateFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-hotpink")]
    [AddComponentMenu("Unity Atoms/Variable Instancers/LocalConnectionState Variable Instancer")]
    public class LocalConnectionStateVariableInstancer : AtomVariableInstancer<
        LocalConnectionStateVariable,
        LocalConnectionStatePair,
        FishNet.Transporting.LocalConnectionState,
        LocalConnectionStateEvent,
        LocalConnectionStatePairEvent,
        LocalConnectionStateLocalConnectionStateFunction>
    { }
}
