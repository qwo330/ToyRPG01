using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using static Cysharp.Threading.Tasks.UniTask;

public class Monster : Actor
{
    public float Speed = 6.0f;
    public float RotSpeed = 5.0f;
    public float RoamingRadius = 3.0f;

    public float JumpSpeed = 8.0f;
    public float Gravity = 10.0f;

    public float AttackCoolTime = 2.0f; // 공격 애니메이션 시간
    [Tooltip("제곱된 거리")] public float AttackRange = 1.0f;

    Vector3 startingPoint;
    Vector3 destinationPoint;

    EActorState prevState;
    EActorState currentState;

    Vector3 motion = Vector3.zero;

    MyObjectPool<Monster> pool;
    public MyObjectPool<Monster> Pool
    {
        set => pool = value;
    }
    
    Actor target;
    public Actor Target
    {
        set => target = value;
    }

    [HideInInspector] public Transform Trans;
    Animator playerAnim;
    CharacterController controller;

    void Start()
    {
        Init();
    }

    void Update()
    {
        if (currentState != EActorState.None && currentState != EActorState.Spawn)
        {
            Attack();
            Move();
        }
    }

    void Init()
    {
        Trans = GetComponent<Transform>();
        controller = GetComponent<CharacterController>();
        playerAnim = GetComponentInChildren<Animator>();

        MyDebug.Log("[Monster] Init");
    }

    void Clear(Vector3 spawnPoint)
    {
        Target = null;

        ChangeState(EActorState.None);
        motion = Vector3.zero;

        MyDebug.Log("[Monster] Clear");
    }

    public void SpawnMonster(Vector3 spawnPoint)
    {
        // todo : 스폰 애니메이션 및 몬스터 초기화 진행


        startingPoint = spawnPoint;
        transform.position = spawnPoint;
        MyDebug.Log($"[Monster] spawn, monster pos : {transform.position}");


        Clear(spawnPoint);
        ChangeState(EActorState.Spawn);

        ActivateMonster();
    }

    async UniTaskVoid ActivateMonster()
    {
        // todo : 몬스터 스폰 완료 및 활동 개시
        await UniTask.WaitUntil(() => currentState == EActorState.Idle);
        ChangeState(EActorState.Idle);
    }

    void ChangeState(EActorState nextState)
    {
        MyDebug.Log($"[Monster] Change State : {currentState} -> {nextState}");

        currentState = nextState;
    }

    public override void Move()
    {
        // 전략패턴 사용?
        // 점프는 일단 보류
        if (target == null)
        {
            if (currentState != EActorState.Roam)
            {
                StartRoam();
            }

            Roam();
        }
        else
        {
            MoveTarget();
        }
    }

    void StartRoam()
    {
        destinationPoint = startingPoint + (Random.insideUnitSphere * RoamingRadius);
        ChangeState(EActorState.Roam);
    }

    void Roam()
    {
        // todo : 시작점을 기준으로 배회
        Vector3 forward = destinationPoint - startingPoint;
        forward.y = 0;
        forward = forward.normalized;

        motion = forward * Speed;

        // rotate
        Quaternion rotation = Quaternion.LookRotation(forward);
        Trans.rotation = rotation;

        controller.Move(motion);

        float sqrDist = (destinationPoint - transform.position).sqrMagnitude;
        if (sqrDist < 0.01f) // 도착 검증할 임시 값
        {
            MyDebug.LogError("FINISH");
            ChangeState(EActorState.Idle);
        }
    }

    public void SetTarget(Actor newTarget)
    {
        this.target = newTarget;
    }

    void MoveTarget()
    {
        Vector3 forward = target.transform.position - transform.position;
        forward.y = 0;
        forward = Vector3.forward.normalized;

        Vector3 motion = forward * Speed;
        Quaternion rotation = Quaternion.LookRotation(forward);
        Trans.rotation = rotation;

        // always
        motion.y -= Gravity * Time.deltaTime;
        controller.Move(motion * Time.deltaTime);
        ChangeAnimation(EActorState.Run);
    }

    public bool CheckInAttackRange()
    {
        if (target == null)
        {
            return false;
        }

        float sqrDist = (target.transform.position - transform.position).sqrMagnitude;
        return sqrDist <= AttackRange;
    }

    public override void Attack()
    {
        if (CheckInAttackRange() == false)
        {
            return;
        }

        // todo : 공격 처리와 애니메이션
        MyDebug.LogError("Attack!");
    }

    public void ChangeAnimation(EActorState newState)
    {
        if (prevState != newState)
        {
            MyDebug.Log($"[Monster] Change Animation : {prevState} -> {newState}");

            prevState = newState;
        }

        ChangeAnimationParameter(newState);
    }

    void ChangeAnimationParameter(EActorState newState)
    {
        switch (newState)
        {
            case EActorState.Spawn:
                break;

            default:
            case EActorState.Idle:
                break;
        }
    }

    public override void Dead()
    {
        MyDebug.LogError($"{gameObject.name} 죽음");
        
        OnReturnToPool();
    }
    
    void OnReturnToPool()
    {
        pool.Release(this);
    }
}