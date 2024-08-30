using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SortBy { Rank, Suit }

public class Hand : MonoBehaviour
{    
    private List<Card> hand = new();
    public GameObject cardPrefab;
    public GameObject dropArea;
    private readonly List<Suit> suitSortOrder = new() { Suit.Clubs, Suit.Diamonds, Suit.Spades, Suit.Hearts };
    public bool isPlayer = false;
    public float cardRotation;

    public Card AddNewCard(CardData cardData)
    {
        if (cardPrefab == null) {
            Debug.Log("Prefab is null");
            return null;
        }

        GameObject card = Instantiate(cardPrefab);
        var cardScript = card.GetComponent<Card>();
        cardScript.Initialize(cardData);
        hand.Add(cardScript);
        
        if (!isPlayer)
        {
            cardScript.TurnCard(false);
            Destroy(gameObject.GetComponent<Draggable>());
            Destroy(gameObject.GetComponent<Clickable>());
        }

        RotateCard(card, false);
        card.transform.SetParent(transform);
        SortHand(SortBy.Suit);

        return cardScript;
    }

    public void SortHand(SortBy sortBy)
    {
        switch (sortBy)
        {
            case SortBy.Rank:
                SortHandByRank();
                break;
            case SortBy.Suit:
                SortHandBySuit(suitSortOrder);
                break;
        }

        for (int i = 0; i < hand.Count; i++)
        {
            SwitchChildOrder(hand[i].transform, transform.GetChild(i));
        }
    }

    private void SortHandByRank()
    {
        hand.OrderBy(card => card.GetRank());
    }

    private void SortHandBySuit(List<Suit> suitOrder)
    {
        hand = hand.OrderBy(card => suitOrder.IndexOf(card.GetSuit())).ThenBy(card => card.GetRank()).ToList();
    }

    public void RemoveCardFromHand(Card card)
    {
        hand.Remove(card);
    }

    public void AddCardToHand(Card card)
    {
        hand.Add(card);
    }

    public void MoveCardToOtherHand(Card card, Hand handToMoveTo)
    {
        var cardData = new CardData(card.GetSuit(), card.GetRank());
        var newCard = handToMoveTo.AddNewCard(cardData);
        hand.Remove(card);
        Destroy(card.gameObject);

        handToMoveTo.SortHand(SortBy.Suit);
        
        if (handToMoveTo.isPlayer)
        {
            newCard.SetBorderColor(Color.red);
        }
    }

    public Card GetExtremeCard(bool findLowest, List<Card> cards, bool heartsAllowed)
    {
        if (cards.Count == 0)
        {
            return null;
        }

        cards = heartsAllowed ? cards : GetCardsWithoutSuit(cards, Suit.Hearts);

        int extremeRank = findLowest ? int.MaxValue : int.MinValue;

        foreach (var card in cards)
        {
            int cardRank = card.GetRank();

            if ((findLowest && cardRank < extremeRank) || (!findLowest && cardRank > extremeRank))
            {
                extremeRank = cardRank;
            }
        }

        var extremeCards = new List<Card>();
        foreach (var card in cards)
        {
            if (card.GetRank() == extremeRank)
            {
                extremeCards.Add(card);
            }
        }

        if (extremeCards.Count == 0)
        {
            Debug.Log("Couldn't find an extreme card in the list of cards");
            return null;
        }
        return extremeCards[UnityEngine.Random.Range(0, extremeCards.Count)];
    }

    public Card GetLowestCard(List<Card> cards, bool heartsAllowed)
    {
        return GetExtremeCard(true, cards, heartsAllowed);
    }

    public Card GetHighestCard(List<Card> cards, bool heartsAllowed)
    {
        return GetExtremeCard(false, cards, heartsAllowed);
    }

    public List<Card> GetCardsInRange(int min, int max, List<Card> cards)
    {
        var cardsInRange = new List<Card>();
        foreach (var card in cards)
        {
            if (card.GetRank() > min && card.GetRank() < max)
            {
                cardsInRange.Add(card);
            }
        }

        if (cardsInRange.Count == 0)
        {
            Debug.Log("Couldnt find cards in range");
        }
        return cardsInRange;
    }

    public Card GetCardInHand(Suit suit, int rank)
    {
        foreach (var card in hand)
        {
            if (card.GetSuit() == suit && card.GetRank() == rank)
            {
                return card;
            }
        }
        Debug.Log("Couldnt find card " + rank + " of " + suit.ToString());
        return null;
    }

    public bool HasSuit(Suit suit)
    {
        foreach (var card in hand)
        {
            if (card.GetSuit() == suit)
            {
                return true;
            }
        }
        return false;
    }

    private void RotateCard(GameObject card, bool hasBeenRotated)
    {
        if (!hasBeenRotated)
        {
            card.transform.Rotate(0f, 0f, cardRotation);
            return;
        }

        for (int i = 0; i < 5; i++)
        {
            card.transform.Rotate(0f, 0f, 90);
            if(cardRotation == card.transform.rotation.y)
            {
                return;
            }
        } 
    }

    public List<Card> GetHand()
    {
        return hand;
    }

    public List<Card> GetHand(Suit suit)
    {
        var cardsOfSuit = new List<Card>();

        foreach (var card in hand)
        {
            if (card.GetSuit() == suit)
            {
                cardsOfSuit.Add(card);
            }
        }
        
        if(cardsOfSuit.Count == 0)
        {
            Debug.Log("Doesnt have any cards of suit " + suit + " in hand.");
        }

        return cardsOfSuit;
    }

    public static List<Card> GetCardsWithoutSuit(List<Card> cards, Suit suit)
    {
        var cardsOfSuit = new List<Card>();

        foreach (var card in cards)
        {
            if (card.GetSuit() != suit)
            {
                cardsOfSuit.Add(card);
            }
        }

        if (cardsOfSuit.Count == 0)
        {
            Debug.Log("Only has cards of suit " + suit + " in hand.");
            return null;
        }

        return cardsOfSuit;
    }

    public static void SwitchChildOrder(Transform firstChild, Transform secondChild)
    {
        int ind1 = firstChild.GetSiblingIndex();
        int ind2 = secondChild.GetSiblingIndex();

        firstChild.SetSiblingIndex(ind2 + 1);
        secondChild.SetSiblingIndex(ind1);
    }
}
