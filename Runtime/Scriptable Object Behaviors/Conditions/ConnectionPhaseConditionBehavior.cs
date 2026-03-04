using Network;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace NRVS.Network
{

    [CreateAssetMenu(fileName = "Condition_ Connection Phase_ New", menuName = "Behaviors/Conditions/Network/Connection Phase")]
    public class ConnectionPhaseConditionBehavior : ConditionBehavior
    {
        [SerializeField]
        ConnectionPhaseReference connectionPhaseReference;

        [SerializeField]
        ConnectionToggleBehaviour.ConnectionPhase value;

        protected override bool Evaluate() => connectionPhaseReference.Value == value;
    }
}
