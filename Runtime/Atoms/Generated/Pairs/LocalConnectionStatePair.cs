using System;
using UnityEngine;
using FishNet.Transporting;
namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// IPair of type `&lt;FishNet.Transporting.LocalConnectionState&gt;`. Inherits from `IPair&lt;FishNet.Transporting.LocalConnectionState&gt;`.
    /// </summary>
    [Serializable]
    public struct LocalConnectionStatePair : IPair<FishNet.Transporting.LocalConnectionState>
    {
        public FishNet.Transporting.LocalConnectionState Item1 { get => _item1; set => _item1 = value; }
        public FishNet.Transporting.LocalConnectionState Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private FishNet.Transporting.LocalConnectionState _item1;
        [SerializeField]
        private FishNet.Transporting.LocalConnectionState _item2;

        public void Deconstruct(out FishNet.Transporting.LocalConnectionState item1, out FishNet.Transporting.LocalConnectionState item2) { item1 = Item1; item2 = Item2; }
    }
}