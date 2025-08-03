using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

// 가시 데이터 구조체
[System.Serializable]
public struct SpikeData
{
    public Vector2Int position;
    public bool[] pattern;
    
    public SpikeData(Vector2Int pos, bool[] pat)
    {
        position = pos;
        pattern = pat;
    }
}

// 타일 타입 정의
public enum TileType
{
    Empty = 0,
    Wall = 1,
    Start = 2,
    Goal = 3,
    Spike = 4
}

// 플레이어 방향
public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

// 명령어 타입
public enum Command
{
    Forward = 'W',    // 위로 이동
    MoveLeft = 'A',   // 왼쪽으로 이동
    MoveRight = 'D',  // 오른쪽으로 이동
    MoveDown = 'S',   // 아래로 이동
    Wait = '_'        // 대기
}

// 메인 게임 매니저
public class LevelDesigner : MonoBehaviour
{
    [Header("맵 설정")]
    public int mapWidth = 10;
    public int mapHeight = 10;
    public float tileSize = 1.0f;
    
    [Header("기본 레벨 설정")]
    public Vector2Int defaultStartPos = new Vector2Int(1, 1);
    public Vector2Int defaultGoalPos = new Vector2Int(8, 8);
    public SpikeData[] defaultSpikes = new SpikeData[]
    {
        new SpikeData(new Vector2Int(3, 3), new bool[] { true, false, true, false, true }),
        new SpikeData(new Vector2Int(5, 5), new bool[] { false, true, false, true, false })
    };
    
    [Header("타일 프리팹들")]
    public GameObject emptyTilePrefab;
    public GameObject wallTilePrefab;
    public GameObject startTilePrefab;
    public GameObject goalTilePrefab;
    public GameObject spikeTilePrefab;
    public GameObject playerPrefab;
    
    [Header("게임 상태")]
    public TileType[,] levelData;
    public bool[,] spikePatterns; // [x,y] 위치의 가시가 현재 턴에 활성화되는지
    public Dictionary<Vector2Int, List<bool>> spikeTimings; // 각 가시의 턴별 패턴
    
    [Header("플레이어")]
    public Vector2Int playerPos;
    public Direction playerDirection = Direction.North;
    public GameObject playerObject;
    
    [Header("게임 플레이")]
    public string commandSequence = "WWAD_"; // W:위로, A:왼쪽으로, D:오른쪽으로, S:아래로, _:대기
    public int currentTurn = 0;
    public bool isSimulating = false;
    public bool gameWon = false;
    public bool gameLost = false;
    
    [Header("에디터 모드")]
    public bool editorMode = true;
    public TileType currentBrush = TileType.Wall;
    
    private GameObject[,] tileObjects;
    private Camera editorCamera;
    
    void Start()
    {
        InitializeGame();
    }
    
    public void InitializeGame()
    {
        levelData = new TileType[mapWidth, mapHeight];
        spikePatterns = new bool[mapWidth, mapHeight];
        spikeTimings = new Dictionary<Vector2Int, List<bool>>();
        tileObjects = new GameObject[mapWidth, mapHeight];
        
        // 카메라 설정
        editorCamera = Camera.main;
        if (editorCamera != null)
        {
            editorCamera.transform.position = new Vector3(mapWidth * tileSize / 2, mapWidth * tileSize / 2, -10);
            editorCamera.orthographic = true;
            editorCamera.orthographicSize = Mathf.Max(mapWidth, mapHeight) * tileSize / 2 + 2;
        }
        
        // 기본 맵 생성
        CreateDefaultLevel();
        RefreshVisuals();
    }
    
