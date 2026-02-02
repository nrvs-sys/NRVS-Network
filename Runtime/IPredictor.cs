using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// Full Interface for defining a class to be used with Client Side Prediction / Server Reconciliation.
    /// </summary>
    /// <typeparam name="I">The Input struct.</typeparam>
    /// <typeparam name="R">The Reconciliation struct.</typeparam>
    public interface IPredictor<I, R> where I : struct where R : struct
    {
        public void GetInputs(ref I inputs);

        public void ResetInputs();

        public void PredictInputs(ref I inputs, in I lastInputs, ReplicateState state);

        public void ProcessInputs(in I inputs, uint tick, ReplicateState state);

        public void GetReconcileState(ref R rs);

        public void Reconciliation(in R rs, uint tick);

        public void PreReplicateReplay(uint clientTick, uint serverTick);

        public void PostReplicateReplay(uint clientTick, uint serverTick);

        public void PostReconcile(uint clientTick, uint serverTick);
    }
}
