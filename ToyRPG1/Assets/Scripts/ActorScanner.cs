using System;

using UnityEngine;

public class ActorScanner : MonoBehaviour
{
    [SerializeField] float scanRadius;
    [SerializeField] LayerMask targetLayer;
    [SerializeField] RaycastHit[] targets;

    [SerializeField] int scanMaxCount = 10;

    public void FindTargets()
    {
        // 배열 초기화 주의, 성능 최적화를 위해 nonAlloc 사용 (array)
        RaycastHit[] result = new RaycastHit[scanMaxCount];
        Physics.SphereCastNonAlloc(transform.position, scanRadius, transform.forward, result, scanRadius, layerMask:targetLayer);
        
        if (result[1].transform != null)
        {
            // 최소 2개는 있어야 정렬
            Array.Sort(result, NearCompare);
        }

        targets = result;
    }

    public Actor GetTarget()
    {
        FindTargets();
        
        if (targets.Length > 0 && targets[0].transform != null)
        {
            return targets[0].transform.GetComponent<Actor>();
        }

        return null;
    }

    int NearCompare(RaycastHit x, RaycastHit y)
    {
        if (x.transform == null && y.transform == null)
        {
            return 0;
        }
        else if (x.transform != null && y.transform == null)
        {
            return -1;
        }
        else if (x.transform == null && y.transform != null)
        {
            return 1;
        }
        else
        {
            Vector3 position = transform.position;
            float xDist = (x.transform.position - position).sqrMagnitude;
            float yDist = (y.transform.position - position).sqrMagnitude;

            return xDist.CompareTo(yDist);
        }
    }
    

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }

#endif
}