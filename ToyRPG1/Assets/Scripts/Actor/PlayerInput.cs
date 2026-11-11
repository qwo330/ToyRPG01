using System;
using R3;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    const int BasicAttackSkillID = 0;

    Camera Cam => Camera.main;
    bool IsAttacking => attackState.IsAttacking;

    ComboAttackState attackState;
    Player player;
    ActorScanner scanner;
    Actor target;
    uint nextSkillSequence;

    void Start()
    {
        if (gameObject.TryGetComponent<Player>(out var p))
        {
            player = p;

            if (!gameObject.TryGetComponent(out scanner))
            {
                MyDebug.LogError($"Cannot find ActorScanner component on {gameObject.name}");
                enabled = false;
                return;
            }

            player.OnSpawned.Subscribe(_ => ResetInputState()).AddTo(this);
            ResetInputState();
        }
        else
        {
            MyDebug.LogError("Cannot find player in scene!");
            enabled = false;
        }
    }

    void ResetInputState()
    {
        attackState = default;
        attackState.Init(player.GetAttackTimings(), player.GetAttackCooldown());
        target = null;
    }

    void Update()
    {
        if (player == null || !player.CanAct)
            return;

        Span<PlayerInputCommand> commands = stackalloc PlayerInputCommand[]
        {
            EvaluateAttackCommand(priority: 4),
            // EvaluateJumpCommand(priority: 3),
            EvaluateMoveCommand(priority: 2)
        };

        var finalCommand = new PlayerInputCommand
        {
            State = ActorState.Idle,
            MoveDirection = Vector3.zero,
            LookRotation = transform.rotation
        };

        var maxPriority = -1;
        for (var i = 0; i < commands.Length; i++)
        {
            if (!commands[i].IsActive || commands[i].Priority <= maxPriority)
                continue;

            maxPriority = commands[i].Priority;
            finalCommand = commands[i];
        }

        var currentIndex = player.Snapshot.DataIndex;
        var nextIndex = currentIndex == 0 ? 1 : currentIndex + 1;
        
        var snapshotPosition = finalCommand.State == ActorState.Move
            ? finalCommand.MoveDirection
            : transform.position;

        ActorSnapshot s = new()
        {
            EntityID = player.EntityID,
            DataIndex = nextIndex,
            Position = snapshotPosition,
            Rotation = finalCommand.LookRotation,
            State = finalCommand.State,
            Combo = attackState.Combo
        };
        
        ActorManager.Instance.AddSnapshot(s);

        if (attackState.IsComboStartedThisTick)
        {
            ActorManager.Instance.AddSkillCommand(new SkillCommand
            {
                SourceID = player.EntityID,
                Sequence = ++nextSkillSequence,
                SkillID = BasicAttackSkillID,
                ComboIndex = attackState.Combo,
                Target = new SkillTarget
                {
                    Type = SkillTargetType.None
                }
            });
        }
    }

    PlayerInputCommand EvaluateMoveCommand(int priority)
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        var rawInput = new Vector3(h, 0, v);
        var inputDir = rawInput.sqrMagnitude > 1f ? rawInput.normalized : rawInput;
        
        var isMoving = inputDir.sqrMagnitude > GlobalValues.IgnoreDistance;
        var moveDir = Vector3.zero;
        var lookRot = transform.rotation;
        
        if (isMoving)
        {
            var camForward = Cam.transform.forward;
            var camRight = Cam.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            
            moveDir = (camForward * inputDir.z + camRight * inputDir.x).normalized;
            lookRot = Quaternion.LookRotation(moveDir);
        }
        
        return new PlayerInputCommand
        {
            IsActive = isMoving,
            Priority = priority,
            State = ActorState.Move,
            MoveDirection = moveDir,
            LookRotation = lookRot
        };
    }
    
    PlayerInputCommand EvaluateAttackCommand(int priority)
    {
        attackState.Tick(IsAttackPressed(), Time.deltaTime);
        
        return new PlayerInputCommand
        {
            IsActive = attackState.IsAttacking,
            Priority = priority,
            State = ActorState.Attack,
            MoveDirection = Vector3.zero,
            LookRotation = GetAttackLookRotation()
        };
    }

    Quaternion GetAttackLookRotation()
    {
        if (!attackState.IsAttacking)
            return transform.rotation;

        var attackTarget = GetAttackTarget();
        if (attackTarget == null)
            return transform.rotation;

        var direction = attackTarget.transform.position - transform.position;
        direction.y = 0f;

        return QuaternionExtensions.SafeLookRotation(direction, transform.rotation);
    }

    Actor GetAttackTarget()
    {
        if (target != null && !target.IsDead)
            return target;

        target = scanner.GetTarget();
        return target;
    }

    PlayerInputCommand EvaluateJumpCommand(int priority)
    {
        var isJumping = !IsAttacking && Input.GetKeyDown(KeyCode.Space);

        return new PlayerInputCommand
        {
            IsActive = isJumping,
            Priority = priority,
            State = ActorState.Jump,
            MoveDirection = Vector3.zero,
            LookRotation = transform.rotation
        };
    }
    
    bool IsAttackPressed()
    {
        return Input.GetMouseButton(0);
    }
}
