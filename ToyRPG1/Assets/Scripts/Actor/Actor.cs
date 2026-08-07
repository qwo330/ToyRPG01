using System.Collections.Generic;
using R3;
using UnityEngine;

[RequireComponent(typeof(ActorAnimator))]
public abstract class Actor : MonoBehaviour
{
    public int EntityID;
    public int MaxHP;
    public int HP;
    
    public ActorSnapshot Snapshot { get; private set; }
    public bool IsDead => isDead;
    public Observable<ActorSpawnEvent> OnSpawned => spawnedSubject;
    public Observable<ActorDespawnEvent> OnDespawned => despawnedSubject;
    public Observable<ActorHitEvent> OnHit => hitSubject;
    public Observable<ActorDeadEvent> OnDead => deadSubject;
    public Observable<ActorHPChangedEvent> OnHealthChanged => healthChangedSubject;
    public Observable<ActorSnapshot> OnSnapshotChanged => snapshotChangedSubject;

    readonly List<IAction> actions = new();
    readonly Subject<ActorSpawnEvent> spawnedSubject = new();
    readonly Subject<ActorDespawnEvent> despawnedSubject = new();
    readonly Subject<ActorHitEvent> hitSubject = new();
    readonly Subject<ActorDeadEvent> deadSubject = new();
    readonly Subject<ActorHPChangedEvent> healthChangedSubject = new();
    readonly Subject<ActorSnapshot> snapshotChangedSubject = new();

    ActorAnimator actorAnimator;
    bool isInitialized;
    bool isSpawned;
    bool isDead;

    void Start()
    {
        InitializeIfNeeded();
        PublishSpawned();
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

        PublishDespawned();
    }

    void OnDestroy()
    {
        spawnedSubject.Dispose();
        despawnedSubject.Dispose();
        hitSubject.Dispose();
        deadSubject.Dispose();
        healthChangedSubject.Dispose();
        snapshotChangedSubject.Dispose();
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

        isDead = false;
        
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

        PublishSnapshotChanged();
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

        PublishSnapshotChanged();
    }
    
    public void Dead()
    {
        var previousHP = HP;
        if (HP > 0)
        {
            HP = 0;
            PublishHealthChanged(null, previousHP, HP);
        }

        TryDead(null, previousHP, previousHP);
    }

    public virtual void TakeDamage(Actor attacker, int power)
    {
        if (isDead)
            return;

        var damage = Mathf.Max(0, power);
        if (damage == 0)
            return;

        var previousHP = HP;
        HP = Mathf.Max(0, HP - damage);

        PublishHealthChanged(attacker, previousHP, HP);
        hitSubject.OnNext(new ActorHitEvent(this, attacker, damage, previousHP, HP, MaxHP));
        
        if (HP <= 0)
        {
            TryDead(attacker, previousHP, damage);
        }
    }

    protected abstract void OnDeadCore();

    protected void ResetHealth()
    {
        var previousHP = HP;
        HP = MaxHP;
        isDead = false;
        PublishHealthChanged(null, previousHP, HP);
    }

    protected void PublishSpawned()
    {
        if (isSpawned)
            return;

        isSpawned = true;
        spawnedSubject.OnNext(new ActorSpawnEvent(this, transform.position, transform.rotation, Snapshot));
    }

    bool TryDead(Actor attacker, int previousHP, int damage)
    {
        if (isDead)
            return false;

        isDead = true;
        deadSubject.OnNext(new ActorDeadEvent(this, attacker, damage, previousHP, HP, MaxHP));
        OnDeadCore();
        return true;
    }

    void PublishDespawned()
    {
        if (!isSpawned)
            return;

        isSpawned = false;
        despawnedSubject.OnNext(new ActorDespawnEvent(this, Snapshot));
    }

    void PublishHealthChanged(Actor source, int previousHP, int currentHP)
    {
        if (previousHP == currentHP)
            return;

        healthChangedSubject.OnNext(new ActorHPChangedEvent(this, source, previousHP, currentHP, MaxHP));
    }

    void PublishSnapshotChanged()
    {
        snapshotChangedSubject.OnNext(Snapshot);
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
