using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `ConnectionPhasePair`. Inherits from `AtomEvent&lt;ConnectionPhasePair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/ConnectionPhasePair", fileName = "ConnectionPhasePairEvent")]
    public sealed class ConnectionPhasePairEvent : AtomEvent<ConnectionPhasePair>
    {
    }
}
