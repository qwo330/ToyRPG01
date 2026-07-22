using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class MyObjectPool<T> where T : MonoBehaviour
{
    public bool collectionChecks = true;
    public int defaultCapacity = 10;
    public int maxPoolSize = 10;

    readonly string assetAddress;
    readonly T prefab;
    
    public MyObjectPool(T prefab)
    {
        this.prefab = prefab;
    }

    [Obsolete("path 방식 사용하지 않음")]
    public MyObjectPool(string assetAddress)
    {
        this.assetAddress = assetAddress;
    }
    
    ObjectPool<T> pool;
    public ObjectPool<T> Pool
    {
        get
        {
            if (pool == null)
            {
                pool = new ObjectPool<T>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, defaultCapacity, maxPoolSize);
            }

            return pool;
        }
    }

    public T Get()
    {
        return Pool.Get();
    }
    
    public void Release(T item)
    {
        Pool.Release(item);
    }
    
    T CreatePooledItem()
    {
        var source = prefab != null ? prefab : AddressableTool.LoadAsset<T>(assetAddress);
        if (source == null)
        {
            MyDebug.LogError($"Cannot create pooled item. Source is null: {typeof(T).Name}, {assetAddress}");
            return null;
        }

        var item = Object.Instantiate(source);
        item.gameObject.SetActive(false);
        return item;
    }
    
    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(T item)
    {
        if (item == null)
            return;

        item.gameObject.SetActive(false);
    }
    
    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(T item)
    {
        if (item == null)
            return;

        item.gameObject.SetActive(true);
    }
    
    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(T item)
    {
        if (item == null)
            return;

        // Destroy(item.gameObject);
        Object.Destroy(item.gameObject);
    }
}
