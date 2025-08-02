
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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

    private void Update()
    {
        // 디버깅용 : 턴 진행 
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartConsumingBlocks();
        }
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

    public void StartConsumingBlocks(float delay = 0.25f)
    {
        StartCoroutine(ConsumeBlocksSequentially(delay));
    }

    public IEnumerator ConsumeBlocksSequentially(float delay)
    {
        for (int i = 0; i < blocksInRow.Count; i++)
        {
            var block = blocksInRow[i];
            if (block != null)
            {
                // 효과 
                var tf = block.transform;
                var rt = tf as RectTransform;
                if (rt != null)
                {
                    rt.pivot = new Vector2(0.5f, 0.5f);
                }
                Sequence seq = DOTween.Sequence();
                seq.Append(tf.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack));
                seq.Append(tf.DOScale(0.0f, 0.05f).SetEase(Ease.InBack));

                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(delay);
        }

        blocksInRow.Clear(); // 내부 리스트도 정리
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
