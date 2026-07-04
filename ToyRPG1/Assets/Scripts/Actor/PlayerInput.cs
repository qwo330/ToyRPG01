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
        static readonly AttackTiming[] DefaultTimings =
        {
            new(0.25f, 0.42f, 0.5f, 0.7f),
            new(0.25f, 0.42f, 0.55f, 0.75f),
            new(0f, 0f, 0.65f, 0.85f)
        };

        AttackTiming[] timings;
        float elapsedTime;
        float recoveryTime;

        public int Combo { get; private set; }
        public bool IsAttacking => Combo > 0;
        
        AttackTiming[] Timings => timings != null && timings.Length > 0 ? timings : DefaultTimings;
        int MaxCombo => Timings.Length;
        bool HasNextCombo => Combo < MaxCombo;

        public void Init(AttackTiming[] attackTimings)
        {
            timings = attackTimings;
            Reset();
        }

        public void Tick(bool attackPressed, float deltaTime)
        {
            recoveryTime -= deltaTime;
            if (recoveryTime > 0f)
                return;

            if (!IsAttacking)
            {
                if (attackPressed)
                    Begin(1);

                return;
            }

            elapsedTime += deltaTime;

            var timing = GetTiming(Combo);

            if (attackPressed && HasNextCombo && timing.CanBufferInput(elapsedTime))
            {
                Begin(Combo + 1);
                return;
            }

            if (elapsedTime < timing.AttackEnd)
                return;

            Reset(timing.RecoveryDuration);
        }

        AttackTiming GetTiming(int combo)
        {
            var index = Mathf.Clamp(combo - 1, 0, Timings.Length - 1);
            return Timings[index];
        }

        void Begin(int combo)
        {
            Combo = combo;
            elapsedTime = 0f;
            recoveryTime = 0f;
        }

        void Reset(float recoveryDuration = 0f)
        {
            Combo = 0;
            elapsedTime = 0f;
            recoveryTime = recoveryDuration;
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
