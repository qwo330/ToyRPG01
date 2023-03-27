using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    List<Spawner> spawnerList = new List<Spawner>();
    
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
        currentState = EGameState.None;
        GameStart();
    }

    void Update()
    {
        GetPlayerInput();
    }

    public void GameStart()
    {
        MyDebug.Log("Game Start");
        currentState = EGameState.Play;

        foreach (Spawner VARIABLE in spawnerList)
        {
            VARIABLE.CreateSpawner();
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