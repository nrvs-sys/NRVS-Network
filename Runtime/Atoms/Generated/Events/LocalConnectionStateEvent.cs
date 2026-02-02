using UnityEngine;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomEvent&lt;FishNet.Transporting.LocalConnectionState&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-cherry")]
    [CreateAssetMenu(menuName = "Unity Atoms/Events/LocalConnectionState", fileName = "LocalConnectionStateEvent")]
    public sealed class LocalConnectionStateEvent : AtomEvent<FishNet.Transporting.LocalConnectionState>
    {
    }
}
