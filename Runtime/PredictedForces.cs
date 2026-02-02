//using FishNet.Object;
//using GameKit.Dependencies.Utilities;
//using GameKit.Dependencies.Utilities.Types;
//using FishNet.Managing.Predicting;
//using FishNet.Managing.Timing;
//using log4net.Util;
//using System;
//using System.Collections.Generic;
//using System.Runtime.CompilerServices;
//using UnityEngine;
//using TimeManagerCls = FishNet.Managing.Timing.TimeManager;
//using FishNet.Object.Prediction;

//namespace Network
//{

//        public abstract class PredictedForces : NetworkBehaviour
//        {
//            #region Types.
//            private struct ForceData : IResettable
//            {
//                /// <summary>
//                /// Tick which the collisions happened.
//                /// </summary>
//                public uint Tick;
//                /// <summary>
//                /// Hits for Tick.
//                /// </summary>
//                public HashSet<PredictionRigidbody.EntryData> Entries;

//                public ForceData(uint tick, HashSet<PredictionRigidbody.EntryData> entries)
//                {
//                    Tick = tick;
//                    Entries = entries;
//                }

//                public void InitializeState() { }
//                public void ResetState()
//                {
//                    Tick = TimeManagerCls.UNSET_TICK;
//                    CollectionCaches<PredictionRigidbody.EntryData>.StoreAndDefault(ref Entries);
//                }
//            }
//            #endregion

//            /// <summary>
//            /// Called when a predicted force is applied.
//            /// </summary>
//            public event Action<PredictionRigidbody.EntryData> OnForce;


//            /// <summary>
//            /// How long of collision history to keep. Lower values will result in marginally better memory usage at the cost of collision histories desynchronizing on clients with excessive latency.
//            /// </summary>
//            [Tooltip("How long of collision history to keep. Lower values will result in marginally better memory usage at the cost of collision histories desynchronizing on clients with excessive latency.")]
//            [Range(0.1f, 2f)]
//            [SerializeField]
//            private float historyDuration = 0.5f;

//            /// <summary>
//            /// The colliders on this object.
//            /// </summary>
//            private PredictionRigidbody predictionRigidbody;
//            /// <summary>
//            /// The hits from the last check.
//            /// </summary>
//            private List<PredictionRigidbody.EntryData> entries;
//            /// <summary>
//            /// The history of collider data.
//            /// </summary>
//            private ResettableRingBuffer<ForceData> forceDataHistory;

//            /// <summary>
//            /// True to cache collision histories for comparing start and exits.
//            /// </summary>
//            private bool _useCache => OnForce != null;


//            protected virtual void Awake()
//            {
//                //_colliderDataHistory = ResettableCollectionCaches<ColliderData>.RetrieveRingBuffer();
//                forceDataHistory = new();
//                entries = CollectionCaches<PredictionRigidbody.EntryData>.RetrieveList();
//            }

//            private void OnDestroy()
//            {
//                //ResettableCollectionCaches<ColliderData>.StoreAndDefault(ref _colliderDataHistory);
//                CollectionCaches<PredictionRigidbody.EntryData>.StoreAndDefault(ref entries);
//            }

//            public override void OnStartNetwork()
//            {
//                //Initialize the ringbuffer. Server only needs 1 tick worth of history.
//                uint historyTicks = (base.IsServerStarted) ? 1 : TimeManager.TimeToTicks(historyDuration);
//                forceDataHistory.Initialize((int)historyTicks);

//                //Events needed by server and client.
//                TimeManager.OnPostPhysicsSimulation += TimeManager_OnPostPhysicsSimulation;
//            }

//            public override void OnStartClient()
//            {
//                //Events only needed by the client.
//                PredictionManager.OnPostReplicateReplay += PredictionManager_OnPostReplicateReplay;
//            }

//            public override void OnStopClient()
//            {
//                //Events only needed by the client.
//                PredictionManager.OnPostReplicateReplay -= PredictionManager_OnPostReplicateReplay;

//            }

//            public override void OnStopNetwork()
//            {
//                TimeManager.OnPostPhysicsSimulation -= TimeManager_OnPostPhysicsSimulation;
//            }


