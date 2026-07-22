using UnityEngine;
using UnityEngine.Serialization;

public class ActorScanner : MonoBehaviour
{
    [SerializeField] float scanRadius;
    [FormerlySerializedAs("targetLayer")]
    [SerializeField] LayerMask targetLayers;
    [SerializeField] int scanMaxCount = 10;

    [ReadOnly, SerializeField] Collider[] targets;
    [ReadOnly, SerializeField] int targetCount;

    public void Configure(float radius, LayerMask layers)
    {
        scanRadius = radius;
        targetLayers = layers;

        EnsureTargetBuffer();
    }

    public Actor GetTarget()
    {
        FindTargets();

        Actor closest = null;
        var closestSqrDistance = float.PositiveInfinity;
        var origin = transform.position;

        for (var i = 0; i < targetCount; i++)
        {
            var targetCollider = targets[i];
            if (targetCollider == null)
                continue;

            var actor = targetCollider.GetComponentInParent<Actor>();
            if (actor == null || actor.gameObject == gameObject)
                continue;

            var sqrDistance = (actor.transform.position - origin).sqrMagnitude;
            if (sqrDistance >= closestSqrDistance)
                continue;

            closest = actor;
            closestSqrDistance = sqrDistance;
        }

        return closest;
    }

    public void FindTargets()
    {
        EnsureTargetBuffer();

        if (scanRadius <= 0f || targetLayers.value == 0)
        {
            targetCount = 0;
            return;
        }

        targetCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            scanRadius,
            targets,
            targetLayers,
            QueryTriggerInteraction.Ignore);
    }

    void EnsureTargetBuffer()
    {
        var capacity = Mathf.Max(1, scanMaxCount);
        if (targets == null || targets.Length != capacity)
            targets = new Collider[capacity];
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
#endif
}
