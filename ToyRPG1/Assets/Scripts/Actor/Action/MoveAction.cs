using UnityEngine;

public interface IMove : IAction
{
    public float WalkSpeed { get; }
    public float RotSpeed { get; }
    public float Acceleration { get; }
}

/// <summary>
/// actor의 값 변화를 다루는 자료형. 입력 방식에 관계없이 동일한 처리를 제공한다.
/// </summary>
public struct ActorSnapshot
{
    public int EntityID;
    public uint DataIndex;
    
    public Vector3 Position;
    public Quaternion Rotation;
    public ActorState State;
    public int Combo;
    
    public int TargetID;
    public int SkillID;
}

// public static class ActionExtensions
// {
//     public static bool IsMoving(this ActorSnapshot snapshot)
//     {
//         var dir = snapshot.MoveDirection;
//
//         if (dir.y != 0)
//             return false;
//         
//         return dir.x != 0 || dir.z != 0;
//     }
// }