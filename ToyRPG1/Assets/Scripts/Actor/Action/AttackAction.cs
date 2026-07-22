using UnityEngine;

public interface IAttack : IAction
{
    public string ParticlePath { get; }

    public float AttackCoolTime { get; }
    public float AttackRange { get; }
}

public class AttackAction : MonoBehaviour, IAttack
{
    public string ParticlePath => "FX/ArcaneProjectileSmall.prefab";
    
    public ActorSnapshot Snapshot { get; private set; }
    public float AttackCoolTime { get; private set; }
    public float AttackRange { get; private set; }

    public void Init(ActorData data)
    {
        AttackCoolTime = data.AttackCooltime;
        AttackRange = data.AttackRange;
    }

    public void Apply(ActorSnapshot snapshot)
    {
        Snapshot = snapshot;
    }

    public void Process()
    {
    }
}
