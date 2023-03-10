using System.Collections.Generic;
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

public class PlayerControl : SingletonMono<PlayerControl>
{
    public float Speed = 6.0f;
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

    [SerializeField]
    EPlayerState currentState;

    bool isAttack = false;
    float combo = 0;
    int currentHash = 0, prevHash = 0;
    
    EPlayerState playState = EPlayerState.Idle;
    Vector3 motion = Vector3.zero;
    
    static readonly int HASH_JUMP = Animator.StringToHash("Jump");
    static readonly int HASH_RUN = Animator.StringToHash("Run");
    static readonly int HASH_COMBO = Animator.StringToHash("Combo");

    void Start()
    {
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
    }

    void Move()
    {
        if (IsActionAble() == false)
        {
            return;
        }

        EPlayerState animState = EPlayerState.Idle;
        float animatorValue = 0;

        if (controller.isGrounded == false)
        {
            animState = EPlayerState.Idle;
        }
        else
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 input = new Vector3(horizontalInput, 0, verticalInput);
            if (input != Vector3.zero)
            {
                // move
                Vector3 forward = playerCamera.transform.TransformDirection(input);
                forward.y = 0;
                forward = forward.normalized;

                motion = forward * Speed;

                animState = EPlayerState.Run;
                animatorValue = motion.sqrMagnitude;

                // rotate
                Quaternion rotation = Quaternion.LookRotation(forward);
                Trans.rotation = rotation;
            }
            else // 조작 없을 때
            {
                motion = Vector3.zero;
                animState = EPlayerState.Idle;
            }

            if (Input.GetButton("Jump"))
            {
                motion.y = JumpSpeed;
                animState = EPlayerState.Jump;
            }
        }

        // always
        motion.y -= Gravity * Time.deltaTime;
        controller.Move(motion * Time.deltaTime);
        SetPlayerAnimation(animState, animatorValue);
    }
    
    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (currentState == EPlayerState.Run)
            {
                ChangeStateToIdle();
            }
            
            if (playerAnim.GetFloat(HASH_COMBO) == 0)
            {
                SetPlayerAnimation(EPlayerState.Attack, 1);
            }

            //_lastClickTime = Time.time;
            //_numClicks++;


            //SetPlayerAnimation(EPlayerState.Attack, attackIndex);
            //attackIndex++;

            //if (_numClicks == 1)
            //{
            //    SetPlayerAnimation(EPlayerState.Attack, 1);
            //    _numClicks = 0;

            //    attackIndex = 0;
            //}

            //if (curCombo > 1 && curCombo >= _numClicks)
            //{
            //    SetPlayerAnimation(EPlayerState.Attack, curCombo);
            //    curCombo = curCombo + 1;
            //    _numClicks = curCombo + 1;
            //}
        }
    }
    
    void FinishAttack()
    {
        currentHash = 0;
        prevHash = 0;

        isAttack = false;
        combo = 0;

        SetPlayerAnimation(EPlayerState.Idle);

        Debug.LogError("FINISH !!");
    }
    
    void ChangeState(EPlayerState nextState)
    {
        // todo : 이걸로 상태 변경하고 프로세스를 통해 SetPlayerAnimation 호출하는게 좋을 듯
        currentState = nextState;
    }

    void ChangeStateToIdle()
    {
        // todo : 이동 중 공격을 하면 애니메이터가 꼬인다. (애니메이터는 run, 코드는 attack)
        // ChangeState(EPlayerState.Idle);
        SetPlayerAnimation(EPlayerState.Idle);
    }

    public void SetPlayerAnimation(EPlayerState state, float _animatorValue = 0)
    {
        if (currentState != state)
        {
            EPlayerState saveState = currentState;
            // Debug.LogError($"Change State : {saveState} -> {state}");
        }

        // 뛰면서 바로 공격하면 멈춤, state가 attack이 되어 이동은 먹통, 애니메이션은 전환이 안됨
        // 코드의 state는 바로 바뀌고 animation은 바뀌지 않아 버그 발생함
        currentState = state;
        
        switch (state)
        {
            case EPlayerState.Run:
                playerAnim.SetFloat(HASH_RUN, _animatorValue);
                break;
            case EPlayerState.Jump:
                playerAnim.SetTrigger(HASH_JUMP);
                break;
            case EPlayerState.Attack:
                //_playerAnim.SetFloat("Combo", _animatorValue);
                //_playerAnim.SetTrigger("Attack");
                playerAnim.SetFloat(HASH_COMBO, _animatorValue);

                if (_animatorValue == 0)
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