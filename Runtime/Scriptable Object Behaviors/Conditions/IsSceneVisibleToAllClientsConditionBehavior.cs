using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    [CreateAssetMenu(fileName = "Condition_ Is Scene Visible To All Clients_ New", menuName = "Behaviors/Conditions/Network/Is Scene Visible To All Clients")]
    public class IsSceneVisibleToAllClientsConditionBehavior : ConditionBehavior
    {
        [SerializeField]
        SceneReference sceneReference;

        protected override bool Evaluate() => Ref.TryGet(out NetworkSceneManager networkSceneManager) && networkSceneManager.IsSceneVisibleToAllClients(sceneReference.SceneName());
    }
}
