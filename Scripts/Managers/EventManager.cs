using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void OnHeartsPlayed();
    public static event OnHeartsPlayed onHeartsPlayed;

    public delegate void OnTurnPass();
    public static event OnTurnPass onTurnPass;

    public delegate bool OnTryPlayCard(Card card);
    public static event OnTryPlayCard onTryPlayCard;

    public static bool TryPlayCard(Card card)
    {
        if(onTryPlayCard != null){
            if (onTryPlayCard.Invoke(card) == false)
            {
                Debug.Log("Cant play card");
                return false;
            }
            else
            {
                Debug.Log("Can play card");
                return true;
            }
        }
        Debug.Log("onTryPlayCard is null");
        return false;
    }

    public static void PassTurn()
    {
        onTurnPass?.Invoke();
    }

    public static void HeartsPlayed()
    {
        onHeartsPlayed?.Invoke();
    }
}
