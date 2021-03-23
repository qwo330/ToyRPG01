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
    public float _Speed = 6.0f;
    public float _JumpSpeed = 8.0f;
    public float _Gravity = 10.0f;
    public float _RotSpeed = 5.0f;
    public float _AttackCoolTime = 2.0f; // 공격 애니메이션 시간
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
    EPlayerState _currentState;

    float _nextAttackTime = 0;
    float _nextAttackInputTime = 0;

    void Start()
    {
        _Trans = GetComponent<Transform>();
        _controller = GetComponent<CharacterController>();
        _playerAnim = GetComponentInChildren<Animator>();

        playerCamera = Camera.main;

        _moveDir = Vector3.zero;
        _currentState = EPlayerState.Idle;
        
        _nextAttackTime = 0;
    }

    void Update()
    {
        Attack();
        Move();
    }

    void Move()
    {
        if (_currentState == EPlayerState.Die
            || _currentState == EPlayerState.Hit
            || _currentState == EPlayerState.Attack)
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
                SetPlayerAnimation(EPlayerState.Jump, 1);
            }
        }

        _moveDir.y -= _Gravity * Time.deltaTime;
        _controller.Move(_moveDir * Time.deltaTime);
    }

    int combo = 0;
    void Attack()
    {
        if (Time.time >= _nextAttackInputTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (combo < 2)
                {
                    _nextAttackTime = Time.time + _AttackCoolTime;
                    _nextAttackInputTime = Time.time + _AttackInputTime;

                    combo++;
                    SetPlayerAnimation(EPlayerState.Attack, combo);

                    Debug.Log("Attack " + combo);
                }
                else
                    combo = 0;
            }
            else
            {

            }
        }
        
    }

    void Attack3()
    {
        if (_currentState != EPlayerState.Idle && _currentState != EPlayerState.Attack)
            return;

        if(Time.time >= _nextAttackTime)
        {
            if (combo != 0) // 공격 초기화
            {
                Debug.Log("Combo Clear");
                combo = 0;
                SetPlayerAnimation(EPlayerState.Attack, combo);
                ChangeState(EPlayerState.Idle);

               // var a = _playerAnim.GetCurrentAnimatorClipInfo(0);
               // a[0].
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (combo == 0)
                {
                    _nextAttackTime = Time.time + _AttackCoolTime;
                    _nextAttackInputTime = Time.time + _AttackInputTime;

                    SetPlayerAnimation(EPlayerState.Attack, ++combo);
                    Debug.Log("Attack " + combo);
                }
            }
            
        } // 공격 쿨타임 중일때
        else if (Time.time < _nextAttackInputTime)
        {
            if(Input.GetMouseButtonDown(0))
            {
                _nextAttackTime = Time.time + _AttackCoolTime;
                SetPlayerAnimation(EPlayerState.Attack, ++combo);

                Debug.Log("Attack" + combo);
            }
        }

        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (Time.time >= _nextAttackTime)
        //    {
        //        Debug.Log("Attack");
        //        _nextAttackTime = Time.time + _AttackCoolTime;
        //        //_nextAttackInputTime = Time.time + 
        //        SetPlayerAnimation(EPlayerState.Attack, ++combo);
        //    }
        //    else if (combo == 1 && Time.time < _nextAttackTime)
        //    {
        //        _nextAttackTime = Time.time + _AttackCoolTime;
        //        SetPlayerAnimation(EPlayerState.Attack, ++combo);
        //    }
        //}
    }

    void Attack2()
    {
        if (_currentState != EPlayerState.Idle && _currentState != EPlayerState.Attack)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (_nextAttackInputTime <= Time.time && Time.time < _nextAttackTime) // 2
            {
                Debug.Log("Attacking");
            }
            else if (Time.time < _nextAttackInputTime) // 1
            {
                Debug.Log("Attack 2");
                _nextAttackTime = Time.time + _AttackCoolTime; // 공격했으니 대기시간도 증가
                _nextAttackInputTime = Time.time; // 더 이상 input 받지 않게
                SetPlayerAnimation(EPlayerState.Attack, 2);
            }
            else // Time.time >= _nextAttackTime
            {
                Debug.Log("Attack");
                _nextAttackTime = Time.time + _AttackCoolTime;
                _nextAttackInputTime = Time.time + _AttackInputTime;
                SetPlayerAnimation(EPlayerState.Attack, 1);
            }
        }

        if (Time.time > _nextAttackTime)
        {
            SetPlayerAnimation(EPlayerState.Attack, 0);
            ChangeState(EPlayerState.Idle);
        }
    }

    //float _animatorValue = 0f;

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
                _playerAnim.SetBool("Jump", _animatorValue != 0);
                break;
            case EPlayerState.Attack:
                _playerAnim.SetFloat("Combo", _animatorValue);
                _playerAnim.SetTrigger("Attack");
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
}