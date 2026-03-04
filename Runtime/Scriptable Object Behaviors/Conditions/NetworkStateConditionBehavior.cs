using Network;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

[CreateAssetMenu(fileName = "Condition_ Network State_ New", menuName = "Behaviors/Conditions/Network/Network State")]
public class ConnectionPhaseConditionBehavior : ConditionBehavior
{
    [SerializeField]
    NetworkStateReference networkStateReference;

    [SerializeField]
    NetworkState value;

    protected override bool Evaluate() => networkStateReference.Value == value;
}
