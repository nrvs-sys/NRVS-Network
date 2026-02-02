using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Condition_ Dedicated Server_ New", menuName = "Behaviors/Conditions/Network/Dedicated Server")]
public class DedicatedServerConditionBehavior : ConditionBehavior
{
    protected override bool Evaluate() =>
        #if UNITY_SERVER
        true;
        #else
        false;
        #endif
}
