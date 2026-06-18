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
    
    public float distOffset = 0.1f;
    public float ChasingRadius = 10f;
    public float WalkSpeed = 6.0f;
    public float RotSpeed = 5.0f;
    public float RoamingRadius = 3.0f;

    public float JumpSpeed = 8.0f;
    public float Gravity = 10.0f;

    public int attackPower = 1;
    
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
        MyDebug.LogError($"Monster Pos Start, {transform.position}");
    }

    void Update()
    {
        if (currentState != EActorState.None)
        {
            Target = scanner.GetTarget();
            
            // MyDebug.LogWarning($"update {gameObject.name} pos : {transform.position}");
            
            if (CheckInAttackRange())
            {
                Attack();
            }
            else
            {
                Move();
            }        
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
        MyDebug.LogError($"Active Monster : {transform.position} -> {startingPoint}");

        await UniTask.WaitUntil(() => gameObject.activeSelf);

        ChangeState(EActorState.Idle);
    }

    // public override void Move()
    public void Move()
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
            
            MyDebug.Log($"[Monster] {gameObject.name} Set Roaming : {transform.position} -> {destinationPoint}");
        }

        // todo : 시작점을 기준으로 배회
        Vector3 normalDirection = (destinationPoint - transform.position).NormalizedXZ();

        motion = normalDirection * WalkSpeed;

        // rotate
        Quaternion rotation = Quaternion.LookRotation(normalDirection);
        Trans.rotation = rotation;

        float sqrDist = (destinationPoint - transform.position).sqrMagnitude;
        if (sqrDist < distOffset)
        {
            ChangeState(EActorState.Idle);
        }
        else
        {
            MyDebug.Log($"{gameObject.name} Roaming ...");
        }
    }

    void MoveTarget()
    {
        Vector3 normalDirection;
        
        if (IsOutOfChasingArea())
        {
            // todo : 원래 위치로
            MyDebug.LogError($"COMEBACK : {gameObject.name}");
            
            target = null;
            normalDirection = (startingPoint - transform.position).NormalizedXZ();
        }
        else
        {
            MyDebug.LogError($"Move to Target : {target.name}");
            
            normalDirection = (target.transform.position - transform.position).NormalizedXZ();
        }        

        motion = normalDirection * WalkSpeed;
        Quaternion rotation = Quaternion.LookRotation(normalDirection);
        Trans.rotation = rotation;

        // always
        // motion.y -= Gravity * Time.deltaTime;
        // controller.Move(motion * Time.deltaTime);
        
        ChangeState(EActorState.Run);
    }

    bool IsOutOfChasingArea()
    {
        float sqrDist = (startingPoint - transform.position).sqrMagnitude;

        return sqrDist > ChasingRadius;
    }

    public bool CheckInAttackRange()
    {
        float sqrDist = (target.transform.position - transform.position).sqrMagnitude;
        return sqrDist <= AttackRange;
    }

    // public override void Attack()
    public void Attack()
    {
        // todo : 공격 처리와 애니메이션
        MyDebug.LogError("Attack!");
        ChangeState(EActorState.Attack);
        
        target.TakeDamage(this, attackPower);
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
    
    // void OnDrawGizmosSelected()
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(startingPoint, Vector3.one * 0.1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(destinationPoint, Vector3.one * 0.1f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(startingPoint, destinationPoint);
    }
    
    #endif
}