using System;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    /// <summary>
    /// Utility class for receiving RPC method calls from the RPC Manager, based on a specified key.
    /// </summary>
    public class RPCManagerUtility : MonoBehaviour
    {
        [SerializeField]
        StringReference methodKey;

        public UnityEvent onMethodCalled;

        void OnEnable()
        {
            Ref.Instance.OnRegistered += Ref_OnRegistered;
            Ref.Instance.OnUnregistered += Ref_OnUnregistered;

            if (Ref.TryGet(out RPCManager rpcManager))
                Ref_OnRegistered(typeof(RPCManager), rpcManager);
        }

        void OnDisable()
        {
            Ref.Instance.OnRegistered -= Ref_OnRegistered;
            Ref.Instance.OnUnregistered -= Ref_OnUnregistered;

            if (Ref.TryGet(out RPCManager rpcManager))
                Ref_OnUnregistered(typeof(RPCManager), rpcManager);
        }

        void Invoke()
        {
            onMethodCalled?.Invoke();
        }

        void Ref_OnRegistered(Type type, object instance)
        {
            if (type == typeof(RPCManager))
            {
                if (instance is RPCManager rpcManager)
                {
                    if (rpcManager.GetComponent<RPCUtility>().methods.TryGetValue(methodKey, out RPCUtility.RPCDefinition definition))
                    {
                        definition.method.AddListener(Invoke);
                    }
                    else
                    {
                        Debug.LogError($"Method with key '{methodKey}' not found in RPCUtility methods. Please check your setup.");
                    }
                }
                else
                {
                    Debug.LogError("RPCManager instance is not of the expected type. Please check your setup.");
                }
            }
        }

        void Ref_OnUnregistered(Type type, object instance)
        {
            if (type == typeof(RPCManager))
            {
                if (instance is RPCManager rpcManager)
                {
                    if (rpcManager.GetComponent<RPCUtility>().methods.TryGetValue(methodKey, out RPCUtility.RPCDefinition definition))
                    {
                        definition.method.RemoveListener(Invoke);
                    }
                    else
                    {
                        Debug.LogError($"Method with key '{methodKey}' not found in RPCUtility methods. Please check your setup.");
                    }
                }
            }
        }

    }
}
