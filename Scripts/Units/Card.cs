using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Suit { Spades, Hearts, Clubs, Diamonds }

public class Card : MonoBehaviour
{
    public Sprite spadesSprite;
    public Sprite heartsSprite;
    public Sprite diamondsSprite;
    public Sprite clubsSprite;
    public Sprite cardbackSprite;
    private TextMeshProUGUI rankText;
    private Image suitImage;
    private Image background;
    private CardData cardData;
    private bool lerpingCard;
    private float accum = 0.0f;
    private Vector2 p1, p2;
    private Clickable clickableScript;
    private Draggable draggableScript;


    void Update()
    {
        if (lerpingCard)
        {
            accum += 1.5f * Time.deltaTime;
            transform.position = Vector2.Lerp(p1, p2, accum);
        }
    }

    public void Initialize(CardData data)
    {
        cardData = data;
        rankText = transform.Find("Rank").GetComponent<TextMeshProUGUI>();
        background = transform.Find("Background").GetComponent<Image>();
        suitImage = transform.Find("SuitImage").GetComponent<Image>();
        
        draggableScript = gameObject.GetComponent<Draggable>();
        clickableScript = gameObject.GetComponent<Clickable>();

        if (draggableScript != null)
        {
            gameObject.GetComponent<Draggable>().enabled = false;
        }

        SetRankText();
        SetSuitImage();
    }

    private void SetRankText()
    {
        rankText.text = GetRank() switch
        {
            11 => "J",
            12 => "Q",
            13 => "K",
            14 => "A",
            _ => GetRank().ToString(),
        };
    }

    private void SetSuitImage()
    {
        rankText.color = (GetSuit() == Suit.Spades || GetSuit() == Suit.Clubs) ? Color.black : Color.red;

        suitImage.sprite = GetSuit() switch
        {
            Suit.Spades => spadesSprite,
            Suit.Clubs => clubsSprite,
            Suit.Diamonds => diamondsSprite,
            Suit.Hearts => heartsSprite,
            _ => throw new System.NotImplementedException()
        };
    }

    public void TurnCard(bool toFront)
    {
        if (toFront)
        {
            background.sprite = null;
            background.transform.SetSiblingIndex(0);
            rankText.transform.SetSiblingIndex(1);
            suitImage.transform.SetSiblingIndex(2);
        }
        else
        {
            background.sprite = cardbackSprite;
            rankText.transform.SetSiblingIndex(0);
            suitImage.transform.SetSiblingIndex(1);
            background.transform.SetSiblingIndex(2);
        }
    }

    public Suit GetSuit()
    {
        return cardData.GetSuit();
    }

    public int GetRank()
    {
        return cardData.GetRank();
    }

    public bool IsCard(Suit suit, int rank)
    {
        return cardData.IsCard(suit, rank);
    }

    private void OnMouseDown()
    {
        PlaceCard();
    }

    public void PlaceCard()
    {
        var dropArea = transform.parent.GetComponent<Hand>().dropArea;

        TurnCard(true);
        lerpingCard = true;
        p1 = transform.position;
        p2 = dropArea.transform.position;
        transform.SetParent(dropArea.transform);
    }

    public void SetLerpPositions(Vector2 p1, Vector2 p2)
    {
        lerpingCard = true;
        this.p1 = p1;
        this.p2 = p2;
    }

    public void SetLerping(bool b)
    {
        lerpingCard = b;
        accum = 0.0f;
    }

    public void SetBorderColor(Color color)
    {
        gameObject.GetComponent<Image>().color = color;
    }

    public Clickable GetClickableScript()
    {
        return clickableScript;
    }
}

public struct CardData
{
    private Suit suit;
    private int rank;

    public CardData(Suit suit, int rank)
    {
        this.rank = rank;
        this.suit = suit;
    }

    public Suit GetSuit()
    {
        return suit;
    }

    public int GetRank()
    {
        return rank;
    }

    public bool IsCard(Suit suit, int rank)
    {
        return this.suit == suit && this.rank == rank;
    }
}