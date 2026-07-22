using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using static Cysharp.Threading.Tasks.UniTask;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Spawner : MonoBehaviour
{
    [SerializeField] Enemy actorPrefab;
    [SerializeField] int spawnMaxCount = 2;
    [SerializeField] int msSpawnTime = 5000;
    [SerializeField] float spawnRadius = 3f;

    public IReadOnlyList<Enemy> SpawnedActors
    {
        get
        {
            RemoveReleasedActors();
            return spawnedActors;
        }
    }

    public int SpawnedCount => SpawnedActors.Count;

    int SpawnIntervalMs => Mathf.Max(1, msSpawnTime);

    readonly List<Enemy> spawnedActors = new();

    MyObjectPool<Enemy> actorPool;
    CancellationTokenSource spawnCancellation;
    bool isSpawning;

#if UNITY_EDITOR
    int nameIndex;
#endif

    void Start()
    {
        GameManager.Instance.AddSpawner(this);
        CreateSpawner();
    }

    void OnEnable()
    {
        if (actorPool != null)
            PlaySpawner();
    }

    void OnDisable()
    {
        StopSpawner();
    }

    void OnDestroy()
    {
        StopSpawner();
        GameManager.Instance?.RemoveSpawner(this);
    }

    public void CreateSpawner(Enemy prefab, int spawnTimeMs)
    {
        actorPrefab = prefab;
        msSpawnTime = spawnTimeMs;
        actorPool = null;

        CreateSpawner();
    }

    public void CreateSpawner()
    {
        if (actorPool == null)
            actorPool = CreatePool();

        PlaySpawner();
    }

    MyObjectPool<Enemy> CreatePool()
    {
        if (actorPrefab != null)
            return new MyObjectPool<Enemy>(actorPrefab);

        MyDebug.LogError($"Spawner actor is not set: {gameObject.name}");
        return null;
    }

    void PlaySpawner()
    {
        if (actorPool == null)
        {
            MyDebug.LogError("Spawner is not set");
            return;
        }

        if (isSpawning)
            return;

        spawnCancellation?.Cancel();
        spawnCancellation?.Dispose();
        spawnCancellation = new CancellationTokenSource();
        isSpawning = true;

        StartSpawn(spawnCancellation.Token).Forget();
    }

    void StopSpawner()
    {
        if (spawnCancellation == null)
            return;

        spawnCancellation.Cancel();
        spawnCancellation.Dispose();
        spawnCancellation = null;
        isSpawning = false;
    }

    async UniTaskVoid StartSpawn(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                await Delay(SpawnIntervalMs, cancellationToken: token);

                if (!GameManager.Instance.IsPlaying)
                    continue;

                TrySpawn();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            isSpawning = false;
        }
    }

    void TrySpawn()
    {
        RemoveReleasedActors();
        if (spawnedActors.Count >= spawnMaxCount)
            return;

        var actor = actorPool.Get();
        if (actor == null)
            return;

        if (!spawnedActors.Contains(actor))
            spawnedActors.Add(actor);
        
        var spawnPoint = GetSpawnPoint();
        actor.Spawn(spawnPoint, transform.rotation, actorPool, RemoveSpawnedActor);

#if UNITY_EDITOR
        var actorName = actorPrefab != null ? actorPrefab.name : nameof(Enemy);
        actor.name = $"{actorName}_{++nameIndex}";
        MyDebug.Log($"Spawn actor. spawner: {gameObject.name}, pos: {spawnPoint}");
#endif
    }

    void RemoveSpawnedActor(Enemy actor)
    {
        spawnedActors.Remove(actor);
    }

    void RemoveReleasedActors()
    {
        spawnedActors.RemoveAll(actor => actor.IsNullOrDestroyed() || !actor.gameObject.activeSelf);
    }

    Vector3 GetSpawnPoint()
    {
        var offset = Random.insideUnitCircle * spawnRadius;
        var spawnPoint = transform.position + new Vector3(offset.x, 0f, offset.y);

        return spawnPoint;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Handles.DrawSolidDisc(transform.position, Vector3.up, spawnRadius);

        Gizmos.color = new Color(0.1f, 0.55f, 1f, 1f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.DrawCube(transform.position, Vector3.one * 0.2f);

        Handles.Label(transform.position + Vector3.up * 0.5f, $"{gameObject.name} ({SpawnedCount}/{spawnMaxCount})");
    }
#endif
}
