using UnityEngine;

public class Singleton<T> where T : class, new()
{
    static T instance;
    static readonly object @lock = new object();
    static bool applicationIsQuitting = false;
    
    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                MyDebug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            lock (@lock)
            {
                if (instance == null)
                {
                    instance = new T();
                    
                    if (instance is Singleton<T> s)
                    {
                        s.Init();
                    }
                }

                return instance;
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }

    public virtual void Init()
    {
        
    }

    public virtual void Reset()
    {
        
    }
}

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    static T instance;
    static readonly object @lock = new ();
    static bool applicationIsQuitting = false;

    void Awake()
    {
        applicationIsQuitting = false;
    }

    public static T Instance
    {
        get
        {
            if (applicationIsQuitting)
            {
                MyDebug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            lock (@lock)
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<T>();

                    if (FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
                    {
                        MyDebug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");
                        return instance;
                    }

                    if (instance == null)
                    {
                        GameObject singleton = new GameObject($"(singleton) {typeof(T)}");
                        instance = singleton.AddComponent<T>();

                        DontDestroyOnLoad(singleton);
                    }

                    if (instance is SingletonMono<T> s)
                    {
                        s.Init();
                    }
                }

                return instance;
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        applicationIsQuitting = true;
    }
    
    public virtual void Init()
    {
        
    }
    
    public virtual void Reset()
    {
        
    }
}