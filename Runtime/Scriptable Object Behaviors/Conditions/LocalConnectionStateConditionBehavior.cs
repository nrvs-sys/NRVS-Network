using FishNet.Transporting;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

[CreateAssetMenu(fileName = "Condition_ Local Connection State_ New", menuName = "Behaviors/Conditions/Network/Local Connection State")]
public class LocalConnectionStateConditionBehavior : ConditionBehavior
{
    [SerializeField]
    LocalConnectionStateVariable localConnectionStateReference;

    [SerializeField]
    LocalConnectionState value;

    protected override bool Evaluate() => localConnectionStateReference.Value == value;
}
