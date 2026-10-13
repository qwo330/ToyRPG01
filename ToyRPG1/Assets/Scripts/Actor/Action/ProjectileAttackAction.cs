using UnityEngine;

public class ProjectileAttackAction : AttackAction
{
    [SerializeField] Projectile projectilePrefab;
    [SerializeField] Transform firePoint;

    MyObjectPool<Projectile> projectilePool;
    bool didLogMissingPrefab;

    public override void Init(ActorData data)
    {
        base.Init(data);
        EnsureProjectilePool();
    }

    public override void OnNormalAttackEvent()
    {
        var projectile = GetProjectile();
        if (projectile == null)
            return;

        var origin = firePoint != null ? firePoint : transform;
        var direction = GetFireDirection();

        projectile.Pool = projectilePool;
        projectile.transform.SetPositionAndRotation(origin.position, GetProjectileRotation(direction));
        projectile.Play(direction, AttackPower, Actor);
    }

    protected override void ExecuteAttack(ActorSnapshot snapshot)
    {
        // 투사체 발사는 OnNormalAttackEvent 애니메이션 이벤트에서 처리한다.
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

        if (projectilePrefab == null)
        {
            if (!didLogMissingPrefab)
            {
                MyDebug.LogError($"Projectile prefab is not set on {gameObject.name}");
                didLogMissingPrefab = true;
            }

            return;
        }

        projectilePool = new MyObjectPool<Projectile>(projectilePrefab);
    }

}
