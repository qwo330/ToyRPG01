using UnityEngine;

public class ProjectileAttackAction : AttackAction
{
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] Transform attackPoint;

    MyObjectPool<Projectile> projectilePool;
    int pendingAttackCount;

    protected override void Awake()
    {
        base.Awake();

        if (!enabled)
            return;

        if (projectilePrefab == null)
        {
            MyDebug.LogError($"Projectile prefab is not set on {gameObject.name}");
            enabled = false;
            return;
        }

        if (attackPoint == null)
        {
            MyDebug.LogError($"AttackPoint is not set on {gameObject.name}");
            enabled = false;
        }
    }

    public override void Init(ActorData data)
    {
        base.Init(data);
        EnsureProjectilePool();
    }

    public override void OnNormalAttackEvent()
    {
        if (!CanProcess() || pendingAttackCount <= 0)
            return;

        pendingAttackCount--;

        var projectile = GetProjectile();
        if (projectile == null)
            return;

        var direction = GetFireDirection();

        projectile.Pool = projectilePool;
        projectile.transform.SetPositionAndRotation(attackPoint.position, GetProjectileRotation(direction));
        projectile.Play(direction, AttackPower, Actor);
    }

    protected override void ExecuteAttack(SkillCommand command)
    {
        // 투사체 발사는 OnNormalAttackEvent 애니메이션 이벤트에서 처리한다.
        pendingAttackCount++;
    }

    protected override void ResetAttack()
    {
        pendingAttackCount = 0;
    }

    Projectile GetProjectile()
    {
        EnsureProjectilePool();
        return projectilePool?.Get();
    }

    void EnsureProjectilePool()
    {
        if (projectilePool != null)
            return;

        projectilePool = new MyObjectPool<Projectile>(projectilePrefab);
    }

}
