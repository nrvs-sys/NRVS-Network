using FishNet.Object;
using FishNet.Object.Synchronizing;
using Network;
using UnityEngine;
using UnityEngine.Events;

public class NetworkFloatUtility : NetworkValueUtility<float>
{
    private readonly SyncVar<float> _value = new SyncVar<float>();

    public override float value => _value.Value;

    protected override void Awake() => InitializeSyncVar(_value);

    protected override void SetSyncVar(float newValue)
    {
        _value.Value = newValue;
    }

    [ServerRpc(RequireOwnership = false)]
    protected override void RpcSetSyncVar(float newValue) => RpcSetSyncVar_Impl(newValue);

    [ServerRpc(RequireOwnership = true)]
    protected override void RpcSetSyncVar_Owned(float newValue) => RpcSetSyncVar_Impl(newValue);
}
