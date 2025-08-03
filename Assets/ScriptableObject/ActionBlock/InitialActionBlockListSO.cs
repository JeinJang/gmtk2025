using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InitialActionBlockListSO", menuName = "Scriptable Objects/Initial Action Block List")]
public class InitialActionBlockListSO : ScriptableObject
{
    public List<ActionBlockSO> list;
}