//            /// <summary>
//            /// When using TimeManager for physics timing, this is called immediately after the physics simulation has occured for the tick.
//            /// While using Unity for physics timing, this is called during Update, only if a physics frame.
//            /// This may be useful if you wish to run physics differently for stacked scenes.
//            private void TimeManager_OnPostPhysicsSimulation(float delta)
//            {
//                ApplyForces(TimeManager.LocalTick, false);
//            }

//            /// <summary>
//            /// Called after physics is simulated when replaying a replicate method.
//            /// </summary>
//            private void PredictionManager_OnPostReplicateReplay(uint clientTick, uint serverTick)
//            {
//                ApplyForces(clientTick, true);
//            }

//            /// <summary>
//            /// Cleans history up to, while excluding tick.
//            /// </summary>

//            private void CleanHistory(uint tick)
//            {
//                if (_useCache)
//                {
//                    int removeCount = 0;
//                    int historyCount = forceDataHistory.Count;
//                    for (int i = 0; i < historyCount; i++)
//                    {
//                        if (forceDataHistory[i].Tick >= tick)
//                            break;
//                        removeCount++;
//                    }

//                    for (int i = 0; i < removeCount; i++)
//                        forceDataHistory[i].ResetState();
//                    forceDataHistory.RemoveRange(true, removeCount);
//                }
//                //Cache is not used.
//                else
//                {
//                    ClearForceDataHistory();
//                }
//            }

//            /// <summary>
//            /// Checks for any trigger changes;
//            /// </summary>
//            private void ApplyForces(uint tick, bool replay)
//            {
//                //Should not be possible as tick always starts on 1.
//                if (tick == TimeManagerCls.UNSET_TICK)
//                    return;

//                const int INVALID_HISTORY_VALUE = -1;

//                HashSet<PredictionRigidbody.EntryData> current = CollectionCaches<PredictionRigidbody.EntryData>.RetrieveHashSet();
//                HashSet<PredictionRigidbody.EntryData> previous = null;

//                int previousHitsIndex = INVALID_HISTORY_VALUE;
//                /* Server only keeps 1 history so
//                 * if server is started then
//                 * simply clean one. When the server is
//                 * started replay will never be true, so this
//                 * will only call once per tick. */
//                if (base.IsServerStarted && tick > 0)
//                    CleanHistory(tick - 1);

//                if (_useCache)
//                {
//                    if (replay)
//                    {
//                        previousHitsIndex = GetHistoryIndex(tick - 1, false);
//                        if (previousHitsIndex != -1)
//                            previous = forceDataHistory[previousHitsIndex].Entries;
//                    }
//                    //Not replaying.
//                    else
//                    {
//                        if (forceDataHistory.Count > 0)
//                        {
//                            ForceData cd = forceDataHistory[forceDataHistory.Count - 1];
//                            /* If the hit tick one before current then it can be used, otherwise
//                            * use a new collection for previous. */
//                            if (cd.Tick == (tick - 1))
//                                previous = cd.Entries;
//                        }
//                    }
//                }
//                //Not using history, clear it all.
//                else
//                {
//                    ClearForceDataHistory();
//                }

//                /* Previous may not be set here if there were
//                 * no collisions during the previous tick. */

//                // The rotation of the object for box colliders.
//                Quaternion rotation = transform.rotation;

//                // Check each collider for triggers.
//                foreach (var entry in entries)
//                {

//                    //Number of hits from the checks.
//                    int hits;
//                    //if (col is SphereCollider sphereCollider)
//                    //    hits = GetSphereColliderHits(sphereCollider, _interactableLayers);
//                    //else if (col is CapsuleCollider capsuleCollider)
//                    //    hits = GetCapsuleColliderHits(capsuleCollider, _interactableLayers);
//                    //else if (col is BoxCollider boxCollider)
//                    //    hits = GetBoxColliderHits(boxCollider, rotation, _interactableLayers);
//                    //else
//                        hits = 0;

//                    // Check the hits for triggers.
//                    for (int i = 0; i < hits; i++)
//                    {
//                    var entry = this.entries[i];
//                        if (entry == null || entry == entry)
//                            continue;

//                        /* If not in previous then add and
//                         * invoke enter. */
//                        if (previous == null || !previous.Contains(entry))
//                            OnForce?.Invoke(entry);

