using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `NetworkStatePair`. Inherits from `AtomEvent&lt;NetworkStatePair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/NetworkStatePair", fileName = "NetworkStatePairEvent")]
    public sealed class NetworkStatePairEvent : AtomEvent<NetworkStatePair>
    {
    }
}
