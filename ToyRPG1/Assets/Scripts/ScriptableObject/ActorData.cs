using UnityEngine;
using UnityEngine.Serialization;

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
}

[CreateAssetMenu(fileName = "New Player Data", menuName = "ScriptableObject Object/Player Data")]
public class PlayerData : ActorData
{
    
}

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "ScriptableObject Object/Enemy Data")]
public class EnemyData : ActorData
{
    
}