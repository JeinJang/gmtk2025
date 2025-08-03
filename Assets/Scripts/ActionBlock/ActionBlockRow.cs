using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ActionBlockRow : MonoBehaviour
{
    [SerializeField] private ActionEventChannelSO _turnStartEvent;
    public static ActionBlockRow Instance { get; private set; }

    public RectTransform container;
    public GameObject emptySlotPrefab;
    public float cellWidth = 64f;
    public float spacing = 8f;

    public List<GameObject> blocksInRow = new();

    private void Awake()
    {
        Instance = this;
        container = GetComponent<RectTransform>();
    }

    public void AddBlock(int columnIndex, ActionBlock? block)
    {
        while (blocksInRow.Count <= columnIndex)
            blocksInRow.Add(null);

        if (block != null)
        {
            blocksInRow[columnIndex] = block.gameObject;

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

            blocksInRow[columnIndex] = empty;
        }
    }

    private Vector2 GetAnchoredPosition(int index)
    {
        return new Vector2(index * (cellWidth + spacing), 0);
    }

    public IEnumerator ConsumeBlock()
    {
        var block = blocksInRow[0];
        var actionComponent = block.GetComponent<ActionBlock>();
        blocksInRow.RemoveAt(0); // 항상 첫 번째 요소 제거

        if (actionComponent != null)
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

            _turnStartEvent.RaiseEvent(block.GetComponent<ActionBlock>().actionType);
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
            _turnStartEvent.RaiseEvent(null);
        }

        Destroy(block);

        // 나머지 블록들 위치 이동
        for (int i = 0; i < blocksInRow.Count; i++)
        {
            var item = blocksInRow[i];

            if (item != null)
            {
                var rt = item.GetComponent<RectTransform>();
                rt.DOAnchorPos(GetAnchoredPosition(i), 0.25f).SetEase(Ease.OutCubic);
            }
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
