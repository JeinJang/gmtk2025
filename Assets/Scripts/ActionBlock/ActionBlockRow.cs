using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ActionBlockRow : MonoBehaviour
{
    public static ActionBlockRow Instance { get; private set; }

    public RectTransform container;
    public GameObject emptySlotPrefab;
    public float cellWidth = 120f;
    public float spacing = 0f;

    public List<ActionBlock?> blocksInRow = new();

    private void Awake()
    {
        Instance = this;
        container = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartConsumingBlocks();
        }
    }

    public void AddBlock(int columnIndex, ActionBlock? block)
    {
        while (blocksInRow.Count <= columnIndex)
            blocksInRow.Add(null);

        if (block != null)
        {
            blocksInRow[columnIndex] = block;

            var tf = block.transform;
            tf.SetParent(container);
            tf.localScale = Vector3.one;

            var rt = block.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.125f, 0.5f);
            rt.anchorMax = new Vector2(0.125f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // 수동 위치 계산
            rt.anchoredPosition = GetAnchoredPosition(columnIndex);
        }
        else
        {
            var empty = Instantiate(emptySlotPrefab, container);
            var rt = empty.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;

            rt.anchorMin = new Vector2(0.125f, 0.5f);
            rt.anchorMax = new Vector2(0.125f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            rt.anchoredPosition = GetAnchoredPosition(columnIndex);

            blocksInRow[columnIndex] = null;
        }
    }

    private Vector2 GetAnchoredPosition(int index)
    {
        return new Vector2(index * (cellWidth + spacing), 0);
    }

    public void StartConsumingBlocks(float delay = 0.5f)
    {
        StartCoroutine(ConsumeBlocksSequentially(delay));
    }

    public IEnumerator ConsumeBlocksSequentially(float delay)
    {
        while (blocksInRow.Count > 0)
        {
            var block = blocksInRow[0];
            blocksInRow.RemoveAt(0); // 항상 첫 번째 요소 제거

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
                yield return seq.WaitForCompletion();

                Destroy(block.gameObject);
            }
            else
            {
                yield return new WaitForSeconds(0.25f);
            }

            // 나머지 블록들 위치 이동
            for (int i = 0; i < blocksInRow.Count; i++)
            {
                if (blocksInRow[i] != null)
                {
                    var rt = blocksInRow[i].GetComponent<RectTransform>();
                    rt.DOAnchorPos(GetAnchoredPosition(i), 0.25f).SetEase(Ease.OutCubic);
                }
            }

            yield return new WaitForSeconds(delay);
        }
    }

    public void ClearBlock()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        blocksInRow.Clear();
    }
}
