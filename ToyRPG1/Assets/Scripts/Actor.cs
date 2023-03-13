using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public int MaxHP;
    public int HP;
    
    public abstract void Move();
    public abstract void Attack();

    public void TakeDamage(int power)
    {
        HP -= power;
        
        if (HP <= 0)
        {
            Dead();
        }
    }

    public void Dead()
    {
        MyDebug.LogError($"{gameObject.name} 죽음");
    }
}

public enum EActorState
{
    Idle = 0,
    Run,
    Jump,
    Attack,
    Hit,
    Dead,
    
    // player 외의 행동
    Spawn = 100,
    StandBy, // 대기 모션
    Roam,
}