using System.Collections.Generic;
using UnityEngine;

public interface IAttack : IAction
{
    public int AttackPower { get; }
    public float AttackCoolTime { get; }
    public float AttackRange { get; }

    void OnNormalAttackEvent();
}

public abstract class AttackAction : MonoBehaviour, IAttack, ISkillCommandHandler
{
    public ActorSnapshot Snapshot { get; private set; }
    public int AttackPower => Actor.GetAttackPower();
    public float AttackCoolTime => Actor.GetAttackCooldown();
    public float AttackRange => Actor.GetAttackRange();

    protected Actor Actor { get; private set; }

    readonly Queue<SkillCommand> pendingCommands = new();
    uint lastHandledSequence;

    void Awake()
    {
        if (TryGetComponent(out Actor actor))
        {
            Actor = actor;
            return;
        }

        MyDebug.LogError($"Cannot find Actor component on {gameObject.name}");
        enabled = false;
    }

    protected virtual void OnDisable()
    {
        pendingCommands.Clear();
        ResetAttack();
    }

    public virtual void Init(ActorData data)
    {
    }

    public virtual void Apply(ActorSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public virtual bool CanProcess()
    {
        return isActiveAndEnabled && Actor.CanAct;
    }

    public virtual void Process()
    {
        if (!CanProcess())
            return;

        while (pendingCommands.Count > 0)
            ExecuteAttack(pendingCommands.Dequeue());
    }

    public void HandleSkillCommand(SkillCommand command)
    {
        if (command.Sequence <= lastHandledSequence)
            return;

        lastHandledSequence = command.Sequence;
        pendingCommands.Enqueue(command);
    }

    public abstract void OnNormalAttackEvent();

    protected abstract void ExecuteAttack(SkillCommand command);

    protected virtual void ResetAttack()
    {
    }

    protected virtual Vector3 GetFireDirection()
    {
        var direction = transform.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude <= GlobalValues.IgnoreDistance)
            return Vector3.forward;

        return direction.normalized;
    }

    protected virtual Quaternion GetProjectileRotation(Vector3 direction)
    {
        return Quaternion.FromToRotation(Vector3.up, direction);
    }
}
