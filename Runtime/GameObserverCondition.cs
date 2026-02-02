using FishNet.Connection;
using FishNet.Observing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    [CreateAssetMenu(menuName = "FishNet/Observers/Game Observer Condition_ New", fileName = "Game Observer Condition")]
    public class GameObserverCondition : ObserverCondition
    {
        public override bool ConditionMet(NetworkConnection connection, bool currentlyAdded, out bool notProcessed)
        {
            notProcessed = false;
            return true;
        }

        public override ObserverConditionType GetConditionType()
        {
            return ObserverConditionType.Normal;
        }
    }
}
