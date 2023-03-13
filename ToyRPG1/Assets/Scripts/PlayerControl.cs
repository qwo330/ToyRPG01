using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum EPlayerState
{
    Idle = 0,
    Run,
    Jump,
    Attack,
    Hit,
    Dead,
}

public class PlayerControl : Actor
{
    [SerializeField] Transform attackPoint;
    [SerializeField] Projectile ProjectilePrefab;
    
    readonly static string NORMAL_ATTACK_PATH = "FX/ArcaneProjectileSmall.prefab";
    
    MyObjectPool<Projectile> normalAttackPool;

    static PlayerControl player = null;
    public static PlayerControl Player => player;

    public float WalkSpeed = 6.0f;
    public float RotSpeed = 5.0f;
    
    public float JumpSpeed = 8.0f;
    public float Gravity = 10.0f;
    
    public float AttackCoolTime = 2.0f; // 공격 애니메이션 시간
    public float AttackInputTime = 1.0f; // 2타 입력 대기 시간

    /*
     * 1타 공격 애니메이션 25프레임 x 1.0배속
     * 0.01667 * 25 = 0.41675 -> 0.4
     * 
     * 2타 공격 25프레임 x 0.6배속
     * 0.01667 * 25 / 0.6 = 0.69458 -> 0.7
     * */

    [HideInInspector] public Transform Trans;
    public GameObject FollowTarget;

    Animator playerAnim;
    CharacterController controller;
    Camera playerCamera;

    [SerializeField] EPlayerState prevState;
    [SerializeField] EPlayerState currentState;
    float currentStateValue = 0;

    bool isJumping = false;
    int currentHash = 0, prevHash = 0;
    
    EPlayerState playState = EPlayerState.Idle;
    Vector3 motion = Vector3.zero;
    
    static readonly int HASH_JUMP = Animator.StringToHash("Jump");
    static readonly int HASH_RUN = Animator.StringToHash("Run");
    static readonly int HASH_COMBO = Animator.StringToHash("Combo");

    void Start()
    {
        player = this;
        normalAttackPool = new MyObjectPool<Projectile>(NORMAL_ATTACK_PATH);

        Trans = GetComponent<Transform>();
        controller = GetComponent<CharacterController>();
        playerAnim = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;
        
        motion = Vector3.zero;
        currentState = EPlayerState.Idle;
    }

    void Update()
    {
        //if (Time.time - _lastClickTime > _comboDelay)
        //{
        //    _numClicks = 0;
        //    attackIndex = 0;
        //}

        // todo : 초기에 playState를 Idle로 세팅하고, attack, move 등에서 playState를 교체한다.
        // update의 마지막에 playState, animValue를 매개변수로 SetPlayerAnimation를 호출해서 이번 업데이트의 동작을 결정한다?
        
        Attack();
        Move();
        SetPlayerAnimation(currentState, currentStateValue);
    }

    public override void Move()
    {
        if (isJumping && controller.isGrounded)
        {
            ChangeState(EPlayerState.Jump, 0);
            isJumping = false;
        }
        
        if (IsActionAble())
        {
            if (controller.isGrounded)
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                Vector3 input = new Vector3(horizontalInput, 0, verticalInput);
                if (input == Vector3.zero) // 조작 없을 때
                {
                    ChangeStateToIdle();
                }
                else 
                {
                    Walk(input);
                }

                if (Input.GetButton("Jump"))
                {
                    Jump();
                }
            }
        }

        // always
        UseGravity();
        
        controller.Move(motion * Time.deltaTime);
    }

    void Walk(Vector3 input)
    {
        // move
        Vector3 forward = playerCamera.transform.TransformDirection(input);
        forward.y = 0;
        forward = forward.normalized;

        // rotate
        Quaternion rotation = Quaternion.LookRotation(forward);
        Trans.rotation = rotation;
        
        motion = forward * WalkSpeed;
        ChangeState(EPlayerState.Run, motion.sqrMagnitude);
    }

    void Jump()
    {
        if (controller.isGrounded && Input.GetButton("Jump"))
        {
            isJumping = true;
            motion.y = JumpSpeed;
            ChangeState(EPlayerState.Jump, JumpSpeed);
        }
    }
    
    void UseGravity()
    {
        if (controller.isGrounded)
        {
            return;
        }
        
        motion.y -= Gravity * Time.deltaTime;
    }

    public override void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentState == EPlayerState.Run)
            {
                ChangeStateToIdle();
            }
            
            // start attack
            if (playerAnim.GetFloat(HASH_COMBO) == 0)
            {
                ChangeState(EPlayerState.Attack, 1);
            }
        }
    }

    public void PlayAttack()
    {
        var missle = normalAttackPool.Get();
        missle.transform.position = attackPoint.transform.position;
        missle.Direction = transform.forward;
        missle.Power = 1; // 데미지 값 처리 미정
    }
    
    void FinishAttack()
    {
        currentHash = 0;
        prevHash = 0;

        ChangeState(EPlayerState.Idle);
    }
    
    public void ChangeState(EPlayerState nextState, float nextValue = 0)
    {
        // todo : 이걸로 상태 변경하고 프로세스를 통해 SetPlayerAnimation 호출하는게 좋을 듯
        MyDebug.Log($"Change State : {currentState} -> {nextState}");
        
        currentState = nextState;
        currentStateValue = nextValue;
    }
    
    void ChangeStateToIdle()
    {
        // todo : 이동 중 공격을 하면 애니메이터가 꼬인다. (애니메이터는 run, 코드는 attack)
        motion = Vector3.zero;
        ChangeState(EPlayerState.Idle);
    }
    
    public void SetPlayerAnimation(EPlayerState newState, float animatorValue = 0)
    {
        if (prevState != newState)
        {
            MyDebug.LogError($"Change State : {prevState} -> {newState}");
            ChangeAnimationParameter(prevState, 0); // 이전 상태의 파라미터를 0으로 초기화
            prevState = newState;
        }

        ChangeAnimationParameter(newState, animatorValue);
    }

    void ChangeAnimationParameter(EPlayerState newState, float animatorValue = 0)
    {
        switch (newState)
        {
            case EPlayerState.Run:
                playerAnim.SetFloat(HASH_RUN, animatorValue);
                break;
            
            case EPlayerState.Jump:
                playerAnim.SetBool(HASH_JUMP, animatorValue > 0);
                break;
            
            case EPlayerState.Attack:
                playerAnim.SetFloat(HASH_COMBO, animatorValue);

                if (animatorValue == 0)
                {
                    FinishAttack();
                }
                break;
            
            case EPlayerState.Hit:
                break;
            
            case EPlayerState.Dead:
                break;
            
            default:
            case EPlayerState.Idle:
                playerAnim.SetFloat(HASH_RUN, 0);
                break;
        }
    }
    
    bool IsActionAble()
    {
        return currentState != EPlayerState.Dead
            && currentState != EPlayerState.Hit
            && currentState != EPlayerState.Attack;
    }
}