using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private ActionEventChannelSO _turnStartEvent;
    [SerializeField] private VoidEventChannelSO _turnEndEvent;
    [SerializeField] private ActionEventChannelSO _triggerActionEvent;
    [SerializeField] private VoidEventChannelSO _actionEndEvent;

    private void OnEnable()
    {
        _turnStartEvent.OnEventRaised += OnTurnStart;
        _actionEndEvent.OnEventRaised += OnActionEnd;
    }

    private void OnDisable()
    {
        _turnStartEvent.OnEventRaised -= OnTurnStart;
        _actionEndEvent.OnEventRaised -= OnActionEnd;
    }


    private void OnTurnStart()
    {
        Debug.Log(string.Format("액션: {0}", _turnStartEvent.actionType));

        _triggerActionEvent.RaiseEvent(_turnStartEvent.actionType);
    }

    private void OnActionEnd()
    {
        _turnEndEvent.RaiseEvent();
    }
}
