using FishNet.Object.Synchronizing;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FishNet.Transporting;
using UnityAtoms.BaseAtoms;
using NaughtyAttributes;

namespace Network
{
    public abstract class NetworkValueUtility<T> : NetworkBehaviour
    {
        [Header("Value Settings")]

        [Tooltip("This is used by scriptable object behaviors to reference a specific networked value utility")]
        public StringReference valueName = new("Networked Value");

        [Tooltip("This is the default value for the networked value")]
        public T defaultValue;

        [Header("Sync Settings")]

        public WritePermission writePermission = WritePermission.ServerOnly;
        public ReadPermission readPermission = ReadPermission.Observers;
        [Min(0f)]
        public float sendRate = 0.1f;
        public Channel channel = Channel.Reliable;

        [Space(10)]

        [Tooltip("If enabled, clients can request changes to the value. If disabled, only the server can change the value. ")]
        public bool clientAuthority = false;
        [EnableIf(nameof(clientAuthority)), Tooltip("If enabled, only owning clients can set the value, otherwise any client can set the value")]
        public bool requireOwnership = true;

        [Header("Events")]
        [Tooltip("Fired OnChange for the SyncVar")]
        public UnityEvent<T> onValueUpdated;

        bool syncVarInitialized = false;

        protected virtual void Awake()
        {
            if (!syncVarInitialized)
                throw new System.Exception("The Awake method must be overriden in derived NetworkValueUtility classes to call the InitializeSyncVar method.");
        }

        /// <summary>
        /// Call this in the Awake of the derived class to initialize the SyncVar
        /// </summary>
        /// <param name="syncVar"></param>
        protected void InitializeSyncVar(SyncVar<T> syncVar)
        {
            syncVar.SetInitialValues(defaultValue);
            syncVar.UpdateSettings(new SyncTypeSettings(writePermission, readPermission, sendRate, channel));
            syncVar.OnChange += OnValueChanged;
            syncVarInitialized = true;
        }

        public abstract T value { get; }

        protected abstract void SetSyncVar(T newValue);


        /// <summary>
        /// Sets the value, using RPC if necessary based on authority settings.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(T newValue)
        {
            // If client authority is enabled and this is a client, call rpc based on ownership requirement
            // If client authority is disabled and this is a client, do nothing
            // If server, set value directly, unless client authority is enabled, then call ser value based on ownership
            if (IsServerInitialized)
            {
                if (clientAuthority)
                {
                    if (IsController)
                        SetSyncVar(newValue);
                }
                else
                    SetSyncVar(newValue);
            }
            else if (IsClientInitialized)
            {
                // Allow immediate local change for responsiveness
                // If the WritePermission is ClientUnsynchronized, this will allow setting for clients. Otherwise it will be ignored.
                SetSyncVar(newValue);

                if (clientAuthority)
                {
                    if (requireOwnership)
                    {
                        RpcSetSyncVar_Owned(newValue);
                    }
                    else
                        RpcSetSyncVar(newValue);
                }
            }

        }

        /// <summary>
        /// Implement in derived class with appropriate attributes - [ServerRpc(RequireOwnership = false)]
        /// </summary>
        /// <param name="newValue"></param>
        protected abstract void RpcSetSyncVar(T newValue);

        /// <summary>
        /// Implement in derived class with appropriate attributes - [ServerRpc(RequireOwnership = true)]
        /// </summary>
        /// <param name="newValue"></param>
        protected abstract void RpcSetSyncVar_Owned(T newValue);

        protected void RpcSetSyncVar_Impl(T newValue)
        {
            SetSyncVar(newValue);
        }

        protected virtual void OnValueChanged(T prev, T next, bool asServer)
        {
            onValueUpdated?.Invoke(next);
        }
    }
}
