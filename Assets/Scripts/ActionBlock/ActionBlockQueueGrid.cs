using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ActionBlockQueueGrid : ActionBlockDragZone, IDropHandler
{
    public int columns = 4;
    public int rows = 3;
    public Vector2 cellSize = new(120, 120);
    public Vector2 spacing = new(0, 0);

    public RectTransform gridArea; // 그리드가 렌더링되는 공간
    private GameObject[,] blockGrid;

    protected override void Awake()
    {
        base.Awake();
        blockGrid = new GameObject[columns, rows];
        gridArea = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        FadeOut();

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null || dropped.GetComponent<ActionBlock>() == null)
            return;

        int column = GetColumnFromPointer(eventData.position);
        if (column == -1)
            return;

        int targetRow = FindLowestEmptyRow(column);
        if (targetRow == -1)
        {
            Debug.Log("해당 column이 가득 찼습니다.");
            return;
        }

        blockGrid[column, targetRow] = dropped;

        var actionBlock = dropped.GetComponent<ActionBlock>();
        if (actionBlock != null)
        {
            actionBlock.currentColumn = column;
            actionBlock.currentRow = targetRow;
            actionBlock.currentGrid = this;
        }

        dropped.transform.SetParent(gridArea);
        var rt = dropped.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0);

        Vector2 pos = new Vector2(
            column * (cellSize.x + spacing.x),
            targetRow * (cellSize.y + spacing.y)
        );
        rt.anchoredPosition = pos;
    }

    private int GetColumnFromPointer(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridArea, screenPosition, null, out Vector2 localPos);

        Debug.Log(localPos);
        float totalWidth = columns * (cellSize.x + spacing.x);
        if (localPos.x < -1 * totalWidth / 2 || localPos.x > totalWidth / 2) return -1;

        int col = Mathf.FloorToInt((localPos.x + totalWidth / 2) / (cellSize.x + spacing.x));
        return Mathf.Clamp(col, 0, columns - 1);
    }

    private int FindLowestEmptyRow(int col)
    {
        for (int row = 0; row < rows; row++)
        {
            if (blockGrid[col, row] == null)
                return row;
        }
        return -1;
    }

    // Optional: 특정 행의 블록들 리턴
    public List<GameObject> GetBlocksInRow(int row)
    {
        var result = new List<GameObject>();
        for (int col = 0; col < columns; col++)
        {
            if (blockGrid[col, row] != null)
                result.Add(blockGrid[col, row]);
        }
        return result;
    }

    public void ClearBlockAt(int column, int row)
    {
        if (column >= 0 && column < columns && row >= 0 && row < rows)
        {
            if (blockGrid[column, row] != null)
            {
                blockGrid[column, row] = null;
            }
        }
    }

    public void CollapseColumn(int column)
    {
        for (int row = 0; row < rows - 1; row++)
        {
            if (blockGrid[column, row] == null)
            {
                // 위에 있는 블록을 한 칸 아래로 내림
                for (int upper = row + 1; upper < rows; upper++)
                {
                    if (blockGrid[column, upper] != null)
                    {
                        var block = blockGrid[column, upper];
                        blockGrid[column, row] = block;
                        blockGrid[column, upper] = null;

                        var rt = block.GetComponent<RectTransform>();
                        Vector2 newPos = new Vector2(
                            column * (cellSize.x + spacing.x),
                            row * (cellSize.y + spacing.y)
                        );
                        rt.DOAnchorPos(newPos, 0.25f).SetEase(Ease.OutCubic);

                        var ab = block.GetComponent<ActionBlock>();
                        ab.currentRow = row;

                        break;
                    }
                }
            }
        }
    }

}
