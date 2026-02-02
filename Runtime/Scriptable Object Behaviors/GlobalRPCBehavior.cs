using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Network
{
    [CreateAssetMenu(fileName = "Rpc_ Global_ New", menuName = "Behaviors/Network/Global Rpc")]
    public class GlobalRPCBehavior : ScriptableObjectBehavior
    {
        [SerializeField]
        StringReference methodKey;

        protected override void Execute()
        {
            if (Ref.TryGet(out RPCManager rpcManager))
            {
                if (!rpcManager.isInitialized)
                    Debug.LogWarning("RPCManager is not initialized. Ensure it is before calling Global Rpcs.");
                else
                    rpcManager.CallMethod(methodKey);
            }
            else
            {
                Debug.LogError("RPCManager not found. Please ensure it is present before executing Global Rpcs.");
            }
        }
    }
}
