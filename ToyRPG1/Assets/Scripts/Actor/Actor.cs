using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorAnimator))]
public abstract class Actor : MonoBehaviour
{
    public int EntityID;
    public int MaxHP;
    public int HP;
    
    public ActorSnapshot Snapshot { get; private set; }

    readonly List<IAction> actions = new();

    ActorAnimator actorAnimator;
    bool isInitialized;
    
    void Start()
    {
        InitializeIfNeeded();
    }

    void OnEnable()
    {
        if (isInitialized)
            ActorManager.Instance.AddActor(this);
    }

    void OnDisable()
    {
        if (isInitialized)
            ActorManager.Instance?.RemoveActor(this);
    }

    protected void InitializeIfNeeded()
    {
        if (isInitialized)
            return;

        Init();
    }

    protected virtual void Init()
    {
        isInitialized = true;

        actions.Clear();
        actions.AddRange(gameObject.GetComponents<IAction>());

        var data = GetActorData();
        if (data != null)
        {
            MaxHP = data.MaxHP;
            HP = MaxHP;

            foreach (var action in actions)
            {
                action.Init(data);
            }
        }
        else
        {
            HP = MaxHP;
        }
        
        EntityID = gameObject.GetInstanceID();
        ResetActorSnapshot();
        ActorManager.Instance.AddActor(this);

        if (TryGetComponent(out actorAnimator))
        {
            actorAnimator.Init();
        }
        else
        {
            MyDebug.LogWarning($"Cannot find ActorAnimator component on {gameObject.name}");
        }
    }

    protected virtual ActorData GetActorData() => null;

    public void ApplySnapshot(ActorSnapshot s)
    {
        if (Snapshot.DataIndex >= s.DataIndex)
            return;
        
        Snapshot = s;

        foreach (var action in actions)
        {
            action.Apply(Snapshot);
        }
    }

    public void ProcessActions()
    {
        if (!isActiveAndEnabled)
            return;

        foreach (var action in actions)
        {
            action.Process();
        }
    }

    protected void ResetActorSnapshot(ActorState state = ActorState.Idle)
    {
        ResetActorSnapshot(transform.position, transform.rotation, state);
    }

    protected void ResetActorSnapshot(Vector3 position, Quaternion rotation, ActorState state = ActorState.Idle)
    {
        Snapshot = new ActorSnapshot
        {
            EntityID = EntityID,
            DataIndex = Snapshot.DataIndex + 1,
            Position = position,
            Rotation = rotation,
            State = state
        };

        foreach (var action in actions)
        {
            if (action is IMove move)
                move.Apply(Snapshot);
        }
    }
    
    public abstract void Dead();

    public virtual void TakeDamage(Actor enemy, int power)
    {
        HP -= power;
        
        if (HP <= 0)
        {
            Dead();
        }
    }
}

struct Command
{
    public bool IsActive;
    public int Priority;
    public ActorState State;

    public Vector3 MoveDirection;
    public Quaternion LookRotation;

    public int TargetID;
    public int SkillID;

    public static Command Idle(Actor actor)
    {
        if (actor == null)
            return new Command();

        return new Command
        {
            State = ActorState.Idle,
            MoveDirection = actor.transform.position,
            LookRotation = actor.transform.rotation
        };
    }
}

public enum EActorState
{
    None = 0,
    Idle,
    Run,
    Jump,
    Attack,
    Hit,
    Dead,

    // player 외의 행동
    StandBy = 100, // 대기 모션
    Roam,
}
