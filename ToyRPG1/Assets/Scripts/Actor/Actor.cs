using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorAnimator))]
public abstract class Actor : MonoBehaviour
{
    public int EntityID;
    public int MaxHP;
    public int HP;
    
    public ActorSnapshot Snapshot { get; private set; }
    readonly List<IAction> actions = new ();

    ActorAnimator actorAnimator;
    
    void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        actions.AddRange(gameObject.GetComponents<IAction>());
        
        EntityID = gameObject.GetInstanceID();
        ActorManager.Instance.AddActor(this);

        actorAnimator = GetComponent<ActorAnimator>();
        actorAnimator.Init();
    }

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
        foreach (var action in actions)
        {
            action.Process();
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