    void CreateDefaultLevel()
    {
        // 테두리를 벽으로 설정
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    levelData[x, y] = TileType.Wall;
                }
                else
                {
                    levelData[x, y] = TileType.Empty;
                }
            }
        }
        
        // 인스펙터에서 설정한 시작점과 목표점 설정
        if (IsValidPosition(defaultStartPos))
        {
            levelData[defaultStartPos.x, defaultStartPos.y] = TileType.Start;
            playerPos = defaultStartPos;
        }
        else
        {
            // 유효하지 않은 위치라면 기본값 사용
            levelData[1, 1] = TileType.Start;
            playerPos = new Vector2Int(1, 1);
            Debug.LogWarning($"시작점 위치 ({defaultStartPos.x}, {defaultStartPos.y})가 유효하지 않습니다. 기본값 (1,1)을 사용합니다.");
        }
        
        if (IsValidPosition(defaultGoalPos))
        {
            levelData[defaultGoalPos.x, defaultGoalPos.y] = TileType.Goal;
        }
        else
        {
            levelData[mapWidth - 2, mapHeight - 2] = TileType.Goal;
            Debug.LogWarning($"목표점 위치 ({defaultGoalPos.x}, {defaultGoalPos.y})가 유효하지 않습니다. 기본값을 사용합니다.");
        }
        
        // 인스펙터에서 설정한 가시들 추가
        foreach (SpikeData spike in defaultSpikes)
        {
            if (IsValidPosition(spike.position) && spike.pattern != null && spike.pattern.Length > 0)
            {
                levelData[spike.position.x, spike.position.y] = TileType.Spike;
                spikeTimings[spike.position] = new List<bool>(spike.pattern);
            }
            else
            {
                Debug.LogWarning($"가시 위치 ({spike.position.x}, {spike.position.y}) 또는 패턴이 유효하지 않습니다.");
            }
        }
    }
    
    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x > 0 && pos.x < mapWidth - 1 && pos.y > 0 && pos.y < mapHeight - 1;
    }
    
    void Update()
    {
        if (editorMode)
        {
            HandleEditorInput();
        }
        else
        {
            HandleGameInput();
        }
    }
    
    void HandleEditorInput()
    {
        // 브러시 변경 (숫자 키)
        if (Input.GetKeyDown(KeyCode.Alpha1)) currentBrush = TileType.Empty;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentBrush = TileType.Wall;
        if (Input.GetKeyDown(KeyCode.Alpha3)) currentBrush = TileType.Start;
        if (Input.GetKeyDown(KeyCode.Alpha4)) currentBrush = TileType.Goal;
        if (Input.GetKeyDown(KeyCode.Alpha5)) currentBrush = TileType.Spike;
        
        // 마우스로 타일 편집
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Vector3 mousePos = editorCamera.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.FloorToInt(mousePos.x / tileSize);
            int y = Mathf.FloorToInt(mousePos.y / tileSize);
            
            if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
            {
                if (Input.GetMouseButton(0)) // 좌클릭: 그리기
                {
                    SetTile(x, y, currentBrush);
                }
                else if (Input.GetMouseButton(1)) // 우클릭: 지우기
                {
                    SetTile(x, y, TileType.Empty);
                }
            }
        }
        
        // 시뮬레이션 모드 전환
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSimulation();
        }
    }
    
    void HandleGameInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSimulating)
        {
            StartCoroutine(ExecuteNextCommand());
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSimulation();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            editorMode = true;
            ResetSimulation();
        }
    }
    
    void SetTile(int x, int y, TileType tileType)
    {
        // 이전 시작점 제거
        if (tileType == TileType.Start)
        {
            for (int i = 0; i < mapWidth; i++)
                for (int j = 0; j < mapHeight; j++)
                    if (levelData[i, j] == TileType.Start)
                        levelData[i, j] = TileType.Empty;
            playerPos = new Vector2Int(x, y);
        }
        
        levelData[x, y] = tileType;
        
        // 가시인 경우 기본 패턴 생성
        if (tileType == TileType.Spike)
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (!spikeTimings.ContainsKey(pos))
            {
                spikeTimings[pos] = new List<bool> { true, false, true, false, true };
            }
        }
        else
        {
            Vector2Int pos = new Vector2Int(x, y);
            if (spikeTimings.ContainsKey(pos))
            {
                spikeTimings.Remove(pos);
            }
        }
        
        RefreshVisuals();
    }
    
    void RefreshVisuals()
    {
        // 기존 타일 오브젝트들 제거
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (tileObjects[x, y] != null)
                {
                    DestroyImmediate(tileObjects[x, y]);
                }
            }
        }
        
        // 새 타일 오브젝트들 생성
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, y * tileSize, 0);
                GameObject prefab = GetTilePrefab(levelData[x, y]);
                
                if (prefab != null)
                {
                    tileObjects[x, y] = Instantiate(prefab, pos, Quaternion.identity);
                    tileObjects[x, y].name = $"Tile_{x}_{y}_{levelData[x, y]}";
                }
            }
        }
        
        // 플레이어 오브젝트 업데이트
        UpdatePlayerVisual();
        
        // 에디터 모드에서는 턴 0 기준으로 가시 상태 표시
        if (editorMode)
        {
            int originalTurn = currentTurn;
            currentTurn = 0; // 임시로 0턴으로 설정
            UpdateSpikeVisuals();
            currentTurn = originalTurn; // 원래 턴으로 복구
        }
        else
        {
            // 게임 모드에서는 현재 턴 기준
            UpdateSpikeVisuals();
        }
    }
    
    GameObject GetTilePrefab(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Empty: return emptyTilePrefab;
            case TileType.Wall: return wallTilePrefab;
            case TileType.Start: return startTilePrefab;
            case TileType.Goal: return goalTilePrefab;
            case TileType.Spike: return spikeTilePrefab;
            default: return emptyTilePrefab;
        }
    }
    
    void UpdatePlayerVisual()
    {
        if (playerObject == null && playerPrefab != null)
        {
            playerObject = Instantiate(playerPrefab);
        }
        
        if (playerObject != null)
        {
            Vector3 pos = new Vector3(playerPos.x * tileSize, playerPos.y * tileSize, -0.1f);
            playerObject.transform.position = pos;
            
            // 플레이어 방향 설정
            float angle = (int)playerDirection * 90f;
            playerObject.transform.rotation = Quaternion.Euler(0, 0, -angle);
            
            playerObject.SetActive(!editorMode);
        }
    }
    
    void UpdateSpikeVisuals()
    {
        foreach (var kvp in spikeTimings)
        {
            Vector2Int pos = kvp.Key;
            List<bool> pattern = kvp.Value;
            
            if (tileObjects[pos.x, pos.y] != null)
            {
                bool isActive = (currentTurn < pattern.Count) ? pattern[currentTurn] : false;
                
                // 가시 활성화 상태에 따라 색상 변경
                Renderer renderer = tileObjects[pos.x, pos.y].GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    Material mat = renderer.material;
                    Color targetColor = isActive ? Color.red : Color.gray;
                    
                    // URP Base Color 설정
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", targetColor);
                    }
                    // Built-in RP Color 설정
                    else if (mat.HasProperty("_Color"))
                    {
                        mat.SetColor("_Color", targetColor);
                    }
                }
            }
        }
    }
    
    void StartSimulation()
    {
        editorMode = false;
        isSimulating = false;
        currentTurn = 0;
        gameWon = false;
        gameLost = false;
        
        // 시작점 찾기
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (levelData[x, y] == TileType.Start)
                {
                    playerPos = new Vector2Int(x, y);
                    break;
                }
            }
        }
        
        playerDirection = Direction.North;
        
        // 0턴(시작 턴)의 가시 상태를 먼저 설정
        UpdateSpikeVisuals();
        UpdatePlayerVisual();
        
        Debug.Log($"<color=lime>시뮬레이션 시작! 명령어: {commandSequence}</color>");
        Debug.Log("<color=white>스페이스바를 눌러 다음 명령어를 실행하세요.</color>");
    }
    
    System.Collections.IEnumerator ExecuteNextCommand()
    {
        if (gameWon || gameLost) yield break;
        
        isSimulating = true;
        
        if (currentTurn < commandSequence.Length)
        {
            char cmd = commandSequence[currentTurn];
            Debug.Log($"<color=cyan>턴 {currentTurn + 1}: 명령어 '{cmd}' 실행</color>");
            
            // 먼저 가시 상태를 현재 턴에 맞게 업데이트
            UpdateSpikeVisuals();
            
            // 명령어 실행 (플레이어 이동)
            ExecuteCommand((Command)cmd);
            
            yield return new WaitForSeconds(0.5f);
            
            UpdatePlayerVisual();
            
            // 이동 후 게임 상태 체크 (현재 턴 기준)
            CheckGameState();
            
            // 턴 증가
            currentTurn++;
        }
        else
        {
            Debug.Log("<color=yellow>모든 명령어 실행 완료!</color>");
            
            // 모든 명령어 실행 후 목표에 도달하지 못한 경우
            if (!gameWon && !gameLost)
            {
                if (levelData[playerPos.x, playerPos.y] != TileType.Goal)
                {
                    gameLost = true;
                    Debug.Log("<color=red>게임 오버! 모든 명령어를 실행했지만 목표에 도달하지 못했습니다!</color>");
                }
            }
        }
        
        isSimulating = false;
    }
    
    void ExecuteCommand(Command command)
    {
        Vector2Int newPos = playerPos;
        
        switch (command)
        {
            case Command.Forward:    // W: 위로 이동
                newPos.y += 1;
                break;
            case Command.MoveLeft:   // A: 왼쪽으로 이동
                newPos.x -= 1;
                break;
            case Command.MoveRight:  // D: 오른쪽으로 이동
                newPos.x += 1;
                break;
            case Command.MoveDown:   // S: 아래로 이동
                newPos.y -= 1;
                break;
            case Command.Wait:       // _: 대기
                // 아무것도 하지 않음
                return;
        }
        
        // 맵 범위 체크 및 이동
        if (IsValidMovePosition(newPos))
        {
            playerPos = newPos;
        }
    }
    
    bool IsValidMovePosition(Vector2Int pos)
    {
        // 맵 범위 체크
        if (pos.x < 0 || pos.x >= mapWidth || pos.y < 0 || pos.y >= mapHeight)
            return false;
            
        // 벽이 아닌 경우에만 이동 가능
        return levelData[pos.x, pos.y] != TileType.Wall;
    }
    
    void CheckGameState()
    {
        // 목표 도달 체크
        if (levelData[playerPos.x, playerPos.y] == TileType.Goal)
        {
            gameWon = true;
            Debug.Log("<color=green>승리! 목표에 도달했습니다!</color>");
            return;
        }
        
        // 가시에 찔렸는지 체크 (가시가 활성화된 경우에만)
        if (levelData[playerPos.x, playerPos.y] == TileType.Spike)
        {
            Vector2Int spikePos = playerPos;
            if (spikeTimings.ContainsKey(spikePos))
            {
                List<bool> pattern = spikeTimings[spikePos];
                // 현재 턴에서 가시가 활성화되어 있는지 체크
                bool isActive = (currentTurn < pattern.Count) ? pattern[currentTurn] : false;
                
                if (isActive)
                {
                    gameLost = true;
                    Debug.Log("<color=red>게임 오버! 활성화된 가시에 찔렸습니다!</color>");
                }
                else
                {
                    Debug.Log("<color=orange>가시가 비활성화되어 안전하게 지나갈 수 있습니다.</color>");
                }
            }
        }
    }
    
    void ResetSimulation()
    {
        editorMode = true;
        isSimulating = false;
        currentTurn = 0;
        gameWon = false;
        gameLost = false;
        
        UpdatePlayerVisual();
        UpdateSpikeVisuals();
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
        if (editorMode)
        {
            GUILayout.Label("=== 레벨 에디터 ===");
            GUILayout.Label($"현재 브러시: {currentBrush}");
            GUILayout.Label("1:빈공간, 2:벽, 3:시작점, 4:목표점, 5:가시");
            GUILayout.Label("좌클릭:그리기, 우클릭:지우기");
            GUILayout.Label("스페이스바: 시뮬레이션 시작");
            
            GUILayout.Space(5);
            GUILayout.Label("--- 가시 상태 (턴 0 기준) ---");
            foreach (var kvp in spikeTimings)
            {
                Vector2Int pos = kvp.Key;
                List<bool> pattern = kvp.Value;
                bool isActive = pattern.Count > 0 ? pattern[0] : false;
                string status = isActive ? "<color=red>활성화</color>" : "<color=gray>비활성화</color>";
                GUILayout.Label($"가시({pos.x},{pos.y}): {status}");
            }
            
            GUILayout.Space(10);
            GUILayout.Label("명령어 시퀀스:");
            commandSequence = GUILayout.TextField(commandSequence);
            GUILayout.Label("W:위로, A:왼쪽으로, D:오른쪽으로, S:아래로, _:대기");
        }
        else
        {
            GUILayout.Label("=== 게임 플레이 ===");
            GUILayout.Label($"턴: {currentTurn + 1} / {commandSequence.Length}");
            GUILayout.Label($"플레이어 위치: ({playerPos.x}, {playerPos.y})");
            GUILayout.Label($"방향: {playerDirection}");
            
            if (gameWon)
            {
                GUILayout.Label("*** 승리! ***");
            }
            else if (gameLost)
            {
                GUILayout.Label("*** 게임 오버! ***");
            }
            else if (!isSimulating)
            {
                GUILayout.Label("스페이스바: 다음 명령어 실행");
            }
            
            GUILayout.Label("R: 리셋, ESC: 에디터로 돌아가기");
        }
        
        GUILayout.EndArea();
    }
}

