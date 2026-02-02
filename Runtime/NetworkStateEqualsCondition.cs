using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

namespace Network
{
    [CreateAssetMenu(menuName = "Unity Atoms/Conditions/Network State/Equals", fileName = "Condition_ Network State_ Equals_ New")]
    public class NetworkStateEqualsCondition : NetworkStateCondition
    {
        public NetworkState equalTo = NetworkState.Offline;

        public override bool Call(NetworkState value) => value == equalTo;
    }
}