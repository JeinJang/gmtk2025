using UnityEngine;
using UnityEditor;
using System.Reflection;

#if UNITY_EDITOR

[CustomEditor(typeof(VoidEventChannelSO))]
public class VoidEventChannelSOEditor : Editor
{
    private GUIStyle _whiteLineStyle;

    private void OnEnable()
    {
        // 흰색 선 스타일 초기화
        _whiteLineStyle = new GUIStyle();
        _whiteLineStyle.normal.background = Texture2D.whiteTexture;
        _whiteLineStyle.fixedHeight = 1;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        VoidEventChannelSO eventChannel = (VoidEventChannelSO)target;


        EditorGUILayout.LabelField("Listeners: ", EditorStyles.boldLabel);
        GUILayout.Box(
            GUIContent.none,
            _whiteLineStyle,
            GUILayout.ExpandWidth(true),
            GUILayout.Height(1));

        if (eventChannel.OnEventRaised != null)
        {
            foreach (var listener in eventChannel.OnEventRaised.GetInvocationList())
            {
                if (listener.Target != null)
                {
                    string targetName = listener.Target.GetType().Name;
                    string methodName = listener.Method.Name;

                    if (GUILayout.Button($"{methodName} ({targetName})", EditorStyles.linkLabel))
                    {
                        OpenScriptAtMethod(listener);
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.LabelField("No Listeners Registered.");
        }
    }

    private void OpenScriptAtMethod(System.Delegate action)
    {
        // 메서드 정보 가져오기
        MethodInfo methodInfo = action.Method;

        // 대상 클래스 타입
        System.Type declaringType = methodInfo.DeclaringType;

        // 스크립트 파일 경로 찾기
        string[] guids = AssetDatabase.FindAssets($"{declaringType.Name} t:MonoScript");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script != null && script.GetClass() == declaringType)
            {
                // 해당 스크립트를 열고 메서드 위치로 이동
                AssetDatabase.OpenAsset(script, GetLineNumber(script, methodInfo.Name));
                break;
            }
        }
    }

    private int GetLineNumber(MonoScript script, string methodName)
    {
        // 스크립트의 텍스트 읽기
        string[] lines = script.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains($"void {methodName}") || lines[i].Contains($"{methodName}("))
            {
                return i + 1; // Unity는 1부터 시작
            }
        }
        return 0;
    }
}

#endif