//                        //Also add to current hits.
//                        current.Add(entry);
//                        //OnStay?.Invoke(entry);
//                    }
//                }

//                if (previous != null)
//                {
//                    //Check for stays and exits.
//                    foreach (Collider col in previous)
//                    {
//                        //If it was in previous but not current, it has exited.
//                        //if (!current.Contains(col))
//                        //    OnExit?.Invoke(col);
//                    }
//                }

//                //If not using the cache then clean up collections.
//                if (_useCache)
//                {
//                    //If not replaying add onto the end. */
//                    if (!replay)
//                    {
//                        AddToEnd();
//                    }
//                    /* If a replay then set current colliders
//                     * to one entry past historyIndex. If the next entry
//                     * beyond historyIndex is for the right tick it can be
//                     * updated, otherwise a result has to be inserted. */
//                    else
//                    {
//                        /* Previous hits was not found in history so we
//                         * cannot assume current results go right after the previousIndex.
//                         * Find whichever index is the closest to tick and return it. 
//                         * 
//                         * If an exact match is not found for tick then the entry just after
//                         * tick will be returned. This will let us insert current hits right
//                         * before that entry. */
//                        if (previousHitsIndex == -1)
//                        {
//                            int currentIndex = GetHistoryIndex(tick, true);
//                            AddDataToIndex(currentIndex);
//                        }
//                        //If previous hits are known then the index to update is right after previous index.
//                        else
//                        {
//                            int insertIndex = (previousHitsIndex + 1);
//                            /* InsertIndex is out of bounds which means
//                             * to add onto the end. */
//                            if (insertIndex >= forceDataHistory.Count)
//                                AddToEnd();
//                            //Not the last entry to insert in the middle.
//                            else
//                                AddDataToIndex(insertIndex);
//                        }

//                        /* Adds data to an index. If the tick
//                         * matches on index with the current tick then
//                         * replace the entry. Otherwise insert to the
//                         * correct location. */
//                        void AddDataToIndex(int index)
//                        {
//                            ForceData colliderData = new(tick, current);
//                            /* If insertIndex is the same tick then replace, otherwise
//                             * put in front of. */
//                            //Replace.
//                            if (forceDataHistory[index].Tick == tick)
//                            {
//                                forceDataHistory[index].ResetState();
//                                forceDataHistory[index] = colliderData;
//                            }
//                            //Insert before.
//                            else
//                            {
//                                forceDataHistory.Insert(index, colliderData);
//                            }
//                        }
//                    }

//                    void AddToEnd()
//                    {
//                        ForceData colliderData = new(tick, current);
//                        forceDataHistory.Add(colliderData);
//                    }

//                }
//                /* If not using caching then store results from this run. */
//                else
//                {
//                    CollectionCaches<PredictionRigidbody.EntryData>.Store(current);
//                }

//                //Returns history index for a tick.
//                /* GetClosest will return the closest match which is
//                 * past lTick if lTick could not be found. */
//                int GetHistoryIndex(uint lTick, bool getClosest)
//                {
//                    for (int i = 0; i < forceDataHistory.Count; i++)
//                    {
//                        uint localTick = forceDataHistory[i].Tick;
//                        if (localTick == lTick)
//                            return i;
//                        /* Tick is too high, any further results
//                         * will also be too high. */
//                        if (localTick > tick)
//                        {
//                            if (getClosest)
//                                return i;
//                            else
//                                return INVALID_HISTORY_VALUE;
//                        }
//                    }

//                    //Fall through.
//                    return INVALID_HISTORY_VALUE;
//                }
//            }

//            /// <summary>
//            /// Resets this NetworkBehaviour so that it may be added to an object pool.
//            /// </summary>
//            public override void ResetState(bool asServer)
//            {
//                base.ResetState(asServer);
//                ClearForceDataHistory();
//            }

//            /// <summary>
//            /// Resets datas in force data history and clears collection.
//            /// </summary>
//            private void ClearForceDataHistory()
//            {
//                foreach (ForceData fd in forceDataHistory)
//                    fd.ResetState();
//                forceDataHistory.Clear();
//            }
//        }

//}
