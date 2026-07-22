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
