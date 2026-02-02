using FishNet.Object;
using FishNet.Object.Synchronizing;
using Network;
using UnityEngine;
using UnityEngine.Events;

public class NetworkBoolUtility : NetworkValueUtility<bool>
{
	private readonly SyncVar<bool> _value = new SyncVar<bool>();

	public override bool value => _value.Value;

	protected override void Awake() => InitializeSyncVar(_value);

	protected override void SetSyncVar(bool newValue)
	{
		_value.Value = newValue;
	}

	[ServerRpc(RequireOwnership = false)]
	protected override void RpcSetSyncVar(bool newValue) => RpcSetSyncVar_Impl(newValue);

	[ServerRpc(RequireOwnership = true)]
	protected override void RpcSetSyncVar_Owned(bool newValue) => RpcSetSyncVar_Impl(newValue);
}
