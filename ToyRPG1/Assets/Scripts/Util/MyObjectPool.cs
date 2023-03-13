using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class MyObjectPool<T> where T : MonoBehaviour
{
    public bool collectionChecks = true;
    public int defaultCapacity = 10;
    public int maxPoolSize = 10;

    string assetPath = string.Empty;
    // public string AssetPath
    // {
    //     set => assetPath = value;
    // }
    
    public MyObjectPool(string assetPath)
    {
        this.assetPath = assetPath;
    }
    
    IObjectPool<T> pool;
    public IObjectPool<T> Pool
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
        T asset = AddressableTool.LoadAsset<T>(assetPath);
        T prefab = Object.Instantiate(asset);
        return prefab;
    }
    
    // Called when an item is returned to the pool using Release
    void OnReturnedToPool(T item)
    {
        item.gameObject.SetActive(false);
    }
    
    // Called when an item is taken from the pool using Get
    void OnTakeFromPool(T item)
    {
        item.gameObject.SetActive(true);
    }
    
    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    void OnDestroyPoolObject(T item)
    {
        // Destroy(item.gameObject);
        Object.Destroy(item.gameObject);
    }
}