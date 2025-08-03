using UnityEngine;

public enum ActionType
{
    MOVE_FORWARD,
    MOVE_BACKWARD,
    MOVE_LEFT,
    MOVE_RIGHT
}

[CreateAssetMenu(fileName = "ActionBlockSO", menuName = "Scriptable Objects/Action Block")]
public class ActionBlockSO : ScriptableObject
{
    public ActionType type;
    public Sprite sprite;
}
