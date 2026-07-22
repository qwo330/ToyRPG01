using System;
using UnityEngine;

[RequireComponent(typeof(ActorAnimator))]
[RequireComponent(typeof(RemoteMoveController))]
[RequireComponent(typeof(MonsterBrain))]
public class Enemy : Actor
{
    [SerializeField] EnemyData data;

    public EnemyData Data => data;

    MyObjectPool<Enemy> pool;
    Action<Enemy> released;
    RemoteMoveController moveController;
    MonsterBrain brain;

    protected override void Init()
    {
        EnsureSpawnComponents();
        base.Init();
    }

    protected override ActorData GetActorData() => data;

    public void Spawn(Vector3 spawnPoint, Quaternion rotation, MyObjectPool<Enemy> sourcePool, Action<Enemy> onReleased)
    {
        EnsureSpawnComponents();
        transform.SetPositionAndRotation(spawnPoint, rotation);
        InitializeIfNeeded();

        pool = sourcePool;
        released = onReleased;

        HP = MaxHP;
        ResetActorSnapshot(spawnPoint, rotation);

        if (brain != null)
        {
            brain.Init(data);
            brain.ResetBrain(spawnPoint);
        }
    }

    public override void Dead()
    {
        ReturnToPool();
    }

    void ReturnToPool()
    {
        released?.Invoke(this);
        released = null;

        if (pool == null)
        {
            gameObject.SetActive(false);
            return;
        }

        pool.Release(this);
    }

    void EnsureSpawnComponents()
    {
        if (!TryGetComponent<ActorAnimator>(out _))
            gameObject.AddComponent<ActorAnimator>();

        if (moveController == null && !TryGetComponent(out moveController))
            moveController = gameObject.AddComponent<RemoteMoveController>();

        moveController.AllowRelocation = false;

        if (brain == null && !TryGetComponent(out brain))
            brain = gameObject.AddComponent<MonsterBrain>();
    }
}
