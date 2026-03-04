using Network;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace NRVS.Network
{

    [CreateAssetMenu(fileName = "Condition_ Connection Phase_ New", menuName = "Behaviors/Conditions/Network/Connection Phase")]
    public class ConnectionPhaseConditionBehavior : ConditionBehavior<ConnectionPhaseVariable>
    {
        [SerializeField]
        ConnectionToggleBehaviour.ConnectionPhase value;

        protected override bool Evaluate(ConnectionPhaseVariable value)
        {
            return value.Value == this.value;
        }
    }
}
