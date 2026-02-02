using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{
    public class NetworkObjectOwnerUtility : NetworkBehaviour
    {
        public UnityEvent onGainedOwnership;
        public UnityEvent onLostOwnership;

        bool wasOwner = false;

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);

            if (IsOwner && !wasOwner)
            {
                wasOwner = true;
                onGainedOwnership?.Invoke();
            }
            else if (!IsOwner && wasOwner)
            {
                wasOwner = false;
                onLostOwnership?.Invoke();
            }
        }

        public override void OnOwnershipServer(NetworkConnection prevOwner)
        {
            base.OnOwnershipServer(prevOwner);

            if (IsOwner && !wasOwner)
            {
                wasOwner = true;
                onGainedOwnership?.Invoke();
            }
            else if (!IsOwner && wasOwner)
            {
                wasOwner = false;
                onLostOwnership?.Invoke();
            }
        }
    }
}
