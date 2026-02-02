using FishNet.Object.Prediction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    public interface IRigidbodyPredictor
    {
        public PredictionRigidbody GetPredictionRigidbody();
    }
}
