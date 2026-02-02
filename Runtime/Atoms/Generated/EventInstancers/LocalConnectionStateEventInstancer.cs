using UnityEngine;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Event Instancer of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomEventInstancer&lt;FishNet.Transporting.LocalConnectionState, LocalConnectionStateEvent&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-sign-blue")]
    [AddComponentMenu("Unity Atoms/Event Instancers/LocalConnectionState Event Instancer")]
    public class LocalConnectionStateEventInstancer : AtomEventInstancer<FishNet.Transporting.LocalConnectionState, LocalConnectionStateEvent> { }
}
