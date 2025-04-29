using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(RandomEvent))]
public class RandomEventEditor : Editor
{
    private bool[] choiceFoldouts;
    private SerializedProperty eventIdProp;
    private SerializedProperty choicesProp;
    private SerializedProperty eventDescriptionProp;
    private SerializedProperty eventImageProp;
    private SerializedProperty eventTitleProp;
    private bool[] outcomeFoldouts;

    private void OnEnable()
    {
        eventIdProp = serializedObject.FindProperty("eventId");
        eventTitleProp = serializedObject.FindProperty("eventTitle");
        eventDescriptionProp = serializedObject.FindProperty("eventDescription");
        eventImageProp = serializedObject.FindProperty("eventImage");
        choicesProp = serializedObject.FindProperty("choices");

        InitializeFoldouts();
    }

    private void InitializeFoldouts()
    {
        choiceFoldouts = new bool[choicesProp.arraySize];
        for (var i = 0; i < choicesProp.arraySize; i++)
        {
            choiceFoldouts[i] = false;

            var outcomesProp = choicesProp.GetArrayElementAtIndex(i).FindPropertyRelative("possibleOutcomes");
            if (outcomeFoldouts == null || outcomeFoldouts.Length != outcomesProp.arraySize)
                outcomeFoldouts = new bool[outcomesProp.arraySize];
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(eventIdProp, new GUIContent("이벤트 ID"));

        EditorGUILayout.PropertyField(eventTitleProp);

        EditorGUILayout.LabelField("이벤트 설명");
        eventDescriptionProp.stringValue =
            EditorGUILayout.TextArea(eventDescriptionProp.stringValue, GUILayout.Height(100));

        EditorGUILayout.PropertyField(eventImageProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("선택지", EditorStyles.boldLabel);

        // 선택지 배열 관리
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"선택지 수: {choicesProp.arraySize}");

        if (GUILayout.Button("추가", GUILayout.Width(60)))
        {
            choicesProp.arraySize++;
            InitializeFoldouts();
        }

        EditorGUILayout.EndHorizontal();

        // 각 선택지 표시
        for (var i = 0; i < choicesProp.arraySize; i++)
        {
            var choiceProp = choicesProp.GetArrayElementAtIndex(i);
            var choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
            var outcomesProp = choiceProp.FindPropertyRelative("possibleOutcomes");

            EditorGUILayout.Space(5);

            // 선택지 헤더
            EditorGUILayout.BeginHorizontal();
            choiceFoldouts[i] =
                EditorGUILayout.Foldout(choiceFoldouts[i], $"선택지 {i + 1}: {choiceTextProp.stringValue}");

            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                choicesProp.DeleteArrayElementAtIndex(i);
                InitializeFoldouts();
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.EndHorizontal();

            if (choiceFoldouts[i])
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(choiceTextProp);

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("가능한 결과", EditorStyles.boldLabel);

                // 결과 배열 관리
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"결과 수: {outcomesProp.arraySize}");

                if (GUILayout.Button("추가", GUILayout.Width(60)))
                {
                    outcomesProp.arraySize++;

                    // 기본 확률 설정 (균등 분포)
                    var defaultProb = 100f / outcomesProp.arraySize;
                    for (var j = 0; j < outcomesProp.arraySize; j++)
                    {
                        var probProp = outcomesProp.GetArrayElementAtIndex(j).FindPropertyRelative("probability");
                        probProp.floatValue = defaultProb;
                    }
                }

                EditorGUILayout.EndHorizontal();

                // 각 결과 표시
                float totalProb = 0;
                for (var j = 0; j < outcomesProp.arraySize; j++)
                {
                    var outcomeProp = outcomesProp.GetArrayElementAtIndex(j);
                    var outcomeTextProp = outcomeProp.FindPropertyRelative("outcomeText");
                    var probabilityProp = outcomeProp.FindPropertyRelative("probability");

                    totalProb += probabilityProp.floatValue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"결과 {j + 1} ({probabilityProp.floatValue}%)");

                    if (GUILayout.Button("삭제", GUILayout.Width(60)))
                    {
                        outcomesProp.DeleteArrayElementAtIndex(j);
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;

                    EditorGUILayout.LabelField("결과 텍스트");
                    outcomeTextProp.stringValue =
                        EditorGUILayout.TextArea(outcomeTextProp.stringValue, GUILayout.Height(60));

                    EditorGUILayout.PropertyField(probabilityProp, new GUIContent("확률 (%)"));

                    // 행성 효과
                    var planetEffectsProp = outcomeProp.FindPropertyRelative("planetEffects");
                    EditorGUILayout.PropertyField(planetEffectsProp, new GUIContent("행성 효과"), true);

                    // 자원 효과
                    var resourceEffectsProp = outcomeProp.FindPropertyRelative("resourceEffects");
                    EditorGUILayout.PropertyField(resourceEffectsProp, new GUIContent("자원 효과"), true);

                    // 승무원 효과
                    var crewEffectsProp = outcomeProp.FindPropertyRelative("crewEffects");
                    EditorGUILayout.PropertyField(crewEffectsProp, new GUIContent("승무원 효과"), true);

                    // 특수 효과
                    var specialEffectTypeProp = outcomeProp.FindPropertyRelative("specialEffectType");
                    EditorGUILayout.PropertyField(specialEffectTypeProp);

                    // 특수 효과 유형에 따른 추가 속성
                    if (specialEffectTypeProp.enumValueIndex == (int)SpecialEffectType.TriggerNextEvent)
                    {
                        var nextEventProp = outcomeProp.FindPropertyRelative("nextEvent");
                        EditorGUILayout.PropertyField(nextEventProp);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space(5);
                }

                // 확률 합계 검증
                if (Math.Abs(totalProb - 100f) > 0.01f)
                {
                    EditorGUILayout.HelpBox($"확률의 합이 100%가 아닙니다! (현재: {totalProb:F1}%)", MessageType.Warning);

                    if (GUILayout.Button("확률 균등 분배"))
                    {
                        var equalProb = 100f / outcomesProp.arraySize;
                        for (var j = 0; j < outcomesProp.arraySize; j++)
                        {
                            var probProp = outcomesProp.GetArrayElementAtIndex(j).FindPropertyRelative("probability");
                            probProp.floatValue = equalProb;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(10);

        // 이벤트 JSON 내보내기/가져오기 버튼
        if (GUILayout.Button("JSON으로 내보내기")) ExportEventToJson();

        if (GUILayout.Button("JSON에서 가져오기")) ImportEventFromJson();

        serializedObject.ApplyModifiedProperties();
    }

    private void ExportEventToJson()
    {
        var randomEvent = (RandomEvent)target;
        var json = JsonUtility.ToJson(randomEvent, true);

        var path = EditorUtility.SaveFilePanel(
            "이벤트 JSON 저장",
            Application.dataPath,
            randomEvent.name + ".json",
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"이벤트를 JSON으로 내보냈습니다: {path}");
        }
    }

    private void ImportEventFromJson()
    {
        var path = EditorUtility.OpenFilePanel(
            "이벤트 JSON 가져오기",
            Application.dataPath,
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var json = File.ReadAllText(path);

            var randomEvent = (RandomEvent)target;
            JsonUtility.FromJsonOverwrite(json, randomEvent);

            serializedObject.Update();
            InitializeFoldouts();

            Debug.Log($"JSON에서 이벤트를 가져왔습니다: {path}");
        }
    }
}

public class EventManagerWindow : EditorWindow
{
    private readonly List<RandomEvent> allEvents = new();
    private bool expandedSection = true;
    private Vector2 scrollPosition;
    private string searchFilter = "";

    private void OnGUI()
    {
        GUILayout.Label("이벤트 관리자", EditorStyles.boldLabel);

        // 검색
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("검색:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("지우기", GUILayout.Width(60))) searchFilter = "";
        EditorGUILayout.EndHorizontal();

        // 새 이벤트 생성 버튼
        if (GUILayout.Button("새 이벤트 생성")) CreateNewEvent();

        EditorGUILayout.Space(10);

        // 모든 이벤트 로드
        if (GUILayout.Button("모든 이벤트 로드")) LoadAllEvents();

        expandedSection = EditorGUILayout.Foldout(expandedSection, $"모든 이벤트 ({allEvents.Count})");

        if (expandedSection)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var randomEvent in allEvents)
            {
                // 검색 필터 적용
                if (!string.IsNullOrEmpty(searchFilter) &&
                    !randomEvent.name.ToLower().Contains(searchFilter.ToLower()) &&
                    !randomEvent.eventTitle.ToLower().Contains(searchFilter.ToLower()))
                    continue;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(randomEvent, typeof(RandomEvent), false);

                if (GUILayout.Button("편집", GUILayout.Width(60))) Selection.activeObject = randomEvent;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
    }

    [MenuItem("Game/Event Manager")]
    public static void ShowWindow()
    {
        GetWindow<EventManagerWindow>("이벤트 관리자");
    }

    private void LoadAllEvents()
    {
        allEvents.Clear();

        var guids = AssetDatabase.FindAssets("t:RandomEvent");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var randomEvent = AssetDatabase.LoadAssetAtPath<RandomEvent>(path);

            if (randomEvent != null) allEvents.Add(randomEvent);
        }
    }

    private void CreateNewEvent()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "새 이벤트 생성",
            "New Event",
            "asset",
            "새 랜덤 이벤트 에셋의 이름을 입력하세요"
        );

        if (!string.IsNullOrEmpty(path))
        {
            var newEvent = CreateInstance<RandomEvent>();
            AssetDatabase.CreateAsset(newEvent, path);
            AssetDatabase.SaveAssets();

            Selection.activeObject = newEvent;
            LoadAllEvents();
        }
    }
}
#endif
