using System;
using UnityEngine;
using Network;
namespace UnityAtoms.BaseAtoms
{
    /// <summary>
    /// IPair of type `&lt;Network.ConnectionToggleBehaviour.ConnectionPhase&gt;`. Inherits from `IPair&lt;Network.ConnectionToggleBehaviour.ConnectionPhase&gt;`.
    /// </summary>
    [Serializable]
    public struct ConnectionPhasePair : IPair<Network.ConnectionToggleBehaviour.ConnectionPhase>
    {
        public Network.ConnectionToggleBehaviour.ConnectionPhase Item1 { get => _item1; set => _item1 = value; }
        public Network.ConnectionToggleBehaviour.ConnectionPhase Item2 { get => _item2; set => _item2 = value; }

        [SerializeField]
        private Network.ConnectionToggleBehaviour.ConnectionPhase _item1;
        [SerializeField]
        private Network.ConnectionToggleBehaviour.ConnectionPhase _item2;

        public void Deconstruct(out Network.ConnectionToggleBehaviour.ConnectionPhase item1, out Network.ConnectionToggleBehaviour.ConnectionPhase item2) { item1 = Item1; item2 = Item2; }
    }
}