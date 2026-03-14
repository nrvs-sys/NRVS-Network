using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    public class RPCUtility : NetworkBehaviour
    {
        #region Data Types

        [System.Flags]
        public enum RPCType
        {
            None = 0,
            Server = 1,
            Observer = 1 << 1,
            Target = 1 << 2
        }

        [System.Serializable]
        public class RPCDefinition
        {
            public RPCType rpcType = RPCType.Observer;

            [Header("General Settings")]

            public bool runLocally = false;

            [Header("Server RPC Settings")]

            public bool requireOwnership = true;

            [Header("Observer/Target RPC Settings")]

            public bool excludeServer = false;

            [Space(10)]

            public UnityEvent method;

            public bool isServerRpc => rpcType.HasFlag(RPCType.Server);
            public bool isObserverRPC => rpcType.HasFlag(RPCType.Observer);
            public bool isTargetRPC => rpcType.HasFlag(RPCType.Target);
        }

        #endregion

        public Utility.SerializableDictionary<StringReference, RPCDefinition> methods = new();

        RPCDefinition GetDefinition(string key) => methods.First(GetDefinition => GetDefinition.Key == key).Value;

        public void CallMethod(string key) => CallMethod(null, key);

        public void CallMethod(StringConstant key) => CallMethod(null, key.Value);

        public void CallMethod(StringVariable key) => CallMethod(null, key.Value);

        public void CallMethod(StringReference key) => CallMethod(null, key.Value);

        public void CallMethod(NetworkConnection conn, string key)
        {
            var definition = GetDefinition(key);

            if (definition == null)
                return;

            if (definition.isServerRpc)
            {
                if (definition.requireOwnership)
                {
                    if (definition.runLocally)
                        InvokeServerRunLocallyOwnerShipRequired(key);
                    else
                        InvokeServerOwnerShipRequired(key);
                }
                else
                {
                    if (definition.runLocally)
                        InvokeServerRunLocally(key);
                    else
                        InvokeServer(key);
                }
            }

            if (definition.isObserverRPC)
            {
                if (definition.excludeServer)
                {
                    if (definition.runLocally)
                        InvokeObserverRunLocallyExcludeServer(key);
                    else
                        InvokeObserverExcludeServer(key);
                }
                else
                {
                    if (definition.runLocally)
                        InvokeObserverRunLocally(key);
                    else
                        InvokeObserver(key);
                }
            }

            if (definition.isTargetRPC)
            {
                if (definition.excludeServer)
                {
                    if (definition.runLocally)
                        InvokeTargetRunLocallyExcludeServer(conn, key);
                    else
                        InvokeTargetExcludeServer(conn, key);
                }
                else
                {
                    if (definition.runLocally)
                        InvokeTargetRunLocally(conn, key);
                    else
                        InvokeTarget(conn, key);
                }
            }
        }

        void Invoke(string key) => GetDefinition(key)?.method?.Invoke();

        #region Server RPCs

        [ServerRpc(RunLocally = false, RequireOwnership = false)]
        void InvokeServer(string key) => Invoke(key);
        [ServerRpc(RunLocally = true, RequireOwnership = false)]
        void InvokeServerRunLocally(string key) => Invoke(key);
        [ServerRpc(RunLocally = false, RequireOwnership = true)]
        void InvokeServerOwnerShipRequired(string key) => Invoke(key);
        [ServerRpc(RunLocally = true, RequireOwnership = true)]
        void InvokeServerRunLocallyOwnerShipRequired(string key) => Invoke(key);

        #endregion

        #region Observer RPCs

        [ObserversRpc(RunLocally = false)]
        void InvokeObserver(string key) => Invoke(key);
        [ObserversRpc(RunLocally = true)]
        void InvokeObserverRunLocally(string key) => Invoke(key);
        [ObserversRpc(RunLocally = false, ExcludeServer = true)]
        void InvokeObserverExcludeServer(string key) => Invoke(key);
        [ObserversRpc(RunLocally = true, ExcludeServer = true)]
        void InvokeObserverRunLocallyExcludeServer(string key) => Invoke(key);

        #endregion

        #region Target RPCs

        [TargetRpc(RunLocally = false)]
        void InvokeTarget(NetworkConnection conn, string key) => Invoke(key);
        [TargetRpc(RunLocally = true)]
        void InvokeTargetRunLocally(NetworkConnection conn, string key) => Invoke(key);
        [TargetRpc(RunLocally = false, ExcludeServer = true)]
        void InvokeTargetExcludeServer(NetworkConnection conn, string key) => Invoke(key);
        [TargetRpc(RunLocally = true, ExcludeServer = true)]
        void InvokeTargetRunLocallyExcludeServer(NetworkConnection conn, string key) => Invoke(key);

        #endregion
    }
}
