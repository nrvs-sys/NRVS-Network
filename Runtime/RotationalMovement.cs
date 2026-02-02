using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    /// TODO - re-implement with modern FishNet
    /// TODO - re-implement rollback callbacks
    /// TODO - combine with PositionalMovement Script
    /// TODO - support local transforms?
    public class RotationalMovement : MonoBehaviour
    {
        public Vector3 rotationRate;

        private Rigidbody rb;
        private Vector3 startRotation;

        private bool useRigidbody => rb != null; //&& !rb.isKinematic;

        private bool replaying = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            startRotation = transform.eulerAngles;
        }

        private void OnEnable()
        {
            var timeManager = InstanceFinder.TimeManager;
            if (timeManager != null)
            {
                timeManager.OnPreTick += TimeManager_OnPreTick;
            }

            var predictionManager = InstanceFinder.PredictionManager;
            if (predictionManager != null)
            {
                //predictionManager.OnPreReconcile += PredictionManager_OnPreReconcile;
                //predictionManager.OnPostReconcile += PredictionManager_OnPostReconcile;
                //predictionManager.OnPreReplicateReplay += PredictionManager_OnPreReplicateReplay;
                //predictionManager.OnPostReplicateReplay += PredictionManager_OnPostReplicateReplay;
            }
        }

        private void OnDisable()
        {
            var timeManager = InstanceFinder.TimeManager;
            if (timeManager != null)
            {
                timeManager.OnPreTick -= TimeManager_OnPreTick;
            }

            var predictionManager = InstanceFinder.PredictionManager;
            if (predictionManager != null)
            {
                //predictionManager.OnPreReconcile -= PredictionManager_OnPreReconcile;
                //predictionManager.OnPostReconcile -= PredictionManager_OnPostReconcile;
                //predictionManager.OnPreReplicateReplay -= PredictionManager_OnPreReplicateReplay;
                //predictionManager.OnPostReplicateReplay -= PredictionManager_OnPostReplicateReplay;
            }
        }

        private void Update()
        {
            if (!useRigidbody)
                transform.rotation = GetRotation(GetCurrentTick(), true);
        }

        private void FixedUpdate()
        {
            if (useRigidbody)
                rb.MoveRotation(GetRotation(GetCurrentTick(replaying), !replaying));
        }

        public uint GetCurrentTick(bool replaying = false)
        {
            var timeManager = InstanceFinder.TimeManager;

            if (timeManager == null)
                return 0;

            return !replaying ? timeManager.Tick : timeManager.LastPacketTick.LastRemoteTick;
        }

        public float GetCurrentTime(bool replaying = false)
        {
            var time = Time.time;

            var timeManager = InstanceFinder.TimeManager;
            if (timeManager != null)
                time = (float)timeManager.TicksToTime(GetCurrentTick(replaying));

            return time;
        }

        public Quaternion GetRotation(float time) => Quaternion.Euler(rotationRate * 3 * time + startRotation);

        public Quaternion GetRotation(uint tick, bool interpolate = false)
        {
            var time = Time.time;
            var networkManager = InstanceFinder.NetworkManager;
            var timeManager = networkManager?.TimeManager;

            if (timeManager != null)
            {
                if (interpolate)
                {
                    var percent = (float)timeManager.GetTickPercentAsDouble();
                    var delta = percent == 0 ? 0 : percent / 100f;
                    time = Mathf.Lerp((float)timeManager.TicksToTime(tick), (float)timeManager.TicksToTime(tick + 1), delta);
                }
                else
                    time = (float)timeManager.TicksToTime(tick);
            }

            return GetRotation(time);
        }

        private void TimeManager_OnPreTick()
        {
            var rotation = GetRotation(GetCurrentTick() - 1);

            if (useRigidbody)
                rb.MoveRotation(rotation);
            else
                transform.rotation = rotation;
        }

        private void PredictionManager_OnPreReconcile(uint clientTick, uint serverTick)
        {
            var rotation = GetRotation(GetCurrentTick(true));

            if (useRigidbody)
                rb.MoveRotation(rotation);
            else
                transform.rotation = rotation;
        }

        private void PredictionManager_OnPostReconcile(uint clientTick, uint serverTick)
        {
            var rotation = GetRotation(GetCurrentTick() - 1);

            if (useRigidbody)
                rb.MoveRotation(rotation);
            else
                transform.rotation = rotation;
        }

        private void PredictionManager_OnPreReplicateReplay(uint clientTick, uint serverTick)
        {
            replaying = true;
        }

        private void PredictionManager_OnPostReplicateReplay(uint clientTick, uint serverTick)
        {
            replaying = false;
        }
    }
}
