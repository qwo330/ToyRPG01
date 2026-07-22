using UnityEngine;
using Random = UnityEngine.Random;

public enum MonsterAIState
{
    Idle,
    Roaming,
    Chase,
    Attack,
    Returning,
}

public class MonsterBrain : MonoBehaviour
{
    const float DefaultRoamRadius = 5f;
    const float DefaultDetectRadius = 10f;
    const float DefaultLeashRange = 15f;
    const float DefaultAttackRange = 1f;
    const float DefaultRoamDelay = 0.5f;
    const float DefaultAttackCooldown = 1f;
    const float ArriveThresholdSqr = 0.04f; // 0.2f * 0.2f
    const int DefaultTargetLayerMask = GameLayers.PlayerMask;

    public float RoamRadius => Data != null ? Data.RoamRadius : DefaultRoamRadius;
    public float DetectRadius => Data != null ? Data.DetectRadius : DefaultDetectRadius;
    public float LeashRange => Data != null ? Data.LeashRange : DefaultLeashRange;
    public float AttackRange => Data != null ? Data.AttackRange : DefaultAttackRange;
    public LayerMask TargetLayers => Data != null && Data.TargetLayers.value != 0 ? Data.TargetLayers : DefaultTargetLayerMask;

    protected EnemyData Data { get; private set; }

    float RoamDelay => Data != null && Data.NextRoamingTime > 0f ? Data.NextRoamingTime : DefaultRoamDelay;
    float AttackCooldown => Data != null && Data.AttackCooltime > 0f ? Data.AttackCooltime : DefaultAttackCooldown;
    bool HasMoveTarget => hasMoveTarget;
    bool IsArrived => HasMoveTarget && GetHorizontalSqrDistance(transform.position, targetPosition) <= ArriveThresholdSqr;

    MonsterAIState aiState;
    Command lastCommand;

    Vector3 homePoint;
    Vector3 targetPosition;
    Vector3 lastChaseTargetPosition;
    bool hasMoveTarget;
    bool isReady;

    Actor target;
    Enemy enemy;
    ActorScanner scanner;

    float nextRoamTime;
    float nextAttackTime;

    void Awake()
    {
        CacheComponents();
    }

    void Start()
    {
        CacheComponents();
        if (Data == null && enemy != null)
        {
            Init(enemy.Data);
        }
        else
        {
            ConfigureScanner();
        }

        ResetBrain(transform.position);
    }

    void Update()
    {
        if (!isReady)
            return;

        UpdateKnownTarget();
        UpdatePerception();
        ChangeState(EvaluateTransition());
        TickState();
    }

    public void Init(ActorData data)
    {
        CacheComponents();

        if (data is EnemyData e)
        {
            Data = e;
            ConfigureScanner();
        }
    }

    public void ResetBrain(Vector3 pos)
    {
        homePoint = pos;
        ClearMoveTarget();
        lastChaseTargetPosition = Vector3.zero;
        target = null;
        lastCommand = default;

        aiState = MonsterAIState.Idle;
        nextRoamTime = Time.time + RoamDelay;
        nextAttackTime = 0f;
    }

    void CacheComponents()
    {
        if (enemy == null)
            enemy = gameObject.GetComponent<Enemy>();

        if (scanner == null)
            scanner = gameObject.GetComponent<ActorScanner>();

        isReady = enemy != null;
    }

    void ConfigureScanner()
    {
        if (scanner == null)
            return;

        scanner.Configure(DetectRadius, TargetLayers);
    }

    void UpdateKnownTarget()
    {
        if (target.IsNullOrDestroyed())
            target = null;
    }

    void UpdatePerception()
    {
        if (target != null || scanner == null || aiState == MonsterAIState.Returning)
            return;

        target = scanner.GetTarget();
    }

    MonsterAIState EvaluateTransition()
    {
        if (aiState == MonsterAIState.Returning)
            return IsArrived ? MonsterAIState.Idle : MonsterAIState.Returning;

        if (IsOutLeashRange())
        {
            target = null;
            return MonsterAIState.Returning;
        }

        if (target != null)
        {
            return IsInAttackRange() ? MonsterAIState.Attack : MonsterAIState.Chase;
        }

        if (aiState == MonsterAIState.Roaming)
            return IsArrived ? MonsterAIState.Idle : MonsterAIState.Roaming;

        if (Time.time >= nextRoamTime)
            return MonsterAIState.Roaming;

        return MonsterAIState.Idle;
    }

    void ChangeState(MonsterAIState newState)
    {
        if (aiState == newState)
            return;

        aiState = newState;
        EnterState(newState);
    }

    void EnterState(MonsterAIState state)
    {
        switch (state)
        {
            case MonsterAIState.Idle:
                EnterIdle();
                break;
            case MonsterAIState.Roaming:
                EnterRoaming();
                break;
            case MonsterAIState.Returning:
                EnterReturning();
                break;
            case MonsterAIState.Chase:
                SendChaseCommand(force: true);
                break;
            case MonsterAIState.Attack:
                SendAttackCommand(force: true);
                break;
            default:
                MyDebug.LogWarning($"Not Defined MonsterAIState: {state}");
                EnterIdle();
                break;
        }
    }

    void TickState()
    {
        switch (aiState)
        {
            case MonsterAIState.Roaming:
                TickRoaming();
                break;

            case MonsterAIState.Returning:
                TickReturning();
                break;

            case MonsterAIState.Chase:
                TickChase();
                break;

            case MonsterAIState.Attack:
                TickAttack();
                break;
        }
    }

    void EnterIdle()
    {
        ClearMoveTarget();
        nextRoamTime = Time.time + RoamDelay;
        SendCommandIfChanged(Command.Idle(enemy));
    }

