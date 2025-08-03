using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance { get; private set; }

    [SerializeField] private VoidEventChannelSO _gameStartEvent;
    [SerializeField] private VoidEventChannelSO _gameFailedEvent;
    [SerializeField] private VoidEventChannelSO _gameClearEvent;
    [SerializeField] private ActionEventChannelSO _turnStartEvent;
    [SerializeField] private VoidEventChannelSO _turnEndEvent;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        // 디버깅용
        // 게임 시작 이벤트
        if (Input.GetKeyDown(KeyCode.S))
        {
            _gameStartEvent.RaiseEvent();
        }

        // 게임 실패 이벤트
        if (Input.GetKeyDown(KeyCode.F))
        {
            _gameFailedEvent.RaiseEvent();
        }

        // 한 턴 끝나는 이벤트
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _turnEndEvent.RaiseEvent();
        }

        // 디버깅용 : Scene Reset
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }

    private void OnEnable()
    {
        _turnStartEvent.OnEventRaised += OnTurnStart;
    }

    private void OnDisable()
    {
        _turnStartEvent.OnEventRaised -= OnTurnStart;
    }


    private void OnTurnStart()
    {
        Debug.Log(string.Format("액션: {0}", _turnStartEvent.actionType));
    }
}
