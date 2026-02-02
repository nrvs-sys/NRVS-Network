using UnityEngine;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `LocalConnectionStatePair`. Inherits from `AtomEvent&lt;LocalConnectionStatePair&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/LocalConnectionStatePair", fileName = "LocalConnectionStatePairEvent")]
    public sealed class LocalConnectionStatePairEvent : AtomEvent<LocalConnectionStatePair>
    {
    }
}
