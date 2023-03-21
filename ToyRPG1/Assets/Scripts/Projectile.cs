using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float sqrEffectiveRange = 25f;
    
    Vector3 startPoint;
    Vector3 direction;
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
        if (other.CompareTag("Map"))
        {
            OnReturnToPool();
        }
        else if (other.CompareTag("Monster"))
        {
            Actor target = other.GetComponent<Actor>();
            Hit(target);
        }
    }

    public void Play(Vector3 direction, int power)
    {
        this.startPoint = transform.position;
        this.direction = direction;
        this.power = power;
        
        isPlaying = true;
    }
    
    public void Hit(Actor target)
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
        
        target.TakeDamage(power);
        OnReturnToPool();
    }
    
    void OnReturnToPool()
    {
        isPlaying = false;
        
        pool.Release(this);
    }
}