using System.Linq;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private GameManager gameManager;
    private readonly int minRank = 1;
    private readonly int maxRank = 15;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void PlayCard(Player opponent)
    {
        var hand = opponent.GetHandScript();
        var heartsOnBoard = gameManager.IsHeartsOnBoard();
        var turn = gameManager.GetTurn();
        Card cardToPlay;

        if (turn == 1 && hand.GetCardInHand(Suit.Clubs, 2) != null)
        {
            cardToPlay = hand.GetCardInHand(Suit.Clubs, 2);
        }
        else if(turn == 1)
        {
            cardToPlay = hand.GetLowestCard(hand.GetHand(), gameManager.HasHeartsBroken());
        }
        else if(turn == 4 && hand.HasSuit(gameManager.GetWinningCard().GetSuit()) && !heartsOnBoard)
        {
            cardToPlay = GetCardToPlay(hand, true);
        }
        else
        {
            cardToPlay = GetCardToPlay(hand, false);
        }
        
        if (cardToPlay.GetSuit() == Suit.Hearts)
        {
            EventManager.HeartsPlayed();
        }

        cardToPlay.PlaceCard();
        opponent.SetPlacedCard(cardToPlay);
    }

    private Card GetCardToPlay(Hand hand, bool preferesToWin)
    {
        Card cardToPlay = null;
        var winningCard = gameManager.GetWinningCard();
        var winningRank = winningCard.GetRank();
        var winningSuit = winningCard.GetSuit();
        var hasWinningSuit = hand.HasSuit(winningSuit);
        var heartsAllowed = IsHeartsAllowed(hand, winningSuit);


        if ((winningSuit == Suit.Spades && winningRank > 12 || !hasWinningSuit) && hand.GetCardInHand(Suit.Spades, 12) != null)
        {
            cardToPlay = hand.GetCardInHand(Suit.Spades, 12);
        }
        else if (hasWinningSuit && !preferesToWin)
        {
            var playableCards = hand.GetCardsInRange(minRank, winningRank, hand.GetHand(winningSuit));
            cardToPlay = hand.GetHighestCard(playableCards, heartsAllowed);

            if (cardToPlay == null)
            {
                playableCards = hand.GetCardsInRange(winningRank, maxRank, hand.GetHand(winningSuit));
                cardToPlay = hand.GetLowestCard(playableCards, heartsAllowed);
            }
        }
        else if (hasWinningSuit && preferesToWin)
        {
            cardToPlay = hand.GetHighestCard(hand.GetHand(winningSuit), heartsAllowed);
        }
        else if (!hasWinningSuit)
        {
            cardToPlay = hand.GetHighestCard(hand.GetHand(), heartsAllowed);
        }
        return cardToPlay;
    }

    private bool IsHeartsAllowed(Hand hand, Suit winningSuit)
    {
        if (gameManager.HasHeartsBroken())
        {
            return true;
        }
        else if (hand.GetHand(Suit.Hearts).Count == hand.GetHand().Count)
        {
            return true;
        }
        else if (gameManager.GetTurn() != 1 && !hand.HasSuit(winningSuit))
        {
            return true;
        }
        return false;
    }

    public void ChooseCardsToSwitch(Player opponent)
    {
        var hand = opponent.GetHandScript();
        opponent.SetCardsToSwitch(hand.GetHand().OrderByDescending(card => card.GetRank()).Take(3).ToList());
    }
}
