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
    
    struct ComboAttackState
    {
        AttackTiming[] timings;
        float elapsedTime;
        bool bufferedNextCombo;

        public int Combo { get; private set; }
        public bool IsAttacking => Combo > 0;
        
        int MaxCombo => timings.Length;
        bool HasNextCombo => Combo < MaxCombo;

        public void Init(AttackTiming[] attackTimings)
        {
            timings = attackTimings;
            Reset();
        }

        public void Tick(bool attackPressed, float deltaTime)
        {
            if (!IsAttacking)
            {
                if (attackPressed)
                    Begin(1);

                return;
            }

            elapsedTime += deltaTime;

            var timing = GetTiming(Combo);

            if (attackPressed && HasNextCombo && timing.CanBufferInput(elapsedTime))
                bufferedNextCombo = true;

            if (elapsedTime < timing.AttackEnd)
                return;

            if (bufferedNextCombo && HasNextCombo)
                Begin(Combo + 1);
            else
                Reset();
        }

        AttackTiming GetTiming(int combo)
        {
            var index = Mathf.Clamp(combo - 1, 0, timings.Length - 1);
            return timings[index];
        }

        void Begin(int combo)
        {
            Combo = combo;
            elapsedTime = 0f;
            bufferedNextCombo = false;
        }

        void Reset()
        {
            Combo = 0;
            elapsedTime = 0f;
            bufferedNextCombo = false;
        }
    }

    ComboAttackState attackState;
    Player player;

    Camera Cam => Camera.main;
    bool IsAttacking => attackState.IsAttacking;
    
    void Start()
    {
        if (gameObject.TryGetComponent<Player>(out var p))
        {
            player = p;
            ActorManager.Instance.LocalPlayerID = p.EntityID;
            attackState.Init(player.Data.AttackTimings);
        }
        else
        {
            MyDebug.LogError("Cannot find player in scene!");
            attackState.Init(null);
        }
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
            Combo = attackState.Combo
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
        attackState.Tick(IsAttackPressed(), Time.deltaTime);
        
        return new Command
        {
            IsActive = attackState.IsAttacking,
            Priority = priority,
            State = ActorState.Attack,
            MoveDirection = Vector3.zero,
            LookRotation = transform.rotation
        };
    }

    bool IsAttackPressed()
    {
        return Input.GetMouseButtonDown(0);
    }
    
    Command EvaluateJumpCommand(int priority)
    {
        var isJumping = !IsAttacking && Input.GetKeyDown(KeyCode.Space);

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
