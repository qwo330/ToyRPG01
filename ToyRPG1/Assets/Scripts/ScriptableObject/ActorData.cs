using System;
using UnityEngine;

public class ActorData : ScriptableObject
{
    public int MaxHP;
    
    public float WalkSpeed;
    public float RotSpeed;
    public float Acceleration;
    
    public float JumpPower;
    public float Gravity;
    
    public float AttackCooltime;
    public float AttackRange;

    public AttackTiming[] AttackTimings;
}

[CreateAssetMenu(fileName = "New Player Data", menuName = "ScriptableObject Object/Player Data")]
public class PlayerData : ActorData
{
    
}

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ScriptableObject Object/Enemy Data")]
public class EnemyData : ActorData
{
    public float RoamRadius = 5f;
    public float DetectRadius = 10f;
    public float LeashRange = 15f;
    
    public float StateChangeTime;
    public float NextRoamingTime;
}

[Serializable]
public struct AttackTiming
{
    public float InputStart;
    public float InputEnd;
    public float AttackEnd;
    public float RecoveryEnd;

    public float RecoveryDuration => Mathf.Max(0f, RecoveryEnd - AttackEnd);

    public AttackTiming(float inputStart, float inputEnd, float attackEnd, float recoveryEnd)
    {
        InputStart = inputStart;
        InputEnd = inputEnd;
        AttackEnd = attackEnd;
        RecoveryEnd = recoveryEnd;
    }

    public bool CanBufferInput(float elapsedTime)
    {
        return elapsedTime >= InputStart && elapsedTime <= InputEnd;
    }
}
