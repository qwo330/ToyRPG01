using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    Camera Cam => Camera.main;
    bool IsAttacking => attackState.IsAttacking;

    ComboAttackState attackState;
    Player player;

    void Start()
    {
        if (gameObject.TryGetComponent<Player>(out var p))
        {
            player = p;
            ActorManager.Instance.LocalPlayerID = p.EntityID;
            attackState.Init(player.Data != null ? player.Data.AttackTimings : null);
        }
        else
        {
            MyDebug.LogError("Cannot find player in scene!");
            attackState.Init(null);
        }
    }

    void Update()
    {
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
        
        ActorSnapshot s = new()
        {
            EntityID = player.EntityID,
            DataIndex = nextIndex,
            Position = finalCommand.MoveDirection,
            Rotation = finalCommand.LookRotation,
            State = finalCommand.State,
            Combo = attackState.Combo
        };
        
        ActorManager.Instance.AddSnapshot(s);
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
            LookRotation = transform.rotation
        };
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
        return Input.GetMouseButtonDown(0);
    }
}
