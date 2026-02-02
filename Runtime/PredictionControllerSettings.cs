using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    [CreateAssetMenu(fileName = "Prediction Controller_ Settings_ New", menuName = "Network/Prediction Controller Settings")]
    public class PredictionControllerSettings : ScriptableObject
    {
        [Header("Client Settings")]

        [field: SerializeField, Min(0), Tooltip("The amount of ticks into the future that a Spectated Client will attempt to predict inputs.")]
        public int ClientInputPredictionTicks { get; private set; } = 0;

        [Header("Server Settings")]

        [field: SerializeField, Min(0), Tooltip("The tick interval on which the Server will send a reconcile state to clients.")]
        public int ReconcileSendInterval { get; private set; } = 5;
    }
}
