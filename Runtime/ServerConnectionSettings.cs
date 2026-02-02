using Services.Edgegap;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Network
{
    /// <summary>
    /// Defines the required settings for connecting to a server.
    /// </summary>
    [CreateAssetMenu(fileName = "Server Connection Settings_ New", menuName = "Network/Server Connection Settings")]
    public class ServerConnectionSettings : ManagedObject
    {
        [SerializeField, Tooltip("The address of the server to connect to.")]
        [FormerlySerializedAs("address")]
        string initialAddress = "localhost";

        [SerializeField, Tooltip("The port of the server to connect to.")]
        [FormerlySerializedAs("port")]
        ushort initialPort = 7770;

        public string address {get; set; }
        public ushort port { get; set; }

        protected override void Cleanup() { }

        protected override void Initialize()
        {
            address = initialAddress;
            port = initialPort;
        }

        public void Set(string address, ushort port)
        {
            this.address = address;
            this.port = port;
        }

        public void Set(DeploymentManager.DeploymentSession deploymentSession)
        {
            address = deploymentSession.ipAddress;
            port = (ushort)deploymentSession.port;
        }
    }
}
