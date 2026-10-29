using UnityEngine;

public enum SkillTargetType : byte
{
    None,
    Self,
    Actor,
    Position,
    Direction
}

public struct SkillTarget
{
    public SkillTargetType Type;
    public int ActorID;
    public Vector3 Position;
    public Vector3 Direction;
}

public struct SkillCommand
{
    public int SourceID;
    public uint Sequence;
    public int SkillID;
    public int ComboIndex;
    public SkillTarget Target;
}

public interface ISkillCommandHandler
{
    public void HandleSkillCommand(SkillCommand command);
}
