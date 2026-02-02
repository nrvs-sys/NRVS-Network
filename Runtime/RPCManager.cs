using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Unified RPC manager that handles calling RPC methods based on the provided keys.
    /// </summary>
    public class RPCManager : MonoBehaviour
    {
        public bool isInitialized => rpcUtility != null && (rpcUtility.IsClientInitialized || rpcUtility.IsServerInitialized);

        RPCUtility rpcUtility;

        void Awake()
        {
            rpcUtility = GetComponent<RPCUtility>();

            if (rpcUtility == null)
            {
                Debug.LogError("RPCUtility component is missing on the GameObject. Please add it to use RPC Manager.");
            }

            Ref.Register<RPCManager>(this);
        }

        void OnDestroy()
        {
            Ref.Unregister<RPCManager>(this);
        }

        public void CallMethod(string key) => rpcUtility.CallMethod(key);
        public void CallMethod(StringConstant key) => rpcUtility.CallMethod(key.Value);
        public void CallMethod(StringVariable key) => rpcUtility.CallMethod(key.Value);
        public void CallMethod(StringReference key) => rpcUtility.CallMethod(key.Value);
        public void CallMethod(NetworkConnection conn, string key) => rpcUtility.CallMethod(conn, key);
    }
}
