using System;
using UnityEngine;

[RequireComponent(typeof(RemoteMoveController))]
[RequireComponent(typeof(MonsterBrain))]
public class Enemy : Actor
{
    public EnemyData EnemyData => Data as EnemyData;

    MyObjectPool<Enemy> pool;
    Action<Enemy> released;
    RemoteMoveController moveController;
    MonsterBrain brain;

    protected override void Init()
    {
        EnsureSpawnComponents();
        base.Init();
    }

    public void Spawn(Vector3 spawnPoint, Quaternion rotation, MyObjectPool<Enemy> sourcePool, Action<Enemy> onReleased)
    {
        EnsureSpawnComponents();
        transform.SetPositionAndRotation(spawnPoint, rotation);
        InitializeIfNeeded();

        pool = sourcePool;
        released = onReleased;

        ResetHealth();
        ResetActorSnapshot(spawnPoint, rotation);

        if (brain != null)
        {
            brain.Init(EnemyData);
            brain.ResetBrain(spawnPoint);
        }

        PublishSpawned();
    }

    protected override void OnDeathAnimationEnd()
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
