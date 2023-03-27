using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using static Cysharp.Threading.Tasks.UniTask;

public class Monster : Actor
{
    [SerializeField] ActorScanner scanner;
    
    public float WalkSpeed = 6.0f;
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

    Dictionary<Actor, int> actorContributionDictionary; // 각 액터의 기여도 및 위협도 관리

    [SerializeField] Actor target;
    public Actor Target
    {
        set => target = value;
    }

    [HideInInspector] public Transform Trans;
    Animator monsterAnim;
    CharacterController controller;

    bool isInit = false;
    
    static readonly int HASH_RUN = Animator.StringToHash("Run");
    static readonly int HASH_ATTACK = Animator.StringToHash("Attack");
    static readonly int HASH_HIT = Animator.StringToHash("Hit");
    static readonly int HASH_DEAD = Animator.StringToHash("Dead");
    
    void Start()
    {
        Init();
        MyDebug.LogError("Monster Pos Start");
    }

    void Update()
    {
        if (currentState != EActorState.None)
        {
            Target = scanner.GetTarget();
            
            Attack();
            Move();
        }
    }

    public void Init()
    {
        if (isInit)
        {
            return;
        }
        
        isInit = true;

        Trans = GetComponent<Transform>();
        controller = GetComponent<CharacterController>();
        monsterAnim = GetComponentInChildren<Animator>();

        actorContributionDictionary = new Dictionary<Actor, int>();

        MyDebug.Log("[Monster] Init");
    }

    void ClearData()
    {
        Target = null;
        
        ChangeState(EActorState.None);
        motion = Vector3.zero;

        actorContributionDictionary.Clear();
        
        HP = MaxHP;
        startingPoint = Vector3.zero;
        
        MyDebug.Log("[Monster] Clear Data");
    }

    public void SpawnMonster(Vector3 spawnPoint)
    {
        // todo : 스폰 애니메이션 및 몬스터 초기화 진행
        Init();
        ClearData();
        
        startingPoint = spawnPoint;
        
        ActivateMonster();
        
        MyDebug.Log($"[Monster] spawn monster");
    }

    async UniTaskVoid ActivateMonster()
    {
        // todo : 몬스터 스폰 완료 및 활동 개시
        transform.position = startingPoint;

        await UniTask.WaitUntil(() => gameObject.activeSelf);

        ChangeState(EActorState.Idle);
    }

    public override void Move()
    {
        motion = Vector3.zero;
        
        // 전략패턴 사용?
        // 점프는 일단 보류

        if (controller.isGrounded)
        {
            if (target == null)
            {
                Roam();
            }
            else
            {
                MoveTarget();
            }
        }

        // always
        UseGravity();

        controller.Move(motion * Time.deltaTime);
    }

    void UseGravity()
    {
        if (controller.isGrounded)
        {
            return;
        }
        
        motion.y -= Gravity * Time.deltaTime;
    }
    
    void Roam()
    {
        if (currentState != EActorState.Roam)
        {
            Vector3 insidePoint = Random.insideUnitCircle.ConvertXYtoXZ();

            destinationPoint = startingPoint + (insidePoint * RoamingRadius);
            ChangeState(EActorState.Roam);
            MyDebug.LogError($"Roaming {startingPoint} -> {destinationPoint}");
        }
        
        // todo : 시작점을 기준으로 배회
        Vector3 normalDirection = (destinationPoint - startingPoint).NormalizedXZ();
        
        motion = normalDirection * WalkSpeed;

        // rotate
        Quaternion rotation = Quaternion.LookRotation(normalDirection);
        Trans.rotation = rotation;

        //controller.Move(motion);

        float sqrDist = (destinationPoint - transform.position).sqrMagnitude;
        MyDebug.LogError($"Check Finish : {sqrDist}");
        if (sqrDist < 2.4f) // 도착 검증할 임시 값
        {
            MyDebug.LogError("Roaming FINISH");
            ChangeState(EActorState.Idle);
        }
        else
        {
            MyDebug.LogError("Roaming ...");
        }
    }

    void MoveTarget()
    {
        MyDebug.LogError($"Move to Target : {target.name}");

        Vector3 normalDirection = (target.transform.position - transform.position).NormalizedXZ();

        motion = normalDirection * WalkSpeed;
        Quaternion rotation = Quaternion.LookRotation(normalDirection);
        Trans.rotation = rotation;

        // always
        // motion.y -= Gravity * Time.deltaTime;
        // controller.Move(motion * Time.deltaTime);
        
        ChangeState(EActorState.Run);
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
        ChangeState(EActorState.Attack);
    }

    public override void TakeDamage(Actor enemy, int power)
    {
        UpdateContribution(enemy, power);
        base.TakeDamage(enemy, power);
    }

    void UpdateContribution(Actor actor, int power)
    {
        if (actorContributionDictionary.ContainsKey(actor))
        {
            actorContributionDictionary[actor] += power;
        }
        else
        {
            actorContributionDictionary[actor] = power;
        }
    }
    
    void ChangeState(EActorState nextState)
    {
        MyDebug.Log($"[Monster] Change State : {currentState} -> {nextState}");

        currentState = nextState;
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
            case EActorState.Idle:
                monsterAnim.SetFloat(HASH_RUN, 0);
                break;
            
            case EActorState.Run:
            case EActorState.Roam:
                monsterAnim.SetFloat(HASH_RUN, 1);
                break;
            
            case EActorState.Attack:
                monsterAnim.SetTrigger(HASH_ATTACK);
                break;
            
            case EActorState.Hit:
                monsterAnim.SetTrigger(HASH_HIT);
                break;
            
            case EActorState.Dead:
                monsterAnim.SetBool(HASH_DEAD, true);
                break;

            default:
                MyDebug.LogWarning($"UnDefined State : {newState}");
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

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startingPoint, destinationPoint);
    }
    
    #endif
}