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
    
}

[Serializable]
public struct AttackTiming
{
    public float InputStart;
    public float InputEnd;
    public float AttackEnd;

    public AttackTiming(float inputStart, float inputEnd, float attackEnd)
    {
        InputStart = inputStart;
        InputEnd = inputEnd;
        AttackEnd = attackEnd;
    }

    public bool CanBufferInput(float elapsedTime)
    {
        return elapsedTime >= InputStart && elapsedTime <= InputEnd;
    }
}