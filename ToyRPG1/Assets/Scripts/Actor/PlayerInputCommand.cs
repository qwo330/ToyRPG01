using UnityEngine;

public struct PlayerInputCommand
{
    public bool IsActive;
    public int Priority;
    public ActorState State;
    public Vector3 MoveDirection;
    public Quaternion LookRotation;
}
