using FishNet.Connection;
using FishNet.Managing.Server;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Network
{
    /// <summary>
    /// Manages disconnecting clients from the server with custom disconnect codes and reasons.
    /// </summary>
    public class DisconnectManager : NetworkBehaviour
    {
        [Serializable]
        public struct DisconnectData
        {
            public string description;

            public UnityEvent<string> onClientDisconnected;
        }

        [Header("References")]

        [SerializeField]
        ConnectionToggleBehaviour connectionToggle;

        [SerializeField]
        SerializableDictionary<int, DisconnectData> disconnectHandlers = new();

        Coroutine waitForClientsToDisconnectCoroutine;

        void Awake()
        {
            Ref.Register<DisconnectManager>(this);
        }

        void OnDestroy()
        {
            if (waitForClientsToDisconnectCoroutine != null)
                StopCoroutine(waitForClientsToDisconnectCoroutine);

            waitForClientsToDisconnectCoroutine = null;

            Ref.Unregister<DisconnectManager>(this);
        }

        #region Rpc Methods

        [TargetRpc]
        void RpcDisconnectClient(NetworkConnection conn, int disconnectCode, string message)
        {
            if (disconnectHandlers.TryGetValue(disconnectCode, out DisconnectData data))
            {
                data.onClientDisconnected?.Invoke(message);

                Debug.Log($"Disconnecting client {conn.ClientId} with code {disconnectCode}: {data.description}. Message: {message}");
            }
            else
            {
                Debug.LogWarning($"No disconnect handler found for code {disconnectCode}. Message: {message}. Disconnecting without action.");
            }

            connectionToggle?.GoOffline();
        }

        [ObserversRpc]
        void RpcDisconnect(int disconnectCode, string message)
        {
            if (disconnectHandlers.TryGetValue(disconnectCode, out DisconnectData data))
            {
                data.onClientDisconnected?.Invoke(message);

                Debug.Log($"Disconnecting with code {disconnectCode}: {data.description}. Message: {message}");
            }
            else
            {
                Debug.LogWarning($"No disconnect handler found for code {disconnectCode}. Message: {message}. Disconnecting without action.");
            }

            if (!IsServerInitialized)
                connectionToggle?.GoOffline();
            else
                // If this is called on the Server, we only stop the Client Connection for now.
                // If specified, the Server will "GoOffline" once all Clients have disconnected.
                connectionToggle.OnlineConnection.StopClient();
        }

        /// <summary>
        /// Sends a disconnect request to the server with a specified disconnect code and message.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="disconnectCode"></param>
        /// <param name="message"></param>
        [ServerRpc(RequireOwnership = false)]
        void RpcRequestDisconnectClient(NetworkConnection conn, int disconnectCode, string message = "")
        {
            DisconnectClient(conn, disconnectCode, message);
        }

        /// <summary>
        /// Sends a disconnect request to the server with a specified disconnect code and message.
        /// </summary>
        /// <param name="disconnectCode"></param>
        /// <param name="message"></param>
        /// <param name="stopServer"></param>
        [ServerRpc(RequireOwnership = false)]
        void RpcRequestDisconnect(int disconnectCode, string message = "", bool stopServer = false)
        {
            Disconnect(disconnectCode, message, stopServer);
        }

        #endregion

        /// <summary>
        /// Disconnects a specific client from the server with a specified disconnect code and message. This method is only available on the server.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="disconnectCode"></param>
        /// <param name="message"></param>
        /// <param name="onComplete">Method that will be invoked on the Server after the specified Client has successfully Disconnected.</param>
        [Server]
        public void DisconnectClient(NetworkConnection conn, int disconnectCode, string message = "", Action onComplete = null)
        {
            Debug.Log($" Disconnecting client {conn.ClientId} with code {disconnectCode}. Message: {message}");

            RpcDisconnectClient(conn, disconnectCode, message);

            StartCoroutine(DoWaitForClientToDisconnect(conn, onComplete));
        }

        /// <summary>
        /// Disconnects all clients from the server with a specified disconnect code and message. This method is only available on the server.
        /// </summary>
        /// <param name="disconnectCode"></param>
        /// <param name="message"></param>
        /// <param name="stopServer">If true, stop the server after all Clients have successfully Disconnected.</param>
        /// <param name="onComplete">Method that will be invoked on the Server after all Clients have successfully Disconnected.</param>
        [Server]
        public void Disconnect(int disconnectCode, string message = "", bool stopServer = false, Action onComplete = null)
        {
            if (waitForClientsToDisconnectCoroutine != null)
            {
                Debug.LogWarning("Already waiting for clients to disconnect. Ignoring new disconnect request.");
                return;
            }

            Debug.Log($"Disconnecting all clients with code {disconnectCode}. Message: {message}");

            RpcDisconnect(disconnectCode, message);

            waitForClientsToDisconnectCoroutine = StartCoroutine(DoWaitForClientsToDisconnect(stopServer, onComplete));
        }


        /// <summary>
        /// Requests the server to disconnect a specific client with a specified disconnect code and message. This method is only available on the client.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="disconnectCode"></param>
        /// <param name="message"></param>
        [Client]
        public void RequestDisconnectClient(NetworkConnection conn, int disconnectCode, string message = "")
        {
            Debug.Log($"Requesting disconnect of client {conn.ClientId} with code {disconnectCode}. Message: {message}");
            RpcRequestDisconnectClient(conn, disconnectCode, message);
        }

        /// <summary>
        /// Requests the server to disconnect all clients with a specified disconnect code and message. This method is only available on the client.
        /// </summary>
        /// <param name="disconnectCode"></param>
        /// <param name="message"></param>
        /// <param name="stopServer"></param>
        [Client]
        public void RequestDisconnect(int disconnectCode, string message = "", bool stopServer = false)
        {
            Debug.Log($"Requesting disconnect with code {disconnectCode}. Message: {message}");
            RpcRequestDisconnect(disconnectCode, message, stopServer);
        }

        IEnumerator DoWaitForClientToDisconnect(NetworkConnection conn, Action onComplete)
        {
            while (IsServerInitialized && ServerManager.Clients.ContainsValue(conn))
                yield return null;

            if (onComplete != null)
                onComplete.Invoke();
        }

        IEnumerator DoWaitForClientsToDisconnect(bool stopServer, Action onComplete)
        {
            while (IsServerInitialized && ServerManager.Clients.Count > 0)
                yield return null;

            if (stopServer)
                connectionToggle?.GoOffline();

            if (onComplete != null)
                onComplete.Invoke();

            waitForClientsToDisconnectCoroutine = null;
        }
    }
}
