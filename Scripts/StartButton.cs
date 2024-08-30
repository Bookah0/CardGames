using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    public static void StartGame()
    {
        GameManager.Instance.ChangeState(GameState.RoundStart);
    }
}
