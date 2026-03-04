using UnityEngine;
using System;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable of type `Network.ConnectionToggleBehaviour.ConnectionPhase`. Inherits from `AtomVariable&lt;Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhasePair, ConnectionPhaseEvent, ConnectionPhasePairEvent, ConnectionPhaseConnectionPhaseFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/ConnectionPhase", fileName = "ConnectionPhaseVariable")]
    public sealed class ConnectionPhaseVariable : AtomVariable<Network.ConnectionToggleBehaviour.ConnectionPhase, ConnectionPhasePair, ConnectionPhaseEvent, ConnectionPhasePairEvent, ConnectionPhaseConnectionPhaseFunction>
    {
        protected override bool ValueEquals(Network.ConnectionToggleBehaviour.ConnectionPhase other)
        {
            throw new NotImplementedException();
        }
    }
}
