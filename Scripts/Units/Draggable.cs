using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private Transform parentToReturnTo = null;
    private Transform handParent;
    private bool canPlayCard = false;
    private Card cardScript;
    private Player playerScript;
    private Hand handScript;

    private void Start()
    {
        cardScript = gameObject.GetComponent<Card>();
        handParent = transform.parent;
        playerScript = handParent.parent.GetComponent<Player>();
        handScript = handParent.GetComponent<Hand>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if(GameManager.Instance.state == GameState.PlayerTurn)
        {
            parentToReturnTo = handParent;
            transform.SetParent(transform.parent);
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.state == GameState.PlayerTurn)
        {
            transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (GameManager.Instance.state == GameState.PlayerTurn)
        {
            transform.SetParent(parentToReturnTo);
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            if (parentToReturnTo != handParent)
            {
                playerScript.SetPlacedCard(cardScript);
                transform.position = parentToReturnTo.position;

                if (cardScript.GetSuit() == Suit.Hearts)
                {
                    EventManager.HeartsPlayed();
                }

                EventManager.PassTurn();
                Destroy(gameObject.GetComponent<Draggable>());
                Destroy(gameObject.GetComponent<Clickable>());
            }
            else
            {
                handScript.SortHand(SortBy.Suit);
            }
        }
    }

    public bool CanPlayCard()
    {
        return canPlayCard;
    }

    public void SetReturnParent(Transform t)
    {
        parentToReturnTo = t;
    }
}
