using System.Threading;
using Cysharp.Threading.Tasks;

using UnityEngine;
using static Cysharp.Threading.Tasks.UniTask;

public class Spawner : MonoBehaviour
{
    [SerializeField] int msSpawnTime = 5000;
    [SerializeField] string spawnMonsterPath = "Monster/Zombie.prefab";
    MyObjectPool<Monster> spawnMonsterPool;
    
    //List<Monster> spawnedMonsterList; // 수량 관리를 위한 리스트
    int spawnedMonsterCount = 0;

    CancellationTokenSource disableCancellation;
    
    #if UNITY_EDITOR
    int nameIndex = 0;
    #endif

    void OnEnable()
    {
        if (disableCancellation != null)
        {
            disableCancellation.Dispose();
        }
        
        disableCancellation = new CancellationTokenSource();
    }

    void OnDisable()
    {
        disableCancellation.Cancel();
    }
    
    void Start()
    {
        GameManager.Instance.AddSpawner(this);
        CreateSpawner(spawnMonsterPath, msSpawnTime);
    }

    void OnDestroy()
    {
        OnDisable();
        GameManager.Instance.RemoveSpawner(this);
    }

    public void CreateSpawner(string monsterPath, int msSpawnTime)
    {
        this.msSpawnTime = msSpawnTime;
        spawnMonsterPath = monsterPath;
        spawnMonsterPool = new MyObjectPool<Monster>(monsterPath);

        PlaySpawner();
    }

    public void CreateSpawner()
    {
        spawnMonsterPool = new MyObjectPool<Monster>(spawnMonsterPath);

        PlaySpawner();
    }

    public void PlaySpawner()
    {
        if (spawnMonsterPool == null)
        {
            MyDebug.LogError("Spanwer is not Set");
            return;
        }
        
        StartSpawn();
    }
    
    // void DestroyMonster(Monster monster)
    // {
    //     if (spawnedMonsterList.Contains(monster))
    //     {
    //         spawnedMonsterList.Remove(monster);
    //     }
    // }

    void DiscountMonster()
    {
        if (spawnedMonsterCount < 1)
        {
            return;
        }
        
        spawnedMonsterCount--;
    }

    async UniTaskVoid StartSpawn()
    {
        while (GameManager.Instance.IsPlaying)
        {
            await Delay(msSpawnTime, cancellationToken: disableCancellation.Token);

            Monster monster = spawnMonsterPool.Get();
            if (monster == null)
            {
                continue;
            }
            
            MyDebug.LogError($"spawn monster, monster pos : {monster.transform.position}, spawner pos : {transform.position}");

            monster.Pool = spawnMonsterPool;
            monster.SpawnMonster(transform.position);
            spawnedMonsterCount++;
            
            #if UNITY_EDITOR
            monster.name = $"{monster.name}_{++nameIndex}";
#endif
        }
    }
}
