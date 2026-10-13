using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float sqrEffectiveRange = 25f;
    [SerializeField] LayerMask targetLayers = GameLayers.EnemyMask;
    
    Vector3 startPoint;
    Vector3 direction;
    Actor owner;
    int power;
    bool isPlaying;
    
    bool IsInvaildRange => (transform.position - startPoint).sqrMagnitude > sqrEffectiveRange;
    
    MyObjectPool<Projectile> pool;
    public MyObjectPool<Projectile> Pool
    {
        set => pool = value;
    }

    void FixedUpdate()
    {
        if (isPlaying == false)
        {
            return;
        }

        if (IsInvaildRange)
        {
            OnReturnToPool();
        }

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsTargetLayer(other))
        {
            Actor target = other.GetComponentInParent<Actor>();
            Hit(target, other.ClosestPoint(transform.position));
        }
        else
        {
            MyDebug.Log(other.name + " is not a target");
            OnReturnToPool();
        }
    }

    public void Play(Vector3 direction, int power)
    {
        Play(direction, power, null);
    }

    public void Play(Vector3 direction, int power, Actor owner)
    {
        this.startPoint = transform.position;
        this.direction = direction;
        this.power = power;
        this.owner = owner;
        
        isPlaying = true;
    }
    
    public void Hit(Actor target, Vector3 hitPoint)
    {
        if (target == null)
        {
            MyDebug.LogError("Target is Null");
            return;
        }
        
        if (power == 0)
        {
            MyDebug.LogError("Power is not set");
        }
        
        target.TakeDamage(new DamageInfo(owner, power, hitPoint));
        OnReturnToPool();
    }

    bool IsTargetLayer(Collider target)
    {
        return (targetLayers.value & (1 << target.gameObject.layer)) != 0;
    }
    
    void OnReturnToPool()
    {
        isPlaying = false;
        owner = null;

        if (pool == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        pool.Release(this);
    }
}
