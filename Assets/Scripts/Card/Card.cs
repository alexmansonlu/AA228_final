using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool isPlayable = false; // Determines if this card can currently be played

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 originalPosition;
    public CardData cardData;
    public Player owner;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition;
    }

    // Called when dragging starts
    public void OnBeginDrag(PointerEventData eventData)
    {   
        originalPosition = rectTransform.anchoredPosition;
        if (isPlayable)
        {
            canvasGroup.alpha = 0.6f; // Make the card semi-transparent while dragging
            canvasGroup.blocksRaycasts = false; // Disable raycasts to allow the drag
            
        }
    }

    // Called while dragging the card
    public void OnDrag(PointerEventData eventData)
    {
        if (isPlayable)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    // Called when dragging ends
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // Restore original transparency
        canvasGroup.blocksRaycasts = true; // Re-enable raycasts

        if (IsInPlayZone(eventData))
        {
            GameManager.Instance.PlayCard(this); // Attempt to play the card
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition; // Return to original position if not in play zone
        }
    }

    // Checks if the card was dropped in the play zone
    private bool IsInPlayZone(PointerEventData eventData)
    {
        return eventData.pointerEnter != null && eventData.pointerEnter.GetComponent<DropZone>() != null;
    }
}
