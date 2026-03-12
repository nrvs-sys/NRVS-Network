using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Network
{
    /// <summary>
    /// Behavior to start and stop network connections.
    /// </summary>
    public abstract class ConnectionBehaviour : ScriptableObject
    {
        public enum ConnectionState
        {
            None,
            Starting,
            Stopping
        }

        [SerializeField, Min(0), Tooltip("The index of the desired Transport as determined by the NetworkManager's Multipass Transport")]
        protected int transportIndex = 0;

        public virtual void StartHost()
        {
            StartServer();
            StartClient();
        }

        public virtual void StopHost()
        {
            StopClient();
            StopServer(true);
        }

        public abstract void StartServer();

        public abstract void StopServer(bool sendDisconnectMessage = true);

        public abstract void StartClient();

        public abstract void StopClient();

        public abstract ConnectionState GetServerConnectionState();
        public abstract ConnectionState GetClientConnectionState();
    }
}
