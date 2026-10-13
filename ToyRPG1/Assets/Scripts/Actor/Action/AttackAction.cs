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
    const int DefaultAttackPower = 1;
    
    [SerializeField] int defaultAttackPower = DefaultAttackPower;

    public ActorSnapshot Snapshot { get; private set; }
    public int AttackPower => Data != null && Data.AttackPower > 0 ? Data.AttackPower : defaultAttackPower;
    public float AttackCoolTime => Data != null ? Data.AttackCooltime : 0f;
    public float AttackRange => Data != null ? Data.AttackRange : 0f;

    protected Actor Actor
    {
        get
        {
            if (actor == null)
                actor = GetComponentInParent<Actor>();

            return actor;
        }
    }

    protected ActorData Data { get; private set; }

    Actor actor;
    int processedCombo;

    public virtual void Init(ActorData data)
    {
        Data = data;
    }

    public virtual void Apply(ActorSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public virtual void Process()
    {
        if (!IsAttackSnapshot(Snapshot))
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
