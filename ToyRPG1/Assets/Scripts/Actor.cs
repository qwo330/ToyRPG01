using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public int MaxHP;
    public int HP;
    
    public abstract void Move();
    public abstract void Attack();
    public abstract void Dead();

    public virtual void TakeDamage(Actor enemy, int power)
    {
        HP -= power;
        
        if (HP <= 0)
        {
            Dead();
        }
    }
}

public enum EActorState
{
    None = 0,
    Idle,
    Run,
    Jump,
    Attack,
    Hit,
    Dead,
    
    // player 외의 행동
    StandBy = 100, // 대기 모션
    Roam,
}