using FishNet.Object;
using FishNet.Object.Synchronizing;
using Network;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

public class NetworkIntUtility : NetworkValueUtility<int>
{
	private readonly SyncVar<int> _value = new SyncVar<int>();

	public override int value => _value.Value;

	protected override void Awake() => InitializeSyncVar(_value);

	protected override void SetSyncVar(int newValue)
	{
		_value.Value = newValue;
	}

	public void SetValue(IntVariable newValue)
	{
		if (newValue == null)
		{
			Debug.LogError("NetworkIntUtility.SetValue called with null IntVariable.", this);
			return;
		}

		SetValue(newValue.Value);
	}

	[ServerRpc(RequireOwnership = false)]
	protected override void RpcSetSyncVar(int newValue) => RpcSetSyncVar_Impl(newValue);

	[ServerRpc(RequireOwnership = true)]
	protected override void RpcSetSyncVar_Owned(int newValue) => RpcSetSyncVar_Impl(newValue);
}
