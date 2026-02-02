using FishNet;
using FishNet.CodeGenerating;
using FishNet.Component.Prediction;
using FishNet.Connection;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;

namespace Network
{

    /// <summary>
    /// Interface for defining a class to be used with Client Side Prediction / Server Reconciliation with the PlayerController class.
    /// </summary>
    //public interface IPredictionControllerBehavior : IPredictor<PredictionController.InputState, PredictionController.ReconcileState>
    //{
    //    public void InitializePlayerPredictor(PredictionController predictionController);
    //}

    public abstract class PredictionController<ReplicateData, ReconcileData> : NetworkBehaviour where ReplicateData : struct, IReplicateData where ReconcileData : struct, IReconcileData
    {
        public enum NonPredictedAuthority
        {
            Owner, 
            Server, 
            Both
        }

        #region Serialized Fields

        [Header("Prediction Controller Settings")]

        [Tooltip("When not using Client Side Prediction, who has authority Process Ticks?")]
        public NonPredictedAuthority nonPredictedAuthority = NonPredictedAuthority.Server;

        [Header("Prediction Controller References")]

        [SerializeField, Required]
        protected PredictionControllerSettings predictionControllerSettings;

        #endregion

        #region State Fields

        protected ReplicateData lastInputs = default;

        TimeScaleManager _timeScaleManager;
        protected TimeScaleManager timeScaleManager => _timeScaleManager != null ? _timeScaleManager : (_timeScaleManager = Ref.Get<TimeScaleManager>());

        #endregion

        #region QOL Properties

        protected bool shouldReconcileBeCreated => NetworkObject.EnablePrediction && (base.IsClientOnlyInitialized || base.IsServerInitialized && base.TimeManager.LocalTick % predictionControllerSettings.ReconcileSendInterval == 0);

        protected bool hasTickAuthority => NetworkObject.EnablePrediction || hasNonPredictedAuthority;

        protected bool hasNonPredictedAuthority => !NetworkObject.EnablePrediction && (nonPredictedAuthority == NonPredictedAuthority.Both || (nonPredictedAuthority == NonPredictedAuthority.Owner && IsOwner) || (nonPredictedAuthority == NonPredictedAuthority.Server && IsServerInitialized));

        #endregion


        #region Input/Reconciliation Methods

        protected void TickInputs()
        {
            if (timeScaleManager != null && timeScaleManager.IsSystemPaused)
                return;

            GetInputs(out ReplicateData inputs);

            if (NetworkObject.EnablePrediction)
                ReplicateInputs(inputs);
            else if (hasNonPredictedAuthority)
            {
                ProcessInputs(inputs, ReplicateState.Created | ReplicateState.Ticked);
            }
        }

        protected abstract void GetInputs(out ReplicateData inputs);

        protected virtual void PredictInputs(ref ReplicateData inputs, in ReplicateData lastInputs, ReplicateState state) { }

        protected virtual void ResetInputs() { }

        protected abstract void ReplicateInputs(ReplicateData inputs, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable);

        protected void Replicate_Impl(ReplicateData inputs, ReplicateState state, Channel channel, bool inputsCreated = true)
        {
            //Debug.Log($"Player  Process Inputs - Tick: {inputs.GetTick()}, State: {state}");

            //if (IsOwner)
            //    Debug.Log($"Player {displayName.Value} Process Owned Inputs - Tick: {inputs.GetTick()}, State: {state}");

            // If the input state is invalid, do not process it.
            if (!state.IsValid())
            {
                OnReplicateInputsNotProcessed(inputs, state);
                return;
            }

            if (predictionControllerSettings.ClientInputPredictionTicks > 0)
            {
                #region Input Prediction

                // If ticked, then set the last inputs cache.
                // Ticked means the replicate is being run from the tick cycle,
                // or specifically NOT from a replay/reconcile.
                if (inputsCreated && state.ContainsTicked())
                {
                    //If ReplicateData contains fields which could generate garbage you
                    //probably want to dispose of the lastInputs
                    //before replacing it. This step is optional.
                    //lastInputs.Dispose();
                    //Assign newest value as last.
                    lastInputs = inputs;
                }
                // If the state is in the Future, we dont have any inputs from the Owned client yet.
                // We will predict the inputs for the next `inputPredictionTicks` ticks.
                else if (state.IsFuture())
                {
                    uint lastInputsTick = lastInputs.GetTick();
                    uint inputsTick = inputs.GetTick();

                    // If the current input Tick is less than or equal to the value defined by the Prediction Controller Settings,
                    // then we want to predict the inputs
                    if ((inputsTick - lastInputsTick) <= predictionControllerSettings.ClientInputPredictionTicks)
                    {
                        PredictInputs(ref inputs, in lastInputs, state);
                    }
                    // If the current input Tick is greater than the value defined by `inputPredictionTicks`,
                    // then we do not want to predict any more ticks and will exit early
                    else
                    {
                        OnReplicateInputsNotProcessed(inputs, state);
                        return;
                    }
                }

                #endregion
            }
            else
            {
                // If the owner did not actually create the input state, then we do not want to process it.
                // This is because the owner's actual input state is not available yet, so simulating with empty input data could cause desyncs.
                // This situation should only occur on non-owned clients.
                // See the `predictionControllerSettings.ClientInputPredictionTicks` setting if input prediction is desired.
                if (state.IsFuture() || !state.ContainsCreated())
                {
                    OnReplicateInputsNotProcessed(inputs, state);
                    return;
                }
            }

            //if (IsBehaviourReconciling && !IsOwner)
            //    Debug.Log($"Process Non-Owned Inputs - Tick: {inputs.GetTick()}, State: {state}");

            ProcessInputs(inputs, state);
        }

        protected virtual void OnReplicateInputsNotProcessed(ReplicateData inputs, ReplicateState state = ReplicateState.Invalid) { }

        protected abstract void ProcessInputs(ReplicateData inputs, ReplicateState state);

        protected abstract void GetReconcileState(out ReconcileData rs);


        #endregion
    }
}
