using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMono<GameManager>
{
    void Update()
    {
        GetPlayerInput();
    }

    void GetPlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Keypad1
        {
            Debug.LogError("1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.LogError("2");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.LogError("3");
        }
    }
}
