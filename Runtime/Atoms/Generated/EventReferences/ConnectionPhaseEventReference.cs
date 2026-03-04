using System;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Reference of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomEventReference&lt;Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhaseVariable, ConnectionPhaseEvent, ConnectionPhaseVariableInstancer, ConnectionPhaseEventInstancer&gt;`.
    /// </summary>
    [Serializable]
    public sealed class ConnectionPhaseEventReference : AtomEventReference<
        Network.ConnectionToggleBehaviour.ConnectionPhase,
        ConnectionPhaseVariable,
        ConnectionPhaseEvent,
        ConnectionPhaseVariableInstancer,
        ConnectionPhaseEventInstancer>, IGetEvent 
    { }
}
