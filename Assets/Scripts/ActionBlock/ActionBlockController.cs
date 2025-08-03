using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBlockController : MonoBehaviour
{
    public static ActionBlockController Instance { get; private set; }

    [SerializeField] private VoidEventChannelSO _gameStartEvent;
    [SerializeField] private VoidEventChannelSO _gameFailedEvent;
    [SerializeField] private VoidEventChannelSO _turnEndEvent;

    private ActionBlockInventory _inventory;
    private ActionBlockQueueGrid _queueGrid;
    private List<GameObject> _originalInventory;
    private GameObject[,] _originalBlockGrid;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        _gameStartEvent.OnEventRaised += OnGameStart;
        _gameFailedEvent.OnEventRaised += OnGameFailed;
        _turnEndEvent.OnEventRaised += OnTurnEnd;
    }

    private void OnDisable()
    {
        _gameStartEvent.OnEventRaised -= OnGameStart;
        _gameFailedEvent.OnEventRaised -= OnGameFailed;
        _turnEndEvent.OnEventRaised -= OnTurnEnd;
    }

    private void OnGameStart()
    {
        Debug.Log("게임 시작");

        List<Transform> blocks = new();
        foreach (Transform child in ActionBlockInventory.Instance.transform)
        {
            blocks.Add(child);
        }
        _originalInventory = DeepCopyBlockList(blocks);
        _originalBlockGrid = DeepCopyBlockGrid(ActionBlockQueueGrid.Instance.blockGrid);
        StartCoroutine(SetNewRow());
    }

    private void OnGameFailed()
    {
        Debug.Log("게임 실패");
        ActionBlockInventory.Instance.Rollback(_originalInventory);
        ActionBlockQueueGrid.Instance.Reset(_originalBlockGrid);
        ActionBlockRow.Instance.ClearBlock();
    }

    private void OnTurnEnd()
    {
        //ActionBlockRow.Instance;
        if (ActionBlockRow.Instance.blocksInRow.Count > 0)
        {
            StartCoroutine(ActionBlockRow.Instance.ConsumeBlock());
        }
        else
        {
            if (!ActionBlockQueueGrid.Instance.CheckIsEmpty())
            {
                StartCoroutine(SetNewRow());
            }
        }
    }

    private IEnumerator SetNewRow()
    {
        ActionBlockQueueGrid.Instance.DispatchBottomRowToActionRow();
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(ActionBlockRow.Instance.ConsumeBlock());
    }

    private GameObject[,] DeepCopyBlockGrid(GameObject[,] original)
    {
        int cols = original.GetLength(0);
        int rows = original.GetLength(1);
        GameObject[,] copy = new GameObject[cols, rows];

        Transform hiddenParent = new GameObject("Hidden Grid Backup").transform;
        hiddenParent.gameObject.SetActive(false);

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject originalObj = original[x, y];

                if (originalObj != null)
                {
                    GameObject clone = Instantiate(originalObj);
                    clone.name = originalObj.name;
                    clone.SetActive(originalObj.activeSelf);
                    clone.transform.SetParent(hiddenParent);

                    copy[x, y] = clone;
                }
                else
                {
                    copy[x, y] = null;
                }
            }
        }

        return copy;
    }

    private List<GameObject> DeepCopyBlockList(List<Transform> original)
    {
        int cols = original.Count;
        List<GameObject> copy = new();

        Transform hiddenParent = new GameObject("Hidden List Backup").transform;
        hiddenParent.gameObject.SetActive(false);

        for (int x = 0; x < cols; x++)
        {
            GameObject originalObj = original[x].gameObject;

            if (originalObj != null)
            {
                GameObject clone = Instantiate(originalObj);
                clone.name = originalObj.name;
                clone.SetActive(originalObj.activeSelf);
                clone.transform.SetParent(hiddenParent);

                copy.Add(clone);
            }
            else
            {
                copy.Add(null);
            }
        }
        return copy;
    }
}
