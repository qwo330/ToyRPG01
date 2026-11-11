using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMono<GameManager>
{
    EGameState currentState;
    public EGameState CurrentSate => currentState;
    public bool IsPlaying => currentState == EGameState.Play;
    
    Dictionary<(int, int), float> heightMapData;

    public Dictionary<(int, int), float> HeightMapData
    {
        set => heightMapData = value;
    }

    List<Spawner> spawnerList = new();
    IDisposable playerDeathSubscription;
    int startedSceneHandle = -1;
    bool isGameOver;
    
    public bool HasMapData(int x, int z) => heightMapData.ContainsKey((x, z));
    
    public float GetHeight(int x, int z)
    {
        if (HasMapData(x, z))
        {
            return heightMapData[(x, z)];
        }

        return -999; // 해당 좌표의 데이터가 없을 때 반환할 값
    }

    public void AddSpawner(Spawner newSpanwer)
    {
        if (spawnerList.Contains(newSpanwer))
        {
            return;
        }

        MyDebug.Log("[Spanwer] Add spanwer");
        spawnerList.Add(newSpanwer);
    }

    public void RemoveSpawner(Spawner oldSpanwer)
    {
        if (spawnerList.Contains(oldSpanwer))
        {
            MyDebug.Log("[Spanwer] remove spanwer");
            spawnerList.Remove(oldSpanwer);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartScene(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        playerDeathSubscription?.Dispose();
        playerDeathSubscription = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            StartScene(scene);
    }

    void StartScene(Scene scene)
    {
        if (startedSceneHandle == scene.handle)
            return;

        startedSceneHandle = scene.handle;
        currentState = EGameState.None;
        StartSceneAsync(destroyCancellationToken).Forget();
    }

    async UniTask StartSceneAsync(CancellationToken cancellationToken)
    {
        // PlayerInput.Start와 스포너 등록이 끝난 뒤 플레이어를 연결한다.
        await UniTask.NextFrame(cancellationToken: cancellationToken);
        var player = ActorManager.Instance.GetLocalPlayer();
        if (player == null)
        {
            MyDebug.LogError("Cannot start game: local player is not registered.");
            return;
        }

        playerDeathSubscription?.Dispose();
        playerDeathSubscription = player.OnDead.Subscribe(_ => GameOver());
        isGameOver = false;
        GameStart();
    }

    public void GameOver()
    {
        if (isGameOver || !IsPlaying)
            return;

        isGameOver = true;
        currentState = EGameState.Stop;
        GameOverAsync(destroyCancellationToken).Forget();
    }

    async UniTask GameOverAsync(CancellationToken cancellationToken)
    {
        var scenePath = SceneManager.GetActiveScene().path;
        var cleanupTasks = new (string Name, Func<CancellationToken, UniTask> ExecuteAsync)[]
        {
            ("Return spawned monsters", ReturnSpawnedActorsAsync),
            ("Reset actor commands", ResetActorCommandsAsync),
            ("Clear scene registrations", ClearSceneRegistrationsAsync)
        };

        string currentTask = "Wait for actor processing";
        try
        {
            await UniTask.NextFrame(cancellationToken: cancellationToken);
            foreach (var task in cleanupTasks)
            {
                currentTask = task.Name;
                cancellationToken.ThrowIfCancellationRequested();
                await task.ExecuteAsync(cancellationToken);
            }

            currentTask = "Reload scene";
#if UNITY_EDITOR
            await UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                scenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
#endif
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            MyDebug.LogError($"Game over failed at '{currentTask}': {exception}");
        }
    }

    async UniTask ReturnSpawnedActorsAsync(CancellationToken cancellationToken)
    {
        foreach (var spawner in spawnerList)
        {
            if (spawner != null)
                await spawner.StopAndReturnActorsAsync(cancellationToken);
        }
    }

    UniTask ResetActorCommandsAsync(CancellationToken cancellationToken)
    {
        ResetManagers();
        return UniTask.CompletedTask;
    }

    UniTask ClearSceneRegistrationsAsync(CancellationToken cancellationToken)
    {
        playerDeathSubscription?.Dispose();
        playerDeathSubscription = null;
        spawnerList.Clear();
        return UniTask.CompletedTask;
    }

    void Update()
    {
        GetPlayerInput();
    }

    public void InitManagers()
    {
        ActorManager.Instance.Init();
    }

    public void ResetManagers()
    {
        ActorManager.Instance.Reset();
    }

    public void GameStart()
    {
        MyDebug.Log("Game Start");
        currentState = EGameState.Play;

        if (Camera.main.TryGetComponent(out CameraController camController))
        {
            var player = ActorManager.Instance.GetLocalPlayer();
            camController.SetTarget(player);
        }
        
        foreach (var s in spawnerList)
        {
            s.CreateSpawner();
        }
    }
    
    void GetPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Keypad1
        {
            MyDebug.LogError("1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            MyDebug.LogError("2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            MyDebug.LogError("3");
        }
    }
}

public enum EGameState
{
    None,
    Play,
    Stop,
}
