using UnityEngine;

public interface IAttack : IAction
{
    public int AttackPower { get; }
    public float AttackCoolTime { get; }
    public float AttackRange { get; }

    void OnNormalAttackEvent();
}

public abstract class AttackAction : MonoBehaviour, IAttack
{
    public ActorSnapshot Snapshot { get; private set; }
    public int AttackPower => Actor.GetAttackPower();
    public float AttackCoolTime => Actor.GetAttackCooldown();
    public float AttackRange => Actor.GetAttackRange();

    protected Actor Actor { get; private set; }

    int processedCombo;

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
        if (!CanProcess() || !IsAttackSnapshot(Snapshot))
        {
            ResetAttack();
            return;
        }

        if (processedCombo == Snapshot.Combo)
            return;

        processedCombo = Snapshot.Combo;
        ExecuteAttack(Snapshot);
    }

    public abstract void OnNormalAttackEvent();

    protected abstract void ExecuteAttack(ActorSnapshot snapshot);

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

    protected virtual bool IsAttackSnapshot(ActorSnapshot snapshot)
    {
        return snapshot.State == ActorState.Attack && snapshot.Combo > 0;
    }

    protected virtual void ResetAttack()
    {
        processedCombo = 0;
    }
}
