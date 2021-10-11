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

public class PlayerControl : MonoBehaviour
{
    public  float _Speed = 6.0f;
    public  float _JumpSpeed = 8.0f;
    public  float _Gravity = 10.0f;
    public  float _RotSpeed = 5.0f;
    public  float _AttackCoolTime = 2.0f; // 공격 애니메이션 시간
    public float _AttackInputTime = 1.0f; // 2타 입력 대기 시간
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

    Vector3 _moveDir;
    [SerializeField]
    EPlayerState _currentState;

    bool isAttack = false;
    float combo = 0;
    int currentHash = 0, prevHash = 0;

    void Start()
    {
        _Trans = GetComponent<Transform>();
        _controller = GetComponent<CharacterController>();
        _playerAnim = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        _moveDir = Vector3.zero;
        _currentState = EPlayerState.Idle;
    }

    void Update()
    {
        //ShowCurrentAnimName();
        Attack();
        Move();
    }

    void Move()
    {
        if (IsActionAble() == false)
        {
            return;
        }

        if (_controller.isGrounded)
        {
            if (_currentState == EPlayerState.Jump)
            {
                SetPlayerAnimation(EPlayerState.Jump, 0);
            }

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            Vector3 inputDir = new Vector3(horizontalInput, 0, verticalInput);
            if (inputDir != Vector3.zero)
            {
                // rotate
                Vector3 rotate = playerCamera.transform.TransformDirection(inputDir);
                rotate.y = 0;

                Quaternion rotation = Quaternion.LookRotation(rotate);
                _Trans.rotation = rotation;

                // move
                _moveDir = rotate * _Speed;
                SetPlayerAnimation(EPlayerState.Run, _moveDir.sqrMagnitude);
            }
            else // 조작 없을 때
            {
                SetPlayerAnimation(EPlayerState.Idle);
            }

            if (Input.GetButton("Jump"))
            {
                _moveDir.y = _JumpSpeed;
                SetPlayerAnimation(EPlayerState.Jump);
            }
        }

        _moveDir.y -= _Gravity * Time.deltaTime;
        _controller.Move(_moveDir * Time.deltaTime);
    }

    void Attack()
    {
        bool attackFlag = false;

        if (Input.GetMouseButtonDown(0))
        {
            // 최초 공격
            if (combo == 0)
            {
                //Debug.LogError("attack !!");

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
                    //Debug.LogError(phase + " time? " + stateInfo.length);
                }
            }
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

            SetPlayerAnimation(EPlayerState.Attack, combo);
            SetPlayerAnimation(EPlayerState.Idle);

            //Debug.LogError("FINISH !!");
        }
    }

    bool IsInputTime()
    {
        var animStateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);

        if (0.8f <= animStateInfo.normalizedTime && animStateInfo.normalizedTime < 1.0f)
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
        _currentState = nextState;
    }

    void SetPlayerAnimation(EPlayerState state, float _animatorValue = 0)
    {
        _currentState = state;
        switch (state)
        {
            case EPlayerState.Run:
                _playerAnim.SetFloat("Run", _animatorValue);
                break;
            case EPlayerState.Jump:
                _playerAnim.SetTrigger("Jump");
                break;
            case EPlayerState.Attack:
                //_playerAnim.SetFloat("Combo", _animatorValue);
                //_playerAnim.SetTrigger("Attack");
                _playerAnim.SetBool("Attack", _animatorValue != 0);
                _playerAnim.SetFloat("Combo", _animatorValue);
                break;
            case EPlayerState.Hit:
                break;
            case EPlayerState.Die:
                break;
            default:
            case EPlayerState.Idle:
                _playerAnim.SetFloat("Run", 0);
                break;
        }
    }

    bool IsActionAble()
    {
        return _currentState != EPlayerState.Die
            && _currentState != EPlayerState.Hit
            && _currentState != EPlayerState.Attack;
    }

    #region TEST METHODES
    void ShowCurrentAnimName()
    {
        var stateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);
        // attack
        if (stateInfo.IsName("Attack Blend Tree"))
        {
            Debug.Log("Attack Blend Tree");
        }
        else if (stateInfo.IsName("M attack 1"))
        {
            Debug.Log("M attack 1");
        }
        else if (stateInfo.IsName("K,P attack 2"))
        {
            Debug.Log("K,P attack 2");
        }

        // hit
        else if (stateInfo.IsName("M hit"))
        {
            Debug.Log("M hit");
        }

        // idle
        else if (stateInfo.IsName("M idle 2"))
        {
            Debug.Log("M idle 2");
        }

        // run
        else if (stateInfo.IsName("K,M,P run"))
        {
            Debug.Log("K,M,P run");
        }

        // jump
        else if (stateInfo.IsName("M defend"))
        {
            Debug.Log("M defend");
        }
    }

    public void CheckAttackAnim()
    {
        var stateInfo = _playerAnim.GetCurrentAnimatorStateInfo(0);
        // attack
        if (stateInfo.IsName("Attack Blend Tree"))
        {
            Debug.Log("Attack Blend Tree");
        }
        else if (stateInfo.IsName("M attack 1"))
        {
            Debug.Log("M attack 1");
        }
        else if (stateInfo.IsName("K,P attack 2"))
        {
            Debug.Log("K,P attack 2");
        }
        else
            Debug.LogError("OTHEr..");
    }

    public void CheckAttackAnim(AnimatorStateInfo stateInfo)
    {
        // attack
        if (stateInfo.IsName("Attack Blend Tree"))
        {
            Debug.Log("Attack Blend Tree");
        }
        else if (stateInfo.IsName("M attack 1"))
        {
            Debug.Log("M attack 1");
        }
        else if (stateInfo.IsName("K,P attack 2"))
        {
            Debug.Log("K,P attack 2");
        }
        else
            Debug.LogError("OTHEr..");
    }

    string GetPhaseAnimName(float phase)
    {
        //return "Attack Blend Tree";
        switch (phase - 1)
        {
            case 1:
                Debug.Log("M attack 1");
                return "M attack 1";
            case 2:
                Debug.Log("K,P attack 2");
                return "K,P attack 2";
            default:
                Debug.Log("null");
                return null;
        }
    }

    #endregion
}