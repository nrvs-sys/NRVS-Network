using System;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Reference of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomEventReference&lt;FishNet.Transporting.LocalConnectionState, LocalConnectionStateVariable, LocalConnectionStateEvent, LocalConnectionStateVariableInstancer, LocalConnectionStateEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class LocalConnectionStateEventReference : AtomEventReference<
        FishNet.Transporting.LocalConnectionState,
        LocalConnectionStateVariable,
        LocalConnectionStateEvent,
        LocalConnectionStateVariableInstancer,
        LocalConnectionStateEventInstancer>, IGetEvent 
    { }
}
