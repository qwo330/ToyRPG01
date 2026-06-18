using System;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    struct Command
    {
        public bool IsActive;
        public int Priority;
        public ActorState State;
        public Vector3 MoveDirection;
        public Quaternion LookRotation;
    }
    
    Player player;
    Camera Cam => Camera.main;

    [SerializeField] float comboInputSec = 0.8f;
    
    int combo = 0;
    float lastAttackTime;
    bool isAttacking;
    bool isJumping;
    
    void Start()
    {
        if (!gameObject.TryGetComponent<Player>(out var p))
        {
            MyDebug.LogError("Cannot find player in scene!");
        }

        player = p;
        ActorManager.Instance.LocalPlayerID = p.EntityID;
    }

    void Update()
    {
        Span<Command> commands = stackalloc Command[]
        {
            EvaluateAttackCommand(priority: 4),
            // EvaluateJumpCommand(priority: 3),
            EvaluateMoveCommand(priority: 2)
        };

        var finalCommand = new Command
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
            Combo = combo
        };
        
        ActorManager.Instance.AddSnapshot(s);
    }

    #region Commands
    
    Command EvaluateMoveCommand(int priority)
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
        
        return new Command
        {
            IsActive = isMoving,
            Priority = priority,
            State = ActorState.Move,
            MoveDirection = moveDir,
            LookRotation = lookRot
        };
    }
    
    Command EvaluateAttackCommand(int priority)
    {
        if (isAttacking && IsComboCanceled())
        {
            ResetAttackState();
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (IsComboCanceled())
            {
                combo = 0;
            }

            combo++;

            if (combo <= 3)
            {
                isAttacking = true;
                lastAttackTime = Time.time;
            }
        }
        
        return new Command
        {
            IsActive = isAttacking,
            Priority = priority,
            State = ActorState.Attack,
            MoveDirection = Vector3.zero,
            LookRotation = transform.rotation
        };
    }

    bool IsComboCanceled()
    {
        return Time.time - lastAttackTime > comboInputSec;
    }

    public void ResetAttackState()
    {
        isAttacking = false;
        combo = 0;
    }
    
    Command EvaluateJumpCommand(int priority)
    {
        if (!isAttacking && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
        }

        return new Command
        {
            IsActive = isJumping,
            Priority = priority,
            State = ActorState.Jump,
            MoveDirection = Vector3.zero,
            LookRotation = transform.rotation
        };
    }
    
    #endregion Commands
}