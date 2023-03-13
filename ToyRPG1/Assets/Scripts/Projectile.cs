using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    
    Vector3 direction;
    public Vector3 Direction
    {
        set => direction = value;
    }
    
    int power;
    public int Power
    {
        set => power = value;
    }
    
    MyObjectPool<Projectile> pool;
    public MyObjectPool<Projectile> Pool
    {
        set => pool = value;
    }

    void FixedUpdate()
    {
        if (direction == Vector3.zero)
        {
            MyDebug.LogError("Direction is not set");
        }
        
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    public void Hit(Actor target)
    {
        if (power == 0)
        {
            MyDebug.LogError("Power is not set");
        }
        
        target.TakeDamage(power);
    }
    
    void OnReturnToPool()
    {
        pool.Release(this);
    }
}