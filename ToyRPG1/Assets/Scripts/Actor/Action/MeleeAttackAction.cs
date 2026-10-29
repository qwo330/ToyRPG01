using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackAction : AttackAction
{
    [SerializeField] Transform attackPoint;
    [SerializeField] LayerMask targetLayers;
    [SerializeField] float hitRadius = 0.5f;
    [SerializeField] int hitMaxCount = 8;

    Collider[] hitColliders;
    readonly HashSet<Actor> hitActors = new();
    int pendingHitCount;

    protected override void Awake()
    {
        base.Awake();

        if (!enabled)
            return;

        if (attackPoint == null)
        {
            MyDebug.LogError($"AttackPoint is not set on {gameObject.name}");
            enabled = false;
            return;
        }

        if (targetLayers.value == 0)
        {
            MyDebug.LogError($"Target layers are not set on {gameObject.name}");
            enabled = false;
            return;
        }

        hitColliders = new Collider[Mathf.Max(1, hitMaxCount)];
    }

    public override void OnNormalAttackEvent()
    {
        if (!CanProcess() || pendingHitCount <= 0)
            return;

        pendingHitCount--;
        var hitCount = Physics.OverlapSphereNonAlloc(
            attackPoint.position,
            hitRadius,
            hitColliders,
            targetLayers.value & ~GameLayers.DeadActorMask,
            QueryTriggerInteraction.Ignore);

        hitActors.Clear();
        for (var i = 0; i < hitCount; i++)
        {
            var collider = hitColliders[i];
            if (collider == null)
                continue;

            var target = collider.GetComponentInParent<Actor>();
            if (target == null || target == Actor || target.IsDead || !hitActors.Add(target))
                continue;

            target.TakeDamage(new DamageInfo(Actor, AttackPower, target.transform.position));
        }

        System.Array.Clear(hitColliders, 0, hitColliders.Length);
    }

    protected override void ExecuteAttack(SkillCommand command)
    {
        pendingHitCount++;
    }

    protected override void ResetAttack()
    {
        pendingHitCount = 0;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, hitRadius);
    }
#endif
}
