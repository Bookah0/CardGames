using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour, IPointerClickHandler
{
    private Transform handParent;
    private bool cardSetToSwitch = false;
    private Card cardScript;
    private Player playerScript;

    private void Start()
    {
        handParent = transform.parent;
        playerScript = handParent.parent.GetComponent<Player>();
        cardScript = gameObject.GetComponent<Card>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.state == GameState.RoundStart)
        {
            SetSwitchedCard();
        }
        else if (GameManager.Instance.state == GameState.PlayerTurn && EventManager.TryPlayCard(cardScript))
        {
            PlaceCardOnDropArea();
        }
    }

    public void SetSwitchedCard()
    {
        if (!cardSetToSwitch)
        {
            playerScript.AddCardToSwitch(cardScript);
            cardSetToSwitch = true;
        }
        else
        {
            playerScript.RemoveCardToSwitch(cardScript);
            cardSetToSwitch = false;
        }
    }

    private void PlaceCardOnDropArea()
    {
        transform.SetParent(playerScript.dropArea.transform);
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        playerScript.SetPlacedCard(cardScript);
        transform.position = playerScript.dropArea.transform.position;

        if (cardScript.GetSuit() == Suit.Hearts)
        {
            EventManager.HeartsPlayed();
        }

        EventManager.PassTurn();
        Destroy(gameObject.GetComponent<Draggable>());
        Destroy(gameObject.GetComponent<Clickable>());
    }
}
