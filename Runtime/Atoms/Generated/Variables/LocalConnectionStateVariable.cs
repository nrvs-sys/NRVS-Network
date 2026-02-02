using UnityEngine;
using System;
using FishNet.Transporting;

namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// Variable of type `FishNet.Transporting.LocalConnectionState`. Inherits from `AtomVariable&lt;FishNet.Transporting.LocalConnectionState, LocalConnectionStatePair, LocalConnectionStateEvent, LocalConnectionStatePairEvent, LocalConnectionStateLocalConnectionStateFunction&gt;`.
    /// </summary>
    [EditorIcon("atom-icon-lush")]
    [CreateAssetMenu(menuName = "Unity Atoms/Variables/LocalConnectionState", fileName = "LocalConnectionStateVariable")]
    public sealed class LocalConnectionStateVariable : AtomVariable<FishNet.Transporting.LocalConnectionState, LocalConnectionStatePair, LocalConnectionStateEvent, LocalConnectionStatePairEvent, LocalConnectionStateLocalConnectionStateFunction>
    {
        protected override bool ValueEquals(FishNet.Transporting.LocalConnectionState other) => Value == other;
    }
}
