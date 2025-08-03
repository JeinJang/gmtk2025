using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Scriptable Objects/Events/Action Event Channel", fileName = "Action Event Channel")]
public class ActionEventChannelSO : ScriptableObject
{
	public UnityAction OnEventRaised;
	[TextArea] public string description;
	public ActionType? actionType { get; private set; }

	public void RaiseEvent(ActionType? type)
	{
		actionType = type;
	
		if (OnEventRaised != null)
			OnEventRaised.Invoke();
	}
}
