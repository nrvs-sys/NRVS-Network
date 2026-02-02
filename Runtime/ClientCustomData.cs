using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Structure assigned to a NetworkConnection's `CustomData` property.
    /// </summary>
    public class ClientCustomData
    {
        public string lobbyPlayerID;
        public string leaderboardID;
        public string displayName;
        public ApplicationInfo.ApplicationMode applicationMode;
    }
}
