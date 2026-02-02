using UnityEngine;
using Network;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Constant of type `Network.NetworkState`. Inherits from `AtomBaseVariable&lt;Network.NetworkState&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-teal")]
    [CreateAssetMenu(menuName = "Unity Atoms/Constants/NetworkState", fileName = "NetworkStateConstant")]
    public sealed class NetworkStateConstant : AtomBaseVariable<Network.NetworkState> { }
}
