using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    public Card cardOnDropArea;

    public void OnDrop(PointerEventData eventData)
    {
        var draggable = eventData.pointerDrag.GetComponent<Draggable>();
        var card = eventData.pointerDrag.GetComponent<Card>();

        if (draggable == null)
        {
            return;
        } 
        else if (EventManager.TryPlayCard(card))
        {
            draggable.SetReturnParent(transform);
        }
    }
}
