using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionBlockInventory : ActionBlockDragZone, IDropHandler
{
    public static ActionBlockInventory Instance { get; private set; }

    [SerializeField] private InitialActionBlockListSO _initialActionList;

    private GameObject _blockPrefab;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;
        _blockPrefab = Resources.Load("Prefabs/ActionBlock/Action Block") as GameObject;

        if (_blockPrefab == null)
        {
            Debug.LogError("Prefab not found! Check path or file name.");
        }
        Reset();
    }

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

    public void Reset()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (ActionBlockSO data in _initialActionList.list)
        {
            GameObject obj = Instantiate(_blockPrefab);
            obj.transform.GetComponentInChildren<Image>().sprite = data.sprite;
            obj.GetComponent<ActionBlock>().actionType = data.type;

            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one;
        }
    }

    public void Rollback(List<GameObject> original)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var obj in original)
        {
            obj.transform.SetParent(transform);
            obj.transform.localScale = Vector3.one;
        }
    }
}