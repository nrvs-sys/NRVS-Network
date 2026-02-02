using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Network
{
    /// <summary>
    /// TODO - re-implement with modern FishNet
    /// TODO - re-implement rollback callbacks
    /// TODO - combine with RotationalMovement Script
    /// TODO - support local transforms?
    /// </summary>
    public class PositionalMovement : MonoBehaviour
    {
        public Vector3 positionOffset;

        [Tooltip("How much time (in seconds) it will take to move to the Position Offset and back")]
        public float moveRate = 5f;

        private Rigidbody rb;
        private Vector3 startPosition;

        private bool useRigidbody => rb != null;// && !rb.isKinematic;

        private bool replaying = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            startPosition = transform.position;
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
                transform.position = GetPosition(GetCurrentTick(), true);
        }

        private void FixedUpdate()
        {
            if (useRigidbody)
                rb.MovePosition(GetPosition(GetCurrentTick(replaying), !replaying));
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

        public Vector3 GetPosition(float time) => GetPosition(time, startPosition);

        public Vector3 GetPosition(uint tick, bool interpolate = false)
        {
            var time = Time.time;
            var networkManager = InstanceFinder.NetworkManager;
            var timeManager = networkManager?.TimeManager;

            if (timeManager != null)
            {
                if (interpolate)
                {
                    var delta = (float)timeManager.GetTickPercentAsDouble();
                    time = Mathf.Lerp((float)timeManager.TicksToTime(tick), (float)timeManager.TicksToTime(tick + 1), delta);
                }
                else
                    time = (float)timeManager.TicksToTime(tick);
            }

            return GetPosition(time);
        }

        public Vector3 GetPosition(float time, in Vector3 startPosition)
        {
            var positionOffset = this.positionOffset / 2;
            var moveOffset = (3 * moveRate) / 4;
            return startPosition + positionOffset + positionOffset * Mathf.Sin(((Mathf.PI * 2) / moveOffset) * time);
        }
        
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                var start = transform.position;
                var target = GetPosition((float)UnityEditor.EditorApplication.timeSinceStartup, start);
                var end = GetPosition(moveRate, start);

                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target, 1);
                Gizmos.DrawLine(start, end);
            }
#endif
        }

        private void TimeManager_OnPreTick()
        {
            var position = GetPosition(GetCurrentTick() - 1);

            if (useRigidbody)
                rb.MovePosition(position);
            else
                transform.position = position;
        }

        private void PredictionManager_OnPreReconcile(uint clientTick, uint serverTick)
        {
            var position = GetPosition(GetCurrentTick(true));

            if (useRigidbody)
                rb.MovePosition(position);
            else
                transform.position = position;
        }

        private void PredictionManager_OnPostReconcile(uint clientTick, uint serverTick)
        {
            var position = GetPosition(GetCurrentTick() - 1);

            if (useRigidbody)
                rb.MovePosition(position);
            else
                transform.position = position;
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
