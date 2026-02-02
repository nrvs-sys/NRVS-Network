using System;
using UnityEngine;
using Network;
namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// IPair of type `&lt;Network.NetworkState&gt;`. Inherits from `IPair&lt;Network.NetworkState&gt;`.
    /// </summary>
    [Serializable]
    public struct NetworkStatePair : IPair<Network.NetworkState>
    {
        public Network.NetworkState Item1 { get => _item1; set => _item1 = value; }
        public Network.NetworkState Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private Network.NetworkState _item1;
        [SerializeField]
        private Network.NetworkState _item2;

        public void Deconstruct(out Network.NetworkState item1, out Network.NetworkState item2) { item1 = Item1; item2 = Item2; }
    }
}