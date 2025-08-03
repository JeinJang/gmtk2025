using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private ActionEventChannelSO _turnStartEvent;
    [SerializeField] private VoidEventChannelSO _turnEndEvent;
    [SerializeField] private ActionEventChannelSO _triggerActionEvent;
    [SerializeField] private VoidEventChannelSO _triggerMapActionEvent;
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
        StartCoroutine(Progress());
    }

    private IEnumerator Progress()
    {
        _triggerMapActionEvent.RaiseEvent();
        yield return new WaitForSeconds(1f);
        _triggerActionEvent.RaiseEvent(_turnStartEvent.actionType);
    }

    private void OnActionEnd()
    {
        _turnEndEvent.RaiseEvent();
    }
}
