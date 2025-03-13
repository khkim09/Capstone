using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 퀘스트 에디터
/// </summary>
#if UNITY_EDITOR
[CustomEditor(typeof(RandomQuest))]
public class RandomQuestEditor : Editor
{
    private bool[] objectiveFoldouts;
    private SerializedProperty questTitleProp;
    private SerializedProperty questDescriptionProp;
    private SerializedProperty questStatusProp;
    private SerializedProperty objectivesProp;
    private SerializedProperty rewardsProp;

    /// <summary>
    /// 퀘스트 활성화
    /// </summary>
    private void OnEnable()
    {
        // QuestManager의 Quest와 동일한 변수명을 사용한다고 가정합니다.
        questTitleProp = serializedObject.FindProperty("title");
        questDescriptionProp = serializedObject.FindProperty("description");
        questStatusProp = serializedObject.FindProperty("status");
        objectivesProp = serializedObject.FindProperty("objectives");
        rewardsProp = serializedObject.FindProperty("rewards");

        InitializeFoldouts();
    }

    /// <summary>
    /// 목표의 펼치기, 접기 상태 관리
    /// </summary>
    private void InitializeFoldouts()
    {
        objectiveFoldouts = new bool[objectivesProp.arraySize];
        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            objectiveFoldouts[i] = false;
        }
    }

    /// <summary>
    /// 데이터 동기화, 필드 표시, 목표 배열 관리, 버튼으로 퀘스트 Import 및 Export
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(questTitleProp, new GUIContent("퀘스트 제목"));

        EditorGUILayout.LabelField("퀘스트 설명");
        questDescriptionProp.stringValue = EditorGUILayout.TextArea(questDescriptionProp.stringValue, GUILayout.Height(100));

        EditorGUILayout.PropertyField(questStatusProp, new GUIContent("퀘스트 상태"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("목표", EditorStyles.boldLabel);

        // 목표 배열 관리
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"목표 수: {objectivesProp.arraySize}");

        if (GUILayout.Button("추가", GUILayout.Width(60)))
        {
            objectivesProp.arraySize++;
            InitializeFoldouts();
        }
        EditorGUILayout.EndHorizontal();

        // 각 목표 표시
        for (int i = 0; i < objectivesProp.arraySize; i++)
        {
            SerializedProperty objectiveProp = objectivesProp.GetArrayElementAtIndex(i);
            SerializedProperty objDescriptionProp = objectiveProp.FindPropertyRelative("description");
            SerializedProperty currentAmountProp = objectiveProp.FindPropertyRelative("currentAmount");
            SerializedProperty requiredAmountProp = objectiveProp.FindPropertyRelative("requiredAmount");
            SerializedProperty isCompletedProp = objectiveProp.FindPropertyRelative("isCompleted");

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            objectiveFoldouts[i] = EditorGUILayout.Foldout(objectiveFoldouts[i], $"목표 {i + 1}: {objDescriptionProp.stringValue}");
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
                EditorGUILayout.PropertyField(objDescriptionProp, new GUIContent("목표 설명"));
                EditorGUILayout.PropertyField(currentAmountProp, new GUIContent("현재 진행"));
                EditorGUILayout.PropertyField(requiredAmountProp, new GUIContent("필요 수치"));
                EditorGUILayout.PropertyField(isCompletedProp, new GUIContent("완료 여부"));
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("보상", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rewardsProp, true);

        EditorGUILayout.Space(10);
        // JSON 내보내기/가져오기 버튼
        if (GUILayout.Button("JSON으로 내보내기")) ExportQuestToJson();

        if (GUILayout.Button("JSON에서 가져오기")) ImportQuestFromJson();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// JSON에 퀘스트 저장
    /// </summary>
    private void ExportQuestToJson()
    {
        var quest = (RandomQuest)target;
        var json = JsonUtility.ToJson(quest, true);

        var path = EditorUtility.SaveFilePanel(
            "퀘스트 JSON 저장",
            Application.dataPath,
            quest.name + ".json",
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"퀘스트를 JSON으로 내보냈습니다: {path}");
        }
    }

    /// <summary>
    /// JSON에서 퀘스트 가져오기
    /// </summary>
    private void ImportQuestFromJson()
    {
        var path = EditorUtility.OpenFilePanel(
            "퀘스트 JSON 가져오기",
            Application.dataPath,
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var json = File.ReadAllText(path);
            var quest = (RandomQuest)target;
            JsonUtility.FromJsonOverwrite(json, quest);

            serializedObject.Update();
            InitializeFoldouts();

            Debug.Log($"JSON에서 퀘스트를 가져왔습니다: {path}");
        }
    }
}

/// <summary>
/// 퀘스트 매니저 창
/// </summary>
public class QuestManagerWindow : EditorWindow
{
    private readonly List<RandomQuest> allQuests = new List<RandomQuest>();
    private bool expandedSection = true;
    private Vector2 scrollPosition;
    private string searchFilter = "";

    /// <summary>
    /// 커스텀 UI 그리는 메서드
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("퀘스트 관리자", EditorStyles.boldLabel);

        // 검색
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("검색:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("지우기", GUILayout.Width(60)))
            searchFilter = "";
        EditorGUILayout.EndHorizontal();

        // 새 퀘스트 생성 버튼
        if (GUILayout.Button("새 퀘스트 생성"))
            CreateNewQuest();

        EditorGUILayout.Space(10);

        // 모든 퀘스트 로드
        if (GUILayout.Button("모든 퀘스트 로드"))
            LoadAllQuests();

        expandedSection = EditorGUILayout.Foldout(expandedSection, $"모든 퀘스트 ({allQuests.Count})");

        if (expandedSection)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var quest in allQuests)
            {
                // 검색 필터 적용
                if (!string.IsNullOrEmpty(searchFilter) &&
                    !quest.name.ToLower().Contains(searchFilter.ToLower()) &&
                    !quest.title.ToLower().Contains(searchFilter.ToLower()))
                    continue;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(quest, typeof(RandomQuest), false);

                if (GUILayout.Button("편집", GUILayout.Width(60)))
                    Selection.activeObject = quest;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// 윈도우창 띄우기
    /// </summary>
    [MenuItem("Game/Quest Manager")]
    public static void ShowWindow()
    {
        GetWindow<QuestManagerWindow>("퀘스트 관리자");
    }

    /// <summary>
    /// 모든 퀘스트 로드
    /// </summary>
    private void LoadAllQuests()
    {
        allQuests.Clear();

        var guids = AssetDatabase.FindAssets("t:RandomQuest");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<RandomQuest>(path);

            if (quest != null)
                allQuests.Add(quest);
        }
    }

    /// <summary>
    /// 새로운 퀘스트 생성
    /// </summary>
    private void CreateNewQuest()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "새 퀘스트 생성",
            "New Quest",
            "asset",
            "새 퀘스트 에셋의 이름을 입력하세요"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var newQuest = CreateInstance<RandomQuest>();
            AssetDatabase.CreateAsset(newQuest, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = newQuest;
            LoadAllQuests();
        }
    }
}
#endif
