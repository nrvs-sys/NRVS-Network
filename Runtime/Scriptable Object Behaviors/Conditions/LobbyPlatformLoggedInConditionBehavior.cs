using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Condition_ Lobby Platform Logged In_ New", menuName = "Behaviors/Conditions/Network/Lobby Platform Logged In")]
public class LobbyPlatformLoggedInConditionBehavior : ConditionBehavior
{
    [SerializeField, Tooltip("If true, this condition will pass whether the login was successful or errored.")]
    bool passIfErrored;

    protected override bool Evaluate() => Ref.TryGet<ILobbyPlatform>(out var lobbyPlatform) && 
        (
        passIfErrored ? 
            lobbyPlatform.isLoggedIn || lobbyPlatform.hasLoginError :
            lobbyPlatform.isLoggedIn
        );
}
