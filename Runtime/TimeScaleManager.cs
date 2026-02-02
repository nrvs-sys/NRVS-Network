using FishNet.Component.Prediction;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Serializing;
using FishNet.Transporting;
using GameKit.Dependencies.Utilities;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimeScaleManager : PredictionController<TimeScaleManager.ReplicateData, TimeScaleManager.ReconcileData>
{
    #region Input/Reconcile Structs

    public struct ReplicateData : IReplicateData, IEquatable<ReplicateData>, IDisposable
    {
        /// <summary>
        /// The tick when the input was created on the client.
        /// </summary>
        public uint createdTick;
        /// <summary>
        /// This is set to true if the input was created on the client.
        /// </summary>
        public bool isCreated => createdTick != 0;

        private uint _tick;

        public ReplicateData(uint createdTick) : this() => this.createdTick = createdTick;

        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public override bool Equals(object obj) => obj is ReplicateData other && Equals(other);

        public bool Equals(ReplicateData other) => 
            createdTick == other.createdTick && 
            _tick == other._tick;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + createdTick.GetHashCode();
                hash = hash * 23 + _tick.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ReplicateData left, ReplicateData right) => left.Equals(right);
        public static bool operator !=(ReplicateData left, ReplicateData right) => !left.Equals(right);
    }

    public struct ReconcileData : IReconcileData, IEquatable<ReconcileData>, IDisposable
    {
        public PredictionTimeScale predictionTimeScale;

        public ReconcileData(PredictionTimeScale predictionTimeScale) : this()
        {
            this.predictionTimeScale = predictionTimeScale;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;

        public override bool Equals(object obj) => obj is ReconcileData other && Equals(other);
        public bool Equals(ReconcileData other) => predictionTimeScale.Equals(other.predictionTimeScale) && _tick == other._tick;

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (predictionTimeScale != null ? predictionTimeScale.GetHashCode() : 0);
                hash = hash * 23 + _tick.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ReconcileData left, ReconcileData right) => left.Equals(right);
        public static bool operator !=(ReconcileData left, ReconcileData right) => !left.Equals(right);
    }

    #endregion

    public float timeScale => IsClientOnlyInitialized ? timeScaleSync.Value : predictionTimeScale?.timeScale ?? 1;

    public PredictionTimeScale predictionTimeScale { get; private set; }

    public event Action<bool> OnSystemPaused;

    int _systemPauseRefCount;
    int systemPauseRefCount
	{ 
        get => _systemPauseRefCount;
        set
		{
            var previous = _systemPauseRefCount;

            _systemPauseRefCount = value;

            // only fire on actual pause/unpause
            if ((previous == 0 && _systemPauseRefCount > 0) || (previous == 1 && _systemPauseRefCount == 0))
                OnSystemPaused?.Invoke(IsSystemPaused);
		}
    }
    public bool IsSystemPaused => systemPauseRefCount > 0;
    float LocalPauseFactor => IsSystemPaused ? 0f : 1f;

    readonly SyncVar<float> timeScaleSync = new SyncVar<float>(0f);

    void Awake()
    {
        timeScaleSync.OnChange += TimeScaleSync_OnChange;
        predictionTimeScale = ResettableObjectCaches<PredictionTimeScale>.Retrieve();
        predictionTimeScale.InitializeState();

        Ref.Register<TimeScaleManager>(this);
    }

    void OnDestroy()
    {
        timeScaleSync.OnChange -= TimeScaleSync_OnChange;
        ResettableObjectCaches<PredictionTimeScale>.Store(predictionTimeScale);
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnPreTick += TimeManager_OnPreTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        TimeManager.OnPreTick -= TimeManager_OnPreTick;
    }

    private void TimeManager_OnPreTick()
    {
        if (IsSystemPaused)
        {
            // don’t advance transitions or inputs while paused
            // but keep enforcing the paused physics timescale
            ApplyTimeScale(predictionTimeScale.timeScale);
            return;
        }

        TickInputs();
        CreateReconcile();
    }

    void TimeScaleSync_OnChange(float prev, float next, bool asServer)
    {
        if (IsClientOnlyInitialized)
            ApplyTimeScale(next);
    }

    [Replicate]
    void Replicate(ReplicateData inputs, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable) => Replicate_Impl(inputs, state, channel, inputs.isCreated);

    protected override void ReplicateInputs(ReplicateData inputs, ReplicateState state, Channel channel) => Replicate(inputs, state, channel);

    protected override void GetInputs(out ReplicateData inputs) => inputs = new ReplicateData(IsController ? TimeManager.Tick : 0);

    protected override void PredictInputs(ref ReplicateData inputs, in ReplicateData lastInputs, ReplicateState state)
    {
        base.PredictInputs(ref inputs, lastInputs, state);

        inputs.createdTick = lastInputs.createdTick;
    }

    protected override void ProcessInputs(ReplicateData inputs, ReplicateState state)
    {
        var delta = IsSystemPaused ? 0f : (float)TimeManager.TickDelta;
        predictionTimeScale.Simulate(delta);
        ApplyTimeScale(predictionTimeScale.timeScale);
    }
    public override void CreateReconcile()
    {
        base.CreateReconcile();

        if (shouldReconcileBeCreated)
        {
            GetReconcileState(out var rs);
            Reconciliation(rs);
        }
    }

    protected override void GetReconcileState(out ReconcileData rs) => rs = new ReconcileData(predictionTimeScale);


    [Reconcile]
    void Reconciliation(ReconcileData data, Channel channel = Channel.Unreliable)
    {
        predictionTimeScale.Reconcile(data.predictionTimeScale);
        ApplyTimeScale(predictionTimeScale.timeScale);
    }

    public void TransitionTimeScale(float timeScale, float duration)
    {
        predictionTimeScale.Transition(timeScale, duration);
    }

    public void SetTimeScale(float timeScale)
    {
        predictionTimeScale.Set(timeScale);
    }

    void ApplyTimeScale(float timeScale)
    {
        float finalScale = timeScale * LocalPauseFactor;

        TimeManager.SetPhysicsTimeScale(finalScale);

        // sync ONLY the base value so a local pause doesn’t affect others
        if (IsServerInitialized)
            timeScaleSync.Value = timeScale;
    }

    public void BeginSystemPause()
    {
        systemPauseRefCount++;
        // apply immediately (forces physics to 0 while retaining base slowmo)
        ApplyTimeScale(predictionTimeScale.timeScale);

        Time.timeScale = LocalPauseFactor;
        Debug.Log($"TimeScaleManager: System pause {(systemPauseRefCount == 1 ? "began" : "incremented")}");
    }

    public void EndSystemPause()
    {
        if (systemPauseRefCount > 0)
            systemPauseRefCount--;

        ApplyTimeScale(predictionTimeScale.timeScale);

        Time.timeScale = LocalPauseFactor;
        Debug.Log($"TimeScaleManager: System pause {(systemPauseRefCount == 0 ? "ended" : "decremented")}");
    }

    public void ToggleSystemPause()
    {
        if (IsSystemPaused)
            EndSystemPause();
        else
            BeginSystemPause();
    }
}

