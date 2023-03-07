using System.Collections.Generic;
using UnityEngine;

public enum EPlayerState
{
    Idle = 0,
    Run,
    Jump,
    Attack,
    Hit,
    Die,
}

public class PlayerControl : SingletonMono<PlayerControl>
{
    public float _Speed = 6.0f;
    public float _JumpSpeed = 8.0f;
    public float _Gravity = 10.0f;
    public float _RotSpeed = 5.0f;
    public float _AttackCoolTime = 2.0f; // 공격 애니메이션 시간
    public float _AttackInputTime = 1.0f; // 2타 입력 대기 시간

    public float testStartAnim = 0.6f;
    public float testEndAnim = 1f;
    
    /*
     * 1타 공격 애니메이션 25프레임 x 1.0배속
     * 0.01667 * 25 = 0.41675 -> 0.4
     * 
     * 2타 공격 25프레임 x 0.6배속
     * 0.01667 * 25 / 0.6 = 0.69458 -> 0.7
     * */

    [HideInInspector]
    public Transform _Trans;
    public GameObject _FollowTarget;

    Animator _playerAnim;
    CharacterController _controller;
    Camera playerCamera;

    Vector3 _motion;
    
    [SerializeField]
    EPlayerState _currentState;

    bool isAttack = false;
    float combo = 0;
    int currentHash = 0, prevHash = 0;
    
    static readonly int HASH_JUMP = Animator.StringToHash("Jump");
    static readonly int HASH_RUN = Animator.StringToHash("Run");
    static readonly int HASH_COMBO = Animator.StringToHash("Combo");

    void Start()
    {
        _Trans = GetComponent<Transform>();
        _controller = GetComponent<CharacterController>();
        _playerAnim = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        _motion = Vector3.zero;
        _currentState = EPlayerState.Idle;
    }

    EPlayerState playState = EPlayerState.Idle;
    float animValue = 0;

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

        //Attack();
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

        if (_controller.isGrounded == false)
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

                _motion = forward * _Speed;

                animState = EPlayerState.Run;
                animatorValue = _motion.sqrMagnitude;

                // rotate
                Quaternion rotation = Quaternion.LookRotation(forward);
                _Trans.rotation = rotation;
            }
            else // 조작 없을 때
            {
                _motion = Vector3.zero;
                animState = EPlayerState.Idle;
            }

            if (Input.GetButton("Jump"))
            {
                _motion.y = _JumpSpeed;
                animState = EPlayerState.Jump;
            }
        }

        // always
        _motion.y -= _Gravity * Time.deltaTime;
        _controller.Move(_motion * Time.deltaTime);
        SetPlayerAnimation(animState, animatorValue);

        Debug.LogError($"animState : {animState}");
    }

    int _numClicks = 0;
    int curCombo = 0;
    int attackIndex = 0;
    float _lastClickTime = 0;
    float _comboDelay = 1.2f;

    void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_currentState == EPlayerState.Run)
            {
                SetStatMoveToIdle();
            }
            
            if (_playerAnim.GetFloat("Combo") == 0)
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

    void Attack_old()
    {
        /*
         * 콤보어택 애니메이션 참고 링크
         * https://answers.unity.com/questions/1192912/hot-to-make-combo-attack-1-button-do-3-things.html
         * https://samirgeorgy.wordpress.com/2021/07/22/lets-create-a-simple-melee-combo-system/
         */

        bool attackFlag = false;

        if (Input.GetMouseButton(0))
        {
            // 최초 공격
            if (combo == 0)
            {
                Debug.LogError("attack !!");

                attackFlag = true;

                combo = 1;
                isAttack = true;

                SetPlayerAnimation(EPlayerState.Attack, combo);

                var stateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);
                currentHash = stateInfo.shortNameHash;
                //Debug.LogError(phase + " time? " + stateInfo.length);
            }
            else // 연속으로 공격 입력이 올 때
            {
                // 다음 공격 입력 타이밍이고, 다음 모션이 있을 때
                if (IsInputTime() && IsSameAnimation() == false)
                {

                    attackFlag = true;

                    prevHash = currentHash;

                    combo++;
                    SetPlayerAnimation(EPlayerState.Attack, combo);

                    var stateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);
                    currentHash = stateInfo.shortNameHash;
                    
                    Debug.LogError(combo + " time? " + stateInfo.length);
                }
            }
        }

        if (isAttack)
        {
            var animStateInfo = _playerAnim.GetCurrentAnimatorStateInfo(1);
            Debug.LogError($"=== anim hash : {animStateInfo.shortNameHash}, time : {animStateInfo.normalizedTime}");
            //ShowCurrentAnimName();
        }

        // 공격이 종료될 때까지 다음 입력이 없을 때
        // 애니메이션이 바로 끝나버려서(왠지 모르겠음)
        // flag가 true이면 현재 루프에선 호출 하지 않는다.
        if (isAttack && IsFinishTime() && attackFlag == false)
        {
            currentHash = 0;
            prevHash = 0;

            isAttack = false;
            combo = 0;

            SetPlayerAnimation(EPlayerState.Attack, 0);
            SetPlayerAnimation(EPlayerState.Idle);

            Debug.LogError("FINISH !!");
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

    bool IsInputTime()
    {
        var animStateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);

        if (testStartAnim <= animStateInfo.normalizedTime && animStateInfo.normalizedTime < testEndAnim)
        {
            return true;
        }

        return false;
    }

    bool IsFinishTime()
    {
        var animStateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);
        if (animStateInfo.normalizedTime >= 1.0f)
        {
            return true;
        }

        return false;
    }

    bool IsSameAnimation()
    {
        return prevHash == currentHash;
    }

    void ChangeState(EPlayerState nextState)
    {
        // todo : 이걸로 상태 변경하고 프로세스를 통해 SetPlayerAnimation 호출하는게 좋을 듯
        _currentState = nextState;
    }

    void SetStatMoveToIdle()
    {
        // todo : 이동 중 공격을 하면 애니메이터가 꼬인다. (애니메이터는 run, 코드는 attack)
        // ChangeState(EPlayerState.Idle);
        SetPlayerAnimation(EPlayerState.Idle);
    }

    public void SetPlayerAnimation(EPlayerState state, float _animatorValue = 0)
    {
        if (_currentState != state)
        {
            var saveState = _currentState;
            Debug.LogError($"Change State : {saveState} -> {state}");
        }

        // 뛰면서 바로 공격하면 멈춤, state가 attack이 되어 이동은 먹통, 애니메이션은 전환이 안됨
        // 코드의 state는 바로 바뀌고 animation은 바뀌지 않아 버그 발생함
        _currentState = state;
        
        switch (state)
        {
            case EPlayerState.Run:
                _playerAnim.SetFloat(HASH_RUN, _animatorValue);
                break;
            case EPlayerState.Jump:
                _playerAnim.SetTrigger(HASH_JUMP);
                break;
            case EPlayerState.Attack:
                //_playerAnim.SetFloat("Combo", _animatorValue);
                //_playerAnim.SetTrigger("Attack");
                _playerAnim.SetFloat(HASH_COMBO, _animatorValue);

                if (_animatorValue == 0)
                {
                    FinishAttack();
                }
                break;
            case EPlayerState.Hit:
                break;
            case EPlayerState.Die:
                break;
            default:
            case EPlayerState.Idle:
                _playerAnim.SetFloat(HASH_RUN, 0);
                break;
        }
    }

    bool IsActionAble()
    {
        return _currentState != EPlayerState.Die
            && _currentState != EPlayerState.Hit
            && _currentState != EPlayerState.Attack;
    }
}