using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    [CreateAssetMenu(fileName = "Disconnect_ New", menuName = "Behaviors/Network/Disconnect")]
    public class DisconnectBehavior : ScriptableObjectBehavior<NetworkConnection>
    {
        [Header("Disconnect Settings")]
        [Min(0)]
        public int disconnectCode = 0;
        [TextArea, Tooltip("Message to send to the client upon disconnect.")]
        public string message = "";
        [Tooltip("True to stop the server after disconnecting all clients. False to keep the server running.")]
        public bool stopServer = false;
        [Tooltip("Event to invoke on the Server when the disconnect action is complete.")]
        public UnityEvent onServerDisconnectComplete;

        protected override void Execute(NetworkConnection value)
        {
            if (Ref.TryGet(out DisconnectManager disconnectManager))
            {
                if (value != null)
                    disconnectManager.DisconnectClient(value, disconnectCode, message, () => onServerDisconnectComplete?.Invoke());
                else
                    disconnectManager.Disconnect(disconnectCode, message, stopServer, () => onServerDisconnectComplete?.Invoke());
            }
            else
            {
                Debug.LogWarning("DisconnectManager not found. Cannot execute disconnect behavior.");
            }
        }
    }
}