public class PredictionTimeScale : IResettable
{
    [Serializable]
    public struct TimeScaleTransition
    {
        public float duration;
        public float remaining;
        public float startTimeScale;
        public float targetTimeScale;
    }

    public float timeScale { get; private set; } = 1f;

    List<TimeScaleTransition> transitions = new();

    ~PredictionTimeScale()
    {
        if (transitions != null)
            CollectionCaches<TimeScaleTransition>.StoreAndDefault(ref transitions);
    }

    public void InitializeState()
    {
        if (transitions == null)
            transitions = CollectionCaches<TimeScaleTransition>.RetrieveList();
    }

    public void ResetState()
    {
        CollectionCaches<TimeScaleTransition>.StoreAndDefault(ref transitions);
    }

    public void Transition(float timeScale, float duration)
    {
        TimeScaleTransition transition = new()
        {
            startTimeScale = this.timeScale,
            targetTimeScale = timeScale,
            duration = duration,
            remaining = duration
        };

        transitions.Add(transition);
    }

    public void Set(float timeScale)
    {
        this.timeScale = timeScale;
        RemoveTransitions();
    }

    public void Simulate(float delta)
    {
        if (transitions == null)
            return;

        for (int i = 0; i < transitions.Count; i++)
        {
            var transition = transitions[i];
            if (transition.remaining > 0)
            {
                float t = 1f - (transition.remaining / transition.duration);
                timeScale = Mathf.Lerp(transition.startTimeScale, transition.targetTimeScale, t);
                transition.remaining -= delta;
                transitions[i] = transition;
            }
            else
            {
                transitions.RemoveAt(i);
                i--;
            }
        }
    }

    public void Reconcile(PredictionTimeScale pts)
    {
        transitions.Clear();
        if (pts.transitions != null)
        {
            foreach (var item in pts.transitions)
                transitions.Add(item);
        }

        timeScale = pts.timeScale;

        ResettableObjectCaches<PredictionTimeScale>.Store(pts);
    }

    public void RemoveTransitions() => transitions.Clear();

    internal List<TimeScaleTransition> GetTransitions() => transitions;

    internal void SetReconcileData(float timeScale, List<TimeScaleTransition> transitions)
    {
        this.timeScale = timeScale;
        this.transitions = transitions;
    }
}

public static class TimeScaleManagerExtensions
{
    public static void WriteTimeScaleTransition(this Writer writer, PredictionTimeScale.TimeScaleTransition transition)
    {
        writer.Write(transition.duration);
        writer.Write(transition.remaining);
        writer.Write(transition.startTimeScale);
        writer.Write(transition.targetTimeScale);
    }

    public static void WritePredictionTimeScale(this Writer writer, PredictionTimeScale predictionTimeScale)
    {
        writer.Write(predictionTimeScale.timeScale);
        writer.WriteList(predictionTimeScale.GetTransitions());
    }

    public static PredictionTimeScale.TimeScaleTransition ReadTimeScaleTransition(this Reader reader)
    {
        var transition = new PredictionTimeScale.TimeScaleTransition
        {
            duration = reader.ReadSingle(),
            remaining = reader.ReadSingle(),
            startTimeScale = reader.ReadSingle(),
            targetTimeScale = reader.ReadSingle()
        };

        return transition;
    }

    public static PredictionTimeScale ReadPredictionTimeScale(this Reader reader)
    {
        var transitions = CollectionCaches<PredictionTimeScale.TimeScaleTransition>.RetrieveList();

        var timeScale = reader.ReadSingle();
        reader.ReadList(ref transitions);

        var pts = ResettableObjectCaches<PredictionTimeScale>.Retrieve();

        pts.SetReconcileData(timeScale, transitions);
        return pts;
    }
}
