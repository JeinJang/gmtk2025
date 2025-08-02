using UnityEngine;
using UnityEngine.EventSystems;

public class ActionBlockInventory : ActionBlockDragZone, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        FadeOut();

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null || dropped.GetComponent<ActionBlock>() == null)
            return;

        // 부모를 Grid 영역으로 변경
        dropped.transform.SetParent(transform);
        dropped.transform.localScale = Vector3.one;
    }
}