using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _gameStartEvent;
    [SerializeField] private VoidEventChannelSO _gameFailedEvent;
    [SerializeField] private VoidEventChannelSO _gameClearEvent;

    private GameObject _playButton;

    void Start()
    {
        _playButton = transform.Find("Button_Play").gameObject;

        if (_playButton == null)
        {
            Debug.Log("play button 없음");
        }
    }



    private void OnEnable()
    {
        _gameStartEvent.OnEventRaised += OnGameStart;
        _gameFailedEvent.OnEventRaised += OnGameEnd;
        _gameClearEvent.OnEventRaised += OnGameEnd;
    }

    private void OnDisable()
    {
        _gameStartEvent.OnEventRaised -= OnGameStart;
        _gameFailedEvent.OnEventRaised -= OnGameEnd;
        _gameClearEvent.OnEventRaised -= OnGameEnd;
    }

    private void OnGameStart()
    {
        _playButton.SetActive(false);
    }
    private void OnGameEnd()
    {
        _playButton.SetActive(true);
    }
}
