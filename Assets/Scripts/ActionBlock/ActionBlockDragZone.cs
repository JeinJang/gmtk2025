using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionBlockDragZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image highlightImage;

    protected virtual void Awake()
    {
        highlightImage = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<ActionBlock>() != null)
        {
            FadeIn();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && eventData.pointerDrag.GetComponent<ActionBlock>() != null)
        {
            FadeOut();
        }
    }

    private void FadeIn()
    {
        if (highlightImage != null)
        {
            var color = highlightImage.color;
            color.a = 0.3f;
            highlightImage.color = color;
        }
    }

    protected void FadeOut()
    {
        if (highlightImage != null)
        {
            var color = highlightImage.color;
            color.a = 0f;
            highlightImage.color = color;
        }
    }
}