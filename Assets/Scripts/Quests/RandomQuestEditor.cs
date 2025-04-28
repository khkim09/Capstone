#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomQuest))]
public class RandomQuestEditor : Editor
{
    // 퀘스트 기본 프로퍼티
    SerializedProperty questIdProp;
    SerializedProperty statusProp;
    SerializedProperty titleProp;
    SerializedProperty descProp;

    // 목표 및 보상 리스트
    SerializedProperty objectivesProp;
    SerializedProperty rewardsProp;

    // Foldout 상태 저장용
    bool[] objectiveFoldouts;
    bool[] rewardFoldouts;

    private void OnEnable()
    {
        // 프로퍼티 캐싱
        questIdProp     = serializedObject.FindProperty("questId");
        statusProp      = serializedObject.FindProperty("status");
        titleProp       = serializedObject.FindProperty("title");
        descProp        = serializedObject.FindProperty("description");
        objectivesProp  = serializedObject.FindProperty("objectives");
        rewardsProp     = serializedObject.FindProperty("rewards");

        InitializeFoldouts();
    }

    private void InitializeFoldouts()
    {
        objectiveFoldouts = new bool[objectivesProp.arraySize];
        for (int i = 0; i < objectiveFoldouts.Length; i++) objectiveFoldouts[i] = false;

        rewardFoldouts = new bool[rewardsProp.arraySize];
        for (int i = 0; i < rewardFoldouts.Length; i++) rewardFoldouts[i] = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // —— 식별 및 상태 ——
        EditorGUILayout.LabelField("식별 및 상태", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(questIdProp, new GUIContent("퀘스트 ID"));
        EditorGUILayout.PropertyField(statusProp,  new GUIContent("상태"));

        EditorGUILayout.Space();

        // —— 기본 정보 ——
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(titleProp, new GUIContent("제목"));
        EditorGUILayout.LabelField("설명");
        descProp.stringValue = EditorGUILayout.TextArea(descProp.stringValue, GUILayout.Height(60));

        EditorGUILayout.Space(10);

        // —— 조건(목표) 리스트 ——
        EditorGUILayout.LabelField("조건 (Collect Item)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"목표 수: {objectivesProp.arraySize}");
        if (GUILayout.Button("추가", GUILayout.Width(60)))
        {
            objectivesProp.arraySize++;
            InitializeFoldouts();
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            var objProp    = objectivesProp.GetArrayElementAtIndex(i);
            var targetIdProp = objProp.FindPropertyRelative("targetId");
            var descOProp  = objProp.FindPropertyRelative("description");
            var reqProp    = objProp.FindPropertyRelative("requiredAmount");
            // currentAmount, isCompleted 은 런타임 전용이라 숨김

            // 헤더
            EditorGUILayout.BeginHorizontal();
            objectiveFoldouts[i] = EditorGUILayout.Foldout(
                objectiveFoldouts[i],
                $"조건 {i + 1}: {descOProp.stringValue}"
            );
            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                objectivesProp.DeleteArrayElementAtIndex(i);
                InitializeFoldouts();
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (objectiveFoldouts[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(targetIdProp, new GUIContent("대상 아이템 ID"));
                EditorGUILayout.PropertyField(descOProp,    new GUIContent("설명"));
                EditorGUILayout.PropertyField(reqProp,      new GUIContent("필요 수량"));
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(10);

        // —— 보상 리스트 ——
        EditorGUILayout.LabelField("보상", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"보상 수: {rewardsProp.arraySize}");
        if (GUILayout.Button("추가", GUILayout.Width(60)))
        {
            rewardsProp.arraySize++;
            InitializeFoldouts();
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < rewardsProp.arraySize; i++)
        {
            var rwProp = rewardsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            rewardFoldouts[i] = EditorGUILayout.Foldout(rewardFoldouts[i], $"보상 {i + 1}");
            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                rewardsProp.DeleteArrayElementAtIndex(i);
                InitializeFoldouts();
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUILayout.EndHorizontal();

            if (rewardFoldouts[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(rwProp.FindPropertyRelative("type"),   new GUIContent("보상 종류"));
                EditorGUILayout.PropertyField(rwProp.FindPropertyRelative("amount"), new GUIContent("수량"));
                // ResourceType, itemId, itemQuantity 등 추가 속성이 필요하면 여기에
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(10);

        // —— JSON 입출력 (선택) ——
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("JSON으로 내보내기")) ExportToJson();
        if (GUILayout.Button("JSON에서 가져오기")) ImportFromJson();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void ExportToJson()
    {
        var rq   = (RandomQuest)target;
        var json = JsonUtility.ToJson(rq, true);
        var path = EditorUtility.SaveFilePanel(
            "퀘스트 JSON 저장",
            Application.dataPath,
            rq.name + ".json",
            "json"
        );
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"[RandomQuestEditor] JSON 저장: {path}");
        }
    }

    private void ImportFromJson()
    {
        var path = EditorUtility.OpenFilePanel(
            "퀘스트 JSON 불러오기",
            Application.dataPath,
            "json"
        );
        if (string.IsNullOrEmpty(path)) return;

        var json = File.ReadAllText(path);
        JsonUtility.FromJsonOverwrite(json, target);
        serializedObject.Update();
        InitializeFoldouts();
        Debug.Log($"[RandomQuestEditor] JSON 불러옴: {path}");
    }
}
#endif
