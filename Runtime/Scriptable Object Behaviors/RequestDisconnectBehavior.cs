using FishNet;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    [CreateAssetMenu(fileName = "Request Disconnect_ New", menuName = "Behaviors/Network/Request Disconnect")]
    public class RequestDisconnectBehavior : ScriptableObjectBehavior<NetworkConnection>
    {
        [Header("Disconnect Settings")]
        [Min(0)]
        public int disconnectCode = 0;
        [TextArea, Tooltip("Message to send to the client upon disconnect.")]
        public string message = "";

        [Space(10)]

        [Tooltip("If set to true, and no Network Connection is provided, only the local client will disconnect. If false, all clients will disconnect")]
        public bool disconnectLocalClientOnly = false;

        [Tooltip("True to stop the server after disconnecting all clients. False to keep the server running.")]
        public bool stopServer = false;
        
        public override void Invoke()
        {
            if (disconnectLocalClientOnly)
                Invoke(InstanceFinder.ClientManager.Connection);
            else
                base.Invoke();
        }

        protected override void Execute(NetworkConnection value)
        {
            if (Ref.TryGet(out DisconnectManager disconnectManager))
            {
                if (value != null)
                    disconnectManager.RequestDisconnectClient(value, disconnectCode, message);
                else
                    disconnectManager.RequestDisconnect(disconnectCode, message, stopServer);
            }
            else
            {
                Debug.LogWarning("DisconnectManager not found. Cannot execute disconnect behavior.");
            }
        }
    }
}
