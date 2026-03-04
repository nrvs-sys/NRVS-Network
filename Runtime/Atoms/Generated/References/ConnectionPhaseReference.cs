using System;
using UnityAtoms.BaseAtoms;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Reference of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomReference&lt;Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhasePair, ConnectionPhaseConstant, ConnectionPhaseVariable, ConnectionPhaseEvent, ConnectionPhasePairEvent, ConnectionPhaseConnectionPhaseFunction, ConnectionPhaseVariableInstancer, AtomCollection, AtomList&gt;`.
    /// </summary>
    [Serializable]
    public sealed class ConnectionPhaseReference : AtomReference<
        Network.ConnectionToggleBehaviour.ConnectionPhase,
        ConnectionPhasePair,
        ConnectionPhaseConstant,
        ConnectionPhaseVariable,
        ConnectionPhaseEvent,
        ConnectionPhasePairEvent,
        ConnectionPhaseConnectionPhaseFunction,
        ConnectionPhaseVariableInstancer>, IEquatable<ConnectionPhaseReference>
    {
        public ConnectionPhaseReference() : base() { }
        public ConnectionPhaseReference(Network.ConnectionToggleBehaviour.ConnectionPhase value) : base(value) { }
        public bool Equals(ConnectionPhaseReference other) { return base.Equals(other); }
        protected override bool ValueEquals(Network.ConnectionToggleBehaviour.ConnectionPhase other)
        {
            throw new NotImplementedException();
        }
    }
}
