using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum GameState { MainMenu, RoundStart, SwitchCards, PlayerTurn, OpponentsTurn, RoundDone, DecideWinner };

public class GameManager : StaticInstance<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameObject leftOpponentObj;
    public GameObject topOpponentObj;
    public GameObject rightOpponentObj;
    public GameObject playerObj;
    public TextMeshProUGUI playerNameTextObj;
    public GameObject mainMenuObj;
    public GameObject scorePanelObj;

    private Player leftOpponent;
    private Player topOpponent;
    private Player rightOpponent;
    private Player userControlledplayer;
    private List<Player> players;

    private Queue<Player> turnOrder = new();
    private Player currentTurn;
    private Player first;
    private bool heartsBroken = false;
    private Dictionary<Player, Vector2> playerPositions;
    private Dictionary<Player, Card> placedCardsDict = new();
    private List<Card> placedCardsList = new();
    private AIManager AIManager;
    private int round = 0;
    private ScoreManager scoreManager;

    public GameState state { get; private set; }

    void Start()
    {
        if(Instance == null)
        {
            Debug.Log("Instance is null");
        }

        ChangeState(GameState.MainMenu);
        EventManager.onHeartsPlayed += SetHeartsBroken;
        EventManager.onTurnPass += NextTurn;

        AIManager = gameObject.GetComponent<AIManager>();
        userControlledplayer = playerObj.GetComponent<Player>();
        leftOpponent = leftOpponentObj.GetComponent<Player>();
        topOpponent = topOpponentObj.GetComponent<Player>();
        rightOpponent = rightOpponentObj.GetComponent<Player>();
        leftOpponent.SetName("Ruben");
        topOpponent.SetName("Beatrice");
        rightOpponent.SetName("Yvonne");

        players = new() { leftOpponent, topOpponent, rightOpponent, userControlledplayer };
        playerPositions = new()
        {
            { leftOpponent, new Vector2(-100f, 300f) },
            { topOpponent, new Vector2(480f, 800f) },
            { rightOpponent, new Vector2(1280f, 300f) },
            { userControlledplayer, new Vector2(480f, -300f) },
        };

        userControlledplayer.OnPlayerDoneChoosingSwitch += gameState => ChangeState(gameState);
        EventManager.onTryPlayCard += (card) => CanPlayCard(card);
    }

    private void Delay(float duration, Action action)
    {
        StartCoroutine(DelayedAction(duration, action));
    }

    private IEnumerator DelayedAction(float duration, Action action)
    {
        yield return new WaitForSeconds(duration);
        action?.Invoke();
    }

    public void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);
        state = newState;

        switch (state)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.RoundStart:
                HandleRoundStart();
                break;
            case GameState.SwitchCards:
                HandleSwitchCards();
                break;
            case GameState.PlayerTurn:
                HandlePlayerTurn();
                break;
            case GameState.OpponentsTurn:
                Delay(1f, () => HandleOpponentTurn(currentTurn));
                break;
            case GameState.DecideWinner:
                Delay(1f, () => HandleDecideWinner());
                break;
            case GameState.RoundDone:
                Delay(1f, () => HandleRoundDone());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        OnAfterStateChanged?.Invoke(newState);
    }

    private void HandleMainMenuState()
    {
        mainMenuObj.SetActive(true);
        scorePanelObj.SetActive(false);
    }

    private void HandleRoundStart()
    {
        round += 1;
        ClearDropAreas();
        SetPlayersToSwitchWith();
        userControlledplayer.SetName(playerNameTextObj.text);
        mainMenuObj.SetActive(false);
        scorePanelObj.SetActive(false);
        ScoreManager.Instance.ResetTurnScores(players);
        Give(ShuffleDeck(InitDeck()), 13);
    }

    private void HandleSwitchCards()
    {
        SwitchCards();
        InitTurnOrder();
        NextTurn();
    }

    private void HandleOpponentTurn(Player opponent)
    {
        AIManager.PlayCard(opponent);
        NextTurn();
    }

    private void HandlePlayerTurn()
    {
        
    }

    private void HandleDecideWinner()
    {
        var winner = GetWinner();
        ScoreManager.Instance.CalculateTurnScore(winner, players);

        foreach (Card card in placedCardsList)
        {
            card.SetLerping(true);
            card.SetLerpPositions(card.transform.position, playerPositions[winner]);
        }

        ClearDropAreas();

        if (userControlledplayer.GetHand().Count == 0)
        {
            ChangeState(GameState.RoundDone);
        }
        else
        {
            InitTurnOrder(winner);
            NextTurn();
        }
    }
    
    private void HandleRoundDone()
    {
        RemovePlayedCards();
        ScoreManager.Instance.CalculateRoundScore(players);
        scorePanelObj.SetActive(true);
    }

    /// --------------------- Switch methods ---------------------
    
    public void SwitchCards()
    {
        foreach (Player player in players)
        {
            if (player != userControlledplayer)
            {
                AIManager.ChooseCardsToSwitch(player);
            }
            player.SwitchCards(player.GetPlayerToSwitchWith());
        }

        foreach (Card card in userControlledplayer.GetHand())
        {
            card.GetComponent<Draggable>().enabled = true;
        }
    }

    public void SetPlayersToSwitchWith()
    {
        var nPlayers = players.Count;
        var roundMod = (round % 3) + 1;

        for (int i = 0; i < nPlayers; i++)
        {
            if(i + roundMod > nPlayers-1)
            {
                players[i].SetPlayerToSwitchWith(players[i + roundMod - 4]);
            }
            else
            {
                players[i].SetPlayerToSwitchWith(players[i + roundMod]);
            }
        }
    }

    /// --------------------- Deck methods ---------------------
    
    public void Give(Queue<CardData> deck, int nCardsToGive)
    {
        foreach (Player player in players)
        {
            for (int i = 0; i < nCardsToGive; i++)
            {
                player.GetHandScript().AddNewCard(deck.Dequeue());
            }
        }
    }

    public Queue<CardData> ShuffleDeck(CardData[] deck)
    {
        System.Random rng = new System.Random();

        for (int i = deck.Length - 1; i > 0; i--)
        {
            int k = rng.Next(i + 1);
            CardData card = deck[k];
            deck[k] = deck[i];
            deck[i] = card;
        }
        return new Queue<CardData>(deck);
    }

    private CardData[] InitDeck()
    {
        var deck = new CardData[52];
        int count = 0;

        for (int i = 2; i <= 14; i++)
        {
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                deck[count] = new CardData(suit, i);
                count++;
            }
        }

        return deck;
    }

    /// --------------------- Turn order methods ---------------------

    public void NextTurn()
    {
        if(turnOrder.Count == 0)
        {
            ChangeState(GameState.DecideWinner);
        }
        else
        {
            currentTurn = turnOrder.Dequeue();
            if (currentTurn != userControlledplayer)
            {
                ChangeState(GameState.OpponentsTurn);
            }
            else
            {
                ChangeState(GameState.PlayerTurn);
            }
        }
    }

    public int GetTurn()
    {
        return placedCardsList.Count + 1;
    }

    public bool IsUserFirst()
    {
        return first == userControlledplayer;
    }

    private void InitTurnOrder()
    {
        foreach (Player player in players)
        {
            if (player.GetHandScript().GetCardInHand(Suit.Clubs, 2) != null)
            {
                InitTurnOrder(player);
                Debug.Log(player.name);
                return;
            }
        }
        Debug.Log("Noone has the 2 of clubs");
    }

    public void InitTurnOrder(Player first)
    {
        this.first = first;
        var firstFound = false;

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == first || firstFound)
            {
                turnOrder.Enqueue(players[i]);
                firstFound = true;
            }
        }

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i] == first)
            {
                break;
            }
            else
            {
                turnOrder.Enqueue(players[i]);
            }
        }

        var order = "";
        foreach (var player in turnOrder)
        {
            order = order + player.name + ", ";
        }
        Debug.Log(order);
    }

    /// --------------------- Getters & setters & other methods---------------------
    public bool CanPlayCard(Card card)
    {
        var hand = userControlledplayer.GetHandScript();

        if (card.GetSuit() == Suit.Hearts && hand.GetHand().Count == hand.GetHand(Suit.Hearts).Count)
        {
            return true;
        }
        if (hand.GetHand().Count == 13 && GetTurn() == 1 && !card.IsCard(Suit.Clubs, 2))
        {
            Debug.Log("Must start with 2 of clubs");
            return false;
        }
        else if (GetTurn() != 1 && hand.HasSuit(GetWinningCard().GetSuit()) && card.GetSuit() != GetWinningCard().GetSuit())
        {
            Debug.Log("Must play " + GetWinningCard().GetSuit() + " if you have it");
            return false;
        }
        else if (GetTurn() != 1 && card.GetSuit() == Suit.Hearts && !hand.HasSuit(GetWinningCard().GetSuit()))
        {
            return true;
        }
        else if (card.GetSuit() == Suit.Hearts && !HasHeartsBroken())
        {
            Debug.Log("Hearts are not yet broken");
            return false;
        }

        return true;
    }

    public bool IsHeartsOnBoard()
    {
        foreach (var card in placedCardsList)
        {
            if (card.GetSuit() == Suit.Hearts)
            {
                return true;
            }
        }
        return false;
    }

    private void ClearDropAreas()
    {
        placedCardsList.Clear();
        placedCardsDict.Clear();
    }

    public void AddPlacedCard(Card card, Player player)
    {
        placedCardsDict[player] = card;
        placedCardsList.Add(card);
    }

    private void RemovePlayedCards()
    {
        foreach (Player player in players)
        {
            player.RemovePlayedCards();
        }
    }

    public Card GetStartCard()
    {
        if (first == null)
        {
            Debug.Log("First is null");
            return null;
        }
        return placedCardsDict[first];
    }

    public Card GetWinningCard()
    {
        var winningCard = GetStartCard();

        foreach (var placedCard in placedCardsList)
        {
            if (winningCard.GetSuit() == placedCard.GetSuit() && placedCard.GetRank() > winningCard.GetRank())
            {
                winningCard = placedCard;
            }
        }
        return winningCard;
    }

    public bool HasHeartsBroken()
    {
        return heartsBroken;
    }

    public void SetHeartsBroken()
    {
        heartsBroken = true;
    }

    public List<Card> GetPlacedCards()
    {
        return placedCardsList;
    }

    private Player GetWinner()
    {
        var winningCard = GetWinningCard();

        foreach (var player in players)
        {
            if (placedCardsDict[player] == winningCard)
            {
                return player;
            }
        }
        Debug.Log("No winner");
        return null;
    }
}
