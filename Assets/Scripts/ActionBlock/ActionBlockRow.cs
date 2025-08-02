using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ActionBlockRow : MonoBehaviour
{
    public static ActionBlockRow Instance { get; private set; }

    public RectTransform container; // HorizontalLayoutGroup
    public GameObject emptySlotPrefab; // null일 때 표시할 빈 슬롯
    public List<ActionBlock?> blocksInRow = new();

    private void Awake()
    {
        Instance = this;
        container = GetComponent<RectTransform>();
    }

    public void AddBlock(int columnIndex, ActionBlock? block)
    {
        // 슬롯 확보
        while (blocksInRow.Count <= columnIndex)
            blocksInRow.Add(null);

        if (block != null)
        {
            blocksInRow[columnIndex] = block;

            block.transform.SetParent(container);
            block.transform.localScale = Vector3.one;
            block.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            block.currentRow = -1;
            block.currentColumn = -1;
            block.currentGrid = null;
        }
        else
        {
            // null 슬롯이면 빈 오브젝트 프리팹으로 표시
            var empty = Instantiate(emptySlotPrefab, container);
            empty.transform.localScale = Vector3.one;
            blocksInRow[columnIndex] = null;
        }
    }
    public void ClearBlock()
    {
        foreach (ActionBlock block in blocksInRow)
        {
            if (block == null) continue;
            Destroy(block.gameObject);
        }
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