// 기본 타일 생성을 위한 에디터 스크립트
#if UNITY_EDITOR
[CustomEditor(typeof(LevelDesigner))]
public class LevelDesignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LevelDesigner manager = (LevelDesigner)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("기본 프리팹 생성"))
        {
            CreateBasicPrefabs(manager);
        }
        
        if (GUILayout.Button("레벨 초기화"))
        {
            manager.InitializeGame();
        }
    }
    
    void CreateBasicPrefabs(LevelDesigner manager)
    {
        // 기본 큐브 프리팹들을 생성
        string prefabPath = "Assets/Prefabs/";
        
        if (!AssetDatabase.IsValidFolder(prefabPath))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        // 각 타일 타입별로 다른 색상의 큐브 생성
        CreateTilePrefab("EmptyTile", Color.white, prefabPath);
        CreateTilePrefab("WallTile", Color.black, prefabPath);
        CreateTilePrefab("StartTile", Color.green, prefabPath);
        CreateTilePrefab("GoalTile", Color.blue, prefabPath);
        CreateTilePrefab("SpikeTile", Color.red, prefabPath);
        CreateTilePrefab("Player", Color.yellow, prefabPath);
        
        AssetDatabase.Refresh();
        
        // 생성된 프리팹을 매니저에 할당
        manager.emptyTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "EmptyTile.prefab");
        manager.wallTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "WallTile.prefab");
        manager.startTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "StartTile.prefab");
        manager.goalTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "GoalTile.prefab");
        manager.spikeTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "SpikeTile.prefab");
        manager.playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath + "Player.prefab");
    }
    
    void CreateTilePrefab(string name, Color color, string path)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        
        // 머터리얼을 별도 에셋으로 생성
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
        {
            // URP가 없는 경우 Built-in RP 사용
            mat = new Material(Shader.Find("Standard"));
        }
        
        // Base Color 설정 (URP의 경우)
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", color);
        }
        // 또는 Main Color 설정 (Built-in RP의 경우)
        else if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", color);
        }
        
        // 머터리얼을 에셋으로 저장
        string matPath = path + name + "_Material.mat";
        AssetDatabase.CreateAsset(mat, matPath);
        
        // 저장된 머터리얼을 로드해서 할당
        Material savedMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        cube.GetComponent<Renderer>().material = savedMat;
        
        // 프리팹 저장
        PrefabUtility.SaveAsPrefabAsset(cube, path + name + ".prefab");
        DestroyImmediate(cube);
    }
}
#endif