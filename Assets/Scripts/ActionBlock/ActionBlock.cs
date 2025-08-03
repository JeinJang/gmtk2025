using UnityEngine;
using UnityEngine.EventSystems;

public class ActionBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ActionType actionType;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    public int currentColumn = -1;
    public int currentRow = -1;
    public ActionBlockQueueGrid currentGrid = null;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (ActionBlockController.Instance.isGamePlaying) return;
        if (currentGrid != null && currentColumn >= 0 && currentRow >= 0)
        {
            currentGrid.ClearBlockAt(currentColumn, currentRow);
            currentGrid.CollapseColumn(currentColumn);
            currentColumn = -1;
            currentRow = -1;
            currentGrid = null;
        }

        originalParent = transform.parent;
        transform.SetParent(canvas.transform); // 전역 드래그
        canvasGroup.blocksRaycasts = false; // 드롭 영역 인식 가능하게
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ActionBlockController.Instance.isGamePlaying) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ActionBlockController.Instance.isGamePlaying) return;
        canvasGroup.blocksRaycasts = true;
        // Drop되지 않았다면 원래 자리로 복귀
        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
        }
    }
}