    void EnterRoaming()
    {
        SetMoveTarget(GetNextPosition());
        SendMoveCommand(force: true);
    }

    void EnterReturning()
    {
        target = null;
        SetMoveTarget(homePoint);
        SendMoveCommand(force: true);
    }

    void TickRoaming()
    {
        if (!IsArrived)
            return;

        ChangeState(MonsterAIState.Idle);
    }

    void TickReturning()
    {
        if (!IsArrived)
            return;

        ChangeState(MonsterAIState.Idle);
    }

    void TickChase()
    {
        if (target == null)
        {
            ChangeState(IsOutLeashRange() ? MonsterAIState.Returning : MonsterAIState.Roaming);
            return;
        }

        var pos = target.transform.position;
        if (GetHorizontalSqrDistance(pos, lastChaseTargetPosition) < GlobalValues.HMoveThresholdSqr)
            return;

        SendChaseCommand();
    }

    void TickAttack()
    {
        if (target == null)
        {
            ChangeState(IsOutLeashRange() ? MonsterAIState.Returning : MonsterAIState.Roaming);
            return;
        }

        if (Time.time < nextAttackTime)
            return;

        SendAttackCommand(force: true);
    }

    void SendMoveCommand(bool force = false)
    {
        var dir = targetPosition - transform.position;
        dir.SetY(0);

        var command = new Command
        {
            IsActive = true,
            State = ActorState.Move,
            MoveDirection = targetPosition,
            LookRotation = QuaternionExtensions.SafeLookRotation(dir, transform.rotation)
        };

        SendCommandIfChanged(command, force);
    }

    void SetMoveTarget(Vector3 position)
    {
        targetPosition = position;
        hasMoveTarget = true;
    }

    void ClearMoveTarget()
    {
        targetPosition = Vector3.zero;
        hasMoveTarget = false;
    }

    void SendChaseCommand(bool force = false)
    {
        if (target == null)
            return;

        var pos = target.transform.position;
        var dir = pos - transform.position;
        dir.SetY(0);

        var command = new Command
        {
            IsActive = true,
            State = ActorState.Move,
            MoveDirection = pos,
            LookRotation = QuaternionExtensions.SafeLookRotation(dir, transform.rotation)
        };

        SendCommandIfChanged(command, force);
        lastChaseTargetPosition = pos;
    }

    void SendAttackCommand(bool force = false)
    {
        if (target == null)
            return;

        var dir = target.transform.position - transform.position;
        dir.SetY(0);

        var command = new Command
        {
            State = ActorState.Attack,
            MoveDirection = transform.position,
            LookRotation = QuaternionExtensions.SafeLookRotation(dir, transform.rotation),
            TargetID = target.EntityID,
            SkillID = 0
        };

        SendCommandIfChanged(command, force);
        nextAttackTime = Time.time + AttackCooldown;
    }

    void SendCommandIfChanged(Command command, bool force = false)
    {
        if (!force && !ShouldSend(command))
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

    bool ShouldSend(Command command)
    {
        if (command.State != lastCommand.State)
            return true;

        if (command.TargetID != lastCommand.TargetID)
            return true;

        if (command.SkillID != lastCommand.SkillID)
            return true;

        if (GetHorizontalSqrDistance(command.MoveDirection, lastCommand.MoveDirection) > GlobalValues.HMoveThresholdSqr)
            return true;

        if (Quaternion.Angle(command.LookRotation, lastCommand.LookRotation) > GlobalValues.RotationAngleThreshold)
            return true;

        return false;
    }

    Vector3 GetNextPosition()
    {
        var random = Random.insideUnitSphere * RoamRadius;
        random.SetY(0);

        return homePoint + random;
    }

    bool IsOutLeashRange()
    {
        return GetHorizontalSqrDistance(transform.position, homePoint) > LeashRange * LeashRange;
    }

    bool IsInAttackRange()
    {
        if (target == null)
            return false;

        return GetHorizontalSqrDistance(target.transform.position, transform.position) <= AttackRange * AttackRange;
    }

    static float GetHorizontalSqrDistance(Vector3 a, Vector3 b)
    {
        var dx = a.x - b.x;
        var dz = a.z - b.z;

        return dx * dx + dz * dz;
    }

#if UNITY_EDITOR

    static readonly Color AttackRangeGizmoColor = Color.red;
    static readonly Color DetectRangeGizmoColor = Color.yellow;
    static readonly Color LeashRangeGizmoColor = Color.green;

    void OnDrawGizmos()
    {
        var data = GetGizmoData();
        var attackRange = data != null ? data.AttackRange : DefaultAttackRange;
        var detectRadius = data != null ? data.DetectRadius : DefaultDetectRadius;
        var leashRange = data != null ? data.LeashRange : DefaultLeashRange;

        DrawRangeGizmo(GetLeashGizmoCenter(), leashRange, LeashRangeGizmoColor);
        DrawRangeGizmo(detectRadius, DetectRangeGizmoColor);
        DrawRangeGizmo(attackRange, AttackRangeGizmoColor);
    }

    EnemyData GetGizmoData()
    {
        if (Data != null)
            return Data;

        return TryGetComponent(out Enemy e) ? e.Data : null;
    }

    Vector3 GetLeashGizmoCenter()
    {
        return Application.isPlaying ? homePoint : transform.position;
    }

    void DrawRangeGizmo(float radius, Color color)
    {
        DrawRangeGizmo(transform.position, radius, color);
    }

    void DrawRangeGizmo(Vector3 center, float radius, Color color)
    {
        if (radius <= 0f)
            return;

        Gizmos.color = color;
        Gizmos.DrawWireSphere(center, radius);
    }
#endif
}
