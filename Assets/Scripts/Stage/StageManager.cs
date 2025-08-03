using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [SerializeField] private VoidEventChannelSO _gameClearEvent;

    public List<GameObject> stageFolders;
    public GameObject playerPrefab; // 인스펙터에 플레이어 프리팹 할당

    private GameObject currentPlayer; // 현재 스폰된 플레이어 오브젝트
    private int currentStageIndex = 0;

    public GameObject player;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        LoadStage(0);
    }

    public void LoadStage(int stageIdx)
    {
        // 1. GridManager 전체 셀 클리어 (강제 초기화)
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OccupiedGridCells.Clear();
            if (GridManager.Instance.LastPositions != null)
                GridManager.Instance.LastPositions.Clear();

            if (GridManager.Instance.NextPositions != null)
                GridManager.Instance.NextPositions.Clear();
        }

        // 2. 기존 플레이어 제거
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        // 3. 스테이지 폴더 활성화/비활성화
        for (int i = 0; i < stageFolders.Count; i++)
        {
            stageFolders[i].SetActive(i == stageIdx);
        }
        currentStageIndex = stageIdx;

        // 4. SpawnPoint 위치에 새 플레이어 생성
        SetPlayer(stageIdx);
    }

    public void SetPlayer(int stageIdx)
    {
        Transform spawn = stageFolders[stageIdx].transform.Find("SpawnPoint");
        if (spawn != null && playerPrefab != null)
        {
            currentPlayer = Instantiate(playerPrefab, spawn.position, Quaternion.identity);

            // 한 프레임 대기 후 GridManager 위치 등록
            StartCoroutine(InitializePlayerGridPosition());
        }
    }

    public void ResetPlayer()
    {
        SetPlayer(currentStageIndex);
        if (GridManager.Instance != null)
        {
            GridManager.Instance.OccupiedGridCells.Clear();
            if (GridManager.Instance.LastPositions != null)
                GridManager.Instance.LastPositions.Clear();

            if (GridManager.Instance.NextPositions != null)
                GridManager.Instance.NextPositions.Clear();
        }
    }

    public void NextStage()
    {
        int nextIdx = (currentStageIndex + 1) % stageFolders.Count;
        LoadStage(nextIdx);
    }

    // Goal에 닿으면 이 함수를 호출
    public void OnStageClear()
    {
        _gameClearEvent.RaiseEvent();
        NextStage();
    }

    private IEnumerator InitializePlayerGridPosition()
    {
        yield return null; // 한 프레임 대기

        var charGridMovement = currentPlayer.GetComponent<CharacterGridMovement>();
        if (charGridMovement != null)
        {
            charGridMovement.SetCurrentWorldPositionAsNewPosition();
        }
    }
}