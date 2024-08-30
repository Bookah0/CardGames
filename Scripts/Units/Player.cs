using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum Difficulty { Easy, Medium, Hard, UserControlled };
public enum PlayerType { Left, Top, Right, UserControlled }

public class Player : MonoBehaviour
{
    private Difficulty difficulty;
    private PlayerType type;
    protected List<Card> cardsToSwitch = new();
    public GameObject handObj;
    public GameObject dropArea;
    public TextMeshProUGUI turnScoreObj;
    public TextMeshProUGUI totalScoreObj;
    protected Hand hand;
    protected int turn;
    protected Card winningCard = null;
    protected bool heartsBroken = false;
    private Player playerToSwitchCardsWith;
    private int turnScore = 0;
    private int totalScore = 0;
    private string playerName;

    public event Action<GameState> OnPlayerDoneChoosingSwitch;

    private void Start()
    {

        hand = handObj.GetComponent<Hand>();
    }

    public void RemoveCardToSwitch(Card card)
    {
        cardsToSwitch.Remove(card);
        card.SetBorderColor(Color.black);
    }

    public void AddCardToSwitch(Card card)
    {
        cardsToSwitch.Add(card);
        card.SetBorderColor(Color.red);
        if (card.GetClickableScript() != null && cardsToSwitch.Count == 3)
        {
            OnPlayerDoneChoosingSwitch?.Invoke(GameState.SwitchCards);
        }
    }

    public void SetCardsToSwitch(List<Card> cards)
    {
        cardsToSwitch = cards;
    }

    public void SwitchCards(Player playerToSwitchTo)
    {
        Debug.Log("Cards to switch of len " + cardsToSwitch.Count);
        foreach (var card in cardsToSwitch)
        {
            var handToSwitchTo = playerToSwitchTo.GetHandScript();
            if (hand.isPlayer)
            {
                card.TurnCard(false);
            } else if (handToSwitchTo.isPlayer)
            {
                card.TurnCard(true);
            }
            hand.MoveCardToOtherHand(card, handToSwitchTo);
        }
    }

    public void SetTurnScoreText(string text)
    {
        turnScoreObj.text = text;
     
    }

    public void SetTotalScoreText(string text)
    {
        totalScoreObj.text = text;
    }

    public Hand GetHandScript()
    {
        return hand;
    }

    public List<Card> GetHand()
    {
        return hand.GetHand();
    }

    public bool IsHandEmpty()
    {
        return hand.GetHand().Count == 0;
    }

    public void SetName(string playerName)
    {
        this.playerName = playerName;
    }

    internal void ResetTurnScore()
    {
        turnScore = 0;
    }

    public void SetPlacedCard(Card card)
    {
        GameManager.Instance.AddPlacedCard(card, this);
        hand.RemoveCardFromHand(card);
    }

    public void RemovePlayedCards()
    {
        foreach (Transform child in dropArea.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public int GetTotalScore()
    {
        return totalScore;
    }

    public string GetName()
    {
        return playerName;
    }

    public void IncreaseTotalScore(int n)
    {
        totalScore += n;
    }

    public int GetTurnScore()
    {
        return turnScore;
    }

    internal void IncreaseTurnScore(int n)
    {
        turnScore += n;
    }

    public Player GetPlayerToSwitchWith()
    {
        return playerToSwitchCardsWith;
    }

    public void SetPlayerToSwitchWith(Player player)
    {
        playerToSwitchCardsWith = player;
    }
}
