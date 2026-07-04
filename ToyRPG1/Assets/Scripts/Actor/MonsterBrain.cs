using UnityEngine;
using Random = UnityEngine.Random;

public enum MonsterAIState
{
    Idle,
    Roaming,
    Chase,
    Attack,
}

public class MonsterBrain : MonoBehaviour
{
    public float RoamRadius => Data.RoamRadius;
    public float DetectRadius => Data.DetectRadius;
    public float LeashRange => Data.LeashRange;
    public float AttackRange => Data.AttackRange;

    MonsterAIState aiState;
    Command lastCommand;

    Vector3 basePoint;
    Vector3 roamTarget;

    Actor target;
    Enemy enemy;
    ActorScanner scanner;

    float nextRoamTime;

    bool IsArrived
    {
        get
        {
            if (roamTarget == Vector3.zero)
                return false;
            
            var dist = Vector3.Distance(transform.position, roamTarget);
            return dist < GlobalValues.IgnoreDistance;
        }
    }

    protected EnemyData Data { get; private set; }

    void Start()
    {
        enemy = gameObject.GetComponent<Enemy>();
        scanner = gameObject.GetComponent<ActorScanner>();

        basePoint = transform.position;
        ChangeState(MonsterAIState.Idle);
    }

    void Update()
    {
        UpdateState();

        var command = CreateCommand();
        if (!ShouldSend(command))
            return;
        
        var currentIndex = enemy.Snapshot.DataIndex;
        var nextIndex = currentIndex == 0 ? 1 : currentIndex + 1;
        
        ActorSnapshot snapshot = new()
        {
            EntityID = enemy.EntityID,
            DataIndex = nextIndex,

            Position = command.MoveDirection,
            Rotation = command.LookRotation,
            State = command.State,

            TargetID = command.TargetID,
            SkillID = command.SkillID
        };

        ActorManager.Instance.AddSnapshot(snapshot);
        lastCommand = command;
    }

    Command CreateCommand()
    {
        return aiState switch
        {
            MonsterAIState.Roaming => ProcessRoaming(),
            MonsterAIState.Chase => ProcessChasing(),
            MonsterAIState.Attack => ProcessAttack(),
            _ => Command.Idle(enemy)
        };
    }
    
    bool ShouldSend(Command command)
    {
        if (command.State != lastCommand.State)
            return true;

        if (command.TargetID != lastCommand.TargetID)
            return true;

        if (command.SkillID != lastCommand.SkillID)
            return true;

        if ((command.MoveDirection - lastCommand.MoveDirection).sqrMagnitude > GlobalValues.HMoveThresholdSqr)
            return true;

        if (Quaternion.Angle(command.LookRotation, lastCommand.LookRotation) > GlobalValues.RotationAngleThreshold)
            return true;

        return false;
    }

    public void Init(ActorData data)
    {
        if (data is EnemyData e)
        {
            Data = e;
        }
    }

    /// <summary>
    /// 타겟이 없다 -> 새 타겟 탐지 -> 없으면 로밍
    /// 타겟이 있다 -> 영역 밖이다 -> 타겟 버린다 -> 로밍
    ///           -> 영역 안이다 -> 공격 범위다 -> 공격 / 추적
    /// 타겟 업데이트 처리는 어디서?
    /// </summary>
    void UpdateState()
    {
        if (target == null)
        {
            UpdateTarget();
        }

        if (target == null)
        {
            ChangeState(MonsterAIState.Roaming);
            return;
        }

        var leashDist = Vector3.Distance(basePoint, transform.position);
        if (leashDist > LeashRange)
        {
            target = null;
            ChangeState(MonsterAIState.Roaming);
            return;
        }

        var targetDist = Vector3.Distance(transform.position, target.transform.position);
        if (targetDist < AttackRange)
        {
            ChangeState(MonsterAIState.Attack);
        }
        else
        {
            ChangeState(MonsterAIState.Chase);
        }
    }

    void ChangeState(MonsterAIState newState)
    {
        aiState = newState;
    }

    void UpdateTarget()
    {
        // todo : target 얻는 로직 개선 필요.
        // 기본 규칙은 가까운 순
        // 아군 & 적군에 대한 타겟 구분 필요, 위협도 우선 필요, 기존 타겟 우선 필요
        target = scanner.GetTarget();
    }

    Command ProcessRoaming()
    {
        // todo : 업데이트 잦은 호출 개선 중
        if (IsArrived)
        {
            // if (Time.time < nextRoamTime)
            // {
            //     return Command.Idle(enemy);
            // }
            //
            // nextRoamTime = Time.time + Data.NextRoamingTime;

            var random = Random.insideUnitSphere * RoamRadius;
            random.SetY(0);

            roamTarget = basePoint + random;
            var dir = roamTarget - transform.position;
            dir.SetY(0);

            return new Command
            {
                IsActive = true,
                State = ActorState.Move,
                MoveDirection = roamTarget,
                LookRotation = QuaternionExtensions.SafeLookRotation(dir, transform.rotation)
            };
        }
    }

    Command ProcessChasing()
    {
        if (target == null)
        {
            MyDebug.LogError("Target is null");
            return Command.Idle(enemy);
        }
        
        var targetPos = target.transform.position;
        var dir = targetPos - transform.position;
        dir.SetY(0);

        return new Command
        {
            IsActive = true,
            State = ActorState.Move,
            MoveDirection = targetPos,
            LookRotation = QuaternionExtensions.SafeLookRotation(dir, transform.rotation)
        };
    }

    Command ProcessAttack()
    {
        if (target == null)
        {
            MyDebug.LogError("Target is null");
            return Command.Idle(enemy);
        }
        
        var dir = target.transform.position - transform.position;
        dir.SetY(0);
        
        return new Command
        {
            State = ActorState.Attack,
            LookRotation = QuaternionExtensions.SafeLookRotation(dir, transform.rotation),
            
            TargetID = target.EntityID,
            SkillID = 0
        };
    }
}
