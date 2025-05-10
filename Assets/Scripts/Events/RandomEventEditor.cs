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
    private SerializedProperty eventTypeProp;
    private SerializedProperty outcomeTypeProp;
    private SerializedProperty debugNameProp;
    private SerializedProperty minimumYearProp;
    private SerializedProperty minimumCOMAProp;
    private SerializedProperty minimumFuelProp;
    private SerializedProperty requiredCrewRaceProp;
    private bool[] outcomeFoldouts;
    private CrewRace[] allCrewRace;
    private bool showRequiredCrewFoldout = false;
    private Dictionary<CrewRace, bool> crewRaceChecked = new();
    private HashSet<CrewRace> autoDetectedRequiredRaces = new();
    private Dictionary<CrewRace, bool> crewRaceReadOnly = new();


    private void OnEnable()
    {
        eventIdProp = serializedObject.FindProperty("eventId");
        debugNameProp = serializedObject.FindProperty("debugName");
        eventTitleProp = serializedObject.FindProperty("eventTitle");
        eventDescriptionProp = serializedObject.FindProperty("eventDescription");
        eventImageProp = serializedObject.FindProperty("eventImage");
        eventTypeProp = serializedObject.FindProperty("eventType");
        outcomeTypeProp = serializedObject.FindProperty("outcomeType");
        minimumYearProp = serializedObject.FindProperty("minimumYear");
        minimumCOMAProp = serializedObject.FindProperty("minimumCOMA");
        minimumFuelProp = serializedObject.FindProperty("minimumFuel");
        choicesProp = serializedObject.FindProperty("choices");
        requiredCrewRaceProp = serializedObject.FindProperty("requiredCrewRace");

        InitializeFoldouts();
        InitializeCrewRace();
        DetectRequiredCrewRacesFromEffects();
    }

    private void InitializeFoldouts()
    {
        choiceFoldouts = new bool[choicesProp.arraySize];
        for (int i = 0; i < choicesProp.arraySize; i++)
        {
            choiceFoldouts[i] = false;

            SerializedProperty outcomesProp =
                choicesProp.GetArrayElementAtIndex(i).FindPropertyRelative("possibleOutcomes");
            if (outcomeFoldouts == null || outcomeFoldouts.Length != outcomesProp.arraySize)
                outcomeFoldouts = new bool[outcomesProp.arraySize];
        }
    }

    private void InitializeCrewRace()
    {
        // 모든 CrewSpecies 열거형 값 가져오기
        allCrewRace = (CrewRace[])Enum.GetValues(typeof(CrewRace));

        // 현재 RandomEvent의 requiredCrewSpecies 리스트 가져오기
        RandomEvent randomEvent = (RandomEvent)target;

        foreach (CrewRace race in allCrewRace)
            // None은 모든 종족을 의미하므로 체크 대상에서 제외
            if (race != CrewRace.None)
                crewRaceChecked[race] = randomEvent.requiredCrewRace.Contains(race);
    }

    private void DetectRequiredCrewRacesFromEffects()
    {
        // 자동 감지된 종족 목록 초기화
        autoDetectedRequiredRaces.Clear();

        // 현재 RandomEvent 가져오기
        RandomEvent randomEvent = (RandomEvent)target;

        // 각 선택지 순회
        foreach (EventChoice choice in randomEvent.choices)
            // 각 선택지의 결과 순회
        foreach (EventOutcome outcome in choice.possibleOutcomes)
            // 각 선원 효과 순회
        foreach (CrewEffect crewEffect in outcome.crewEffects)
            // None이 아닌 특정 종족에 적용되는 효과가 있다면, 자동으로 필수 종족으로 추가
            if (crewEffect.raceType != CrewRace.None)
                autoDetectedRequiredRaces.Add(crewEffect.raceType);

        // requiredCrewRaces 리스트 완전 교체 (자동 감지된 것만 포함)
        randomEvent.requiredCrewRace = new List<CrewRace>(autoDetectedRequiredRaces);

        // 변경사항 저장
        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(eventIdProp, new GUIContent("이벤트 ID"));
        if (EditorGUI.EndChangeCheck()) // 변경이 있으면
        {
            // ID가 변경되었을 때 모든 텍스트 자동 업데이트
            int newId = eventIdProp.intValue;
            UpdateAllTextsBasedOnEventId(newId);
        }

        EditorGUILayout.PropertyField(debugNameProp, new GUIContent("디버그 이름"));

        GUI.enabled = false; // UI 요소 비활성화 (읽기 전용)
        EditorGUILayout.PropertyField(eventTitleProp, new GUIContent("이벤트 타이틀 (자동)"));

        EditorGUILayout.LabelField("이벤트 설명 (자동)");
        eventDescriptionProp.stringValue =
            EditorGUILayout.TextArea(eventDescriptionProp.stringValue, GUILayout.Height(100));
        GUI.enabled = true; // UI 요소 다시 활성화

        EditorGUILayout.PropertyField(eventImageProp);

        // 이벤트 타입 선택 필드
        EditorGUILayout.PropertyField(eventTypeProp, new GUIContent("이벤트 타입"));

        // 결과 타입 선택 필드
        EditorGUILayout.PropertyField(outcomeTypeProp, new GUIContent("결과 타입"));

        EditorGUILayout.PropertyField(minimumYearProp, new GUIContent("최소 발생 년도"));

        EditorGUILayout.PropertyField(minimumCOMAProp, new GUIContent("최소 필요 COMA"));

        EditorGUILayout.PropertyField(minimumFuelProp, new GUIContent("최소 필요 연료"));

        EditorGUILayout.Space(10);
        showRequiredCrewFoldout = EditorGUILayout.Foldout(showRequiredCrewFoldout, "필요한 선원 종족", true);

        if (showRequiredCrewFoldout)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox("효과에 포함된 종족만 자동으로 감지됩니다. 모든 체크박스는 읽기 전용입니다.", MessageType.Info);

            RandomEvent randomEvent = (RandomEvent)target;

            // 모든 체크박스를 읽기 전용으로 설정
            GUI.enabled = false;

            foreach (CrewRace race in allCrewRace)
            {
                // None은 제외 (모든 종족을 의미하므로 체크 대상이 아님)
                if (race == CrewRace.None)
                    continue;

                bool isChecked = autoDetectedRequiredRaces.Contains(race);
                EditorGUILayout.Toggle(race.ToString(), isChecked);
            }

            // UI 활성화 복원
            GUI.enabled = true;

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("선택지", EditorStyles.boldLabel);

        // 선택지 배열 관리
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"선택지 수: {choicesProp.arraySize}");

        if (GUILayout.Button("추가", GUILayout.Width(60)))
        {
            choicesProp.arraySize++;
            InitializeFoldouts();
            UpdateAllTextsBasedOnEventId(eventIdProp.intValue);
        }

        EditorGUILayout.EndHorizontal();

        // 각 선택지 표시
        for (int i = 0; i < choicesProp.arraySize; i++)
        {
            SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
            SerializedProperty choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
            SerializedProperty outcomesProp = choiceProp.FindPropertyRelative("possibleOutcomes");

            EditorGUILayout.Space(5);

            // 선택지 헤더
            EditorGUILayout.BeginHorizontal();
            choiceFoldouts[i] =
                EditorGUILayout.Foldout(choiceFoldouts[i], $"선택지 {i + 1}: {choiceTextProp.stringValue}");

            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                choicesProp.DeleteArrayElementAtIndex(i);
                InitializeFoldouts();
                UpdateAllTextsBasedOnEventId(eventIdProp.intValue);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.EndHorizontal();

            if (choiceFoldouts[i])
            {
                EditorGUI.indentLevel++;

                GUI.enabled = false;
                EditorGUILayout.PropertyField(choiceTextProp, new GUIContent("선택지 텍스트 (자동)"));
                GUI.enabled = true;

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("가능한 결과", EditorStyles.boldLabel);

                // 결과 배열 관리
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"결과 수: {outcomesProp.arraySize}");

                if (GUILayout.Button("추가", GUILayout.Width(60))) HandleOutcomeAdded(outcomesProp, i); // 결과 추가 핸들러 호출

                EditorGUILayout.EndHorizontal();

                // 각 결과 표시
                float totalProb = 0;
                for (int j = 0; j < outcomesProp.arraySize; j++)
                {
                    SerializedProperty outcomeProp = outcomesProp.GetArrayElementAtIndex(j);
                    SerializedProperty outcomeTextProp = outcomeProp.FindPropertyRelative("outcomeText");
                    SerializedProperty probabilityProp = outcomeProp.FindPropertyRelative("probability");

                    totalProb += probabilityProp.floatValue;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"결과 {j + 1} ({probabilityProp.floatValue}%)");

                    if (GUILayout.Button("삭제", GUILayout.Width(60)))
                    {
                        outcomesProp.DeleteArrayElementAtIndex(j);
                        UpdateAllTextsBasedOnEventId(eventIdProp.intValue);
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel++;

                    GUI.enabled = false;
                    EditorGUILayout.LabelField("결과 텍스트 (자동)");
                    EditorGUILayout.TextArea(outcomeTextProp.stringValue, GUILayout.Height(60));
                    GUI.enabled = true;

                    EditorGUILayout.PropertyField(probabilityProp, new GUIContent("확률 (%)"));

                    // 행성 효과
                    SerializedProperty planetEffectsProp = outcomeProp.FindPropertyRelative("planetEffects");
                    EditorGUILayout.PropertyField(planetEffectsProp, new GUIContent("행성 효과"), true);

                    // 자원 효과
                    SerializedProperty resourceEffectsProp = outcomeProp.FindPropertyRelative("resourceEffects");
                    EditorGUILayout.PropertyField(resourceEffectsProp, new GUIContent("자원 효과"), true);

                    // 승무원 효과
                    SerializedProperty crewEffectsProp = outcomeProp.FindPropertyRelative("crewEffects");
                    EditorGUILayout.PropertyField(crewEffectsProp, new GUIContent("승무원 효과"), true);

                    // 특수 효과
                    SerializedProperty specialEffectTypeProp = outcomeProp.FindPropertyRelative("specialEffectType");
                    EditorGUILayout.PropertyField(specialEffectTypeProp);

                    // 특수 효과 값
                    SerializedProperty specialEffectValueProp = outcomeProp.FindPropertyRelative("specialEffectValue");
                    if (specialEffectTypeProp.enumValueIndex != (int)SpecialEffectType.None)
                    {
                        EditorGUILayout.PropertyField(specialEffectValueProp, new GUIContent("특수 효과 값"));

                        SerializedProperty specialEffectAmountProp =
                            outcomeProp.FindPropertyRelative("specialEffectAmount");
                        EditorGUILayout.PropertyField(specialEffectAmountProp, new GUIContent("특수 효과 수치"));
                    }

                    // 특수 효과 유형에 따른 추가 속성
                    if (specialEffectTypeProp.enumValueIndex == (int)SpecialEffectType.TriggerNextEvent)
                    {
                        SerializedProperty nextEventProp = outcomeProp.FindPropertyRelative("nextEvent");
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
                        float equalProb = 100f / outcomesProp.arraySize;
                        for (int j = 0; j < outcomesProp.arraySize; j++)
                        {
                            SerializedProperty probProp = outcomesProp.GetArrayElementAtIndex(j)
                                .FindPropertyRelative("probability");
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
        RandomEvent randomEvent = (RandomEvent)target;
        string json = JsonUtility.ToJson(randomEvent, true);

        string path = EditorUtility.SaveFilePanel(
            "이벤트 JSON 저장",
            Application.dataPath,
            randomEvent.name + ".json",
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            // UTF-8 인코딩으로 저장 (BOM 포함)
            File.WriteAllText(path, json, new System.Text.UTF8Encoding(true));
            Debug.Log($"이벤트를 JSON으로 내보냈습니다: {path}");
        }
    }

    private void ImportEventFromJson()
    {
        string path = EditorUtility.OpenFilePanel(
            "이벤트 JSON 가져오기",
            Application.dataPath,
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            string json = File.ReadAllText(path);

            RandomEvent randomEvent = (RandomEvent)target;
            JsonUtility.FromJsonOverwrite(json, randomEvent);

            serializedObject.Update();
            InitializeFoldouts();

            UpdateAllTextsBasedOnEventId(randomEvent.eventId);

            Debug.Log($"JSON에서 이벤트를 가져왔습니다: {path}");
        }
    }

    private void HandleChoiceAdded()
    {
        choicesProp.arraySize++;
        int newIndex = choicesProp.arraySize - 1;

        // 이벤트 ID 가져오기
        int eventId = eventIdProp.intValue;

        // 자동으로 선택지 텍스트 설정
        SerializedProperty choiceTextProp =
            choicesProp.GetArrayElementAtIndex(newIndex).FindPropertyRelative("choiceText");
        choiceTextProp.stringValue = $"event.choice.{eventId}.{newIndex + 1}";

        InitializeFoldouts();
    }

    // 결과 추가 버튼 클릭 시 자동 텍스트 설정을 위한 핸들러
    private void HandleOutcomeAdded(SerializedProperty outcomesProp, int choiceIndex)
    {
        outcomesProp.arraySize++;
        int newIndex = outcomesProp.arraySize - 1;

        // 이벤트 ID 가져오기
        int eventId = eventIdProp.intValue;

        // 자동으로 결과 텍스트 설정
        SerializedProperty outcomeTextProp =
            outcomesProp.GetArrayElementAtIndex(newIndex).FindPropertyRelative("outcomeText");
        outcomeTextProp.stringValue = $"event.result.{eventId}.{choiceIndex + 1}.{newIndex + 1}";

        // 기본 확률 설정 (균등 분포)
        float defaultProb = 100f / outcomesProp.arraySize;
        for (int j = 0; j < outcomesProp.arraySize; j++)
        {
            SerializedProperty probProp = outcomesProp.GetArrayElementAtIndex(j).FindPropertyRelative("probability");
            probProp.floatValue = defaultProb;
        }
    }

    private void UpdateAllTextsBasedOnEventId(int newId)
    {
        // 이벤트 타이틀과 설명 업데이트
        eventTitleProp.stringValue = $"event.name.{newId}";
        eventDescriptionProp.stringValue = $"event.description.{newId}";

        // 모든 선택지 텍스트 업데이트
        for (int i = 0; i < choicesProp.arraySize; i++)
        {
            SerializedProperty choiceProp = choicesProp.GetArrayElementAtIndex(i);
            SerializedProperty choiceTextProp = choiceProp.FindPropertyRelative("choiceText");
            choiceTextProp.stringValue = $"event.choice.{newId}.{i + 1}";

            // 해당 선택지의 모든 결과 텍스트 업데이트
            SerializedProperty outcomesProp = choiceProp.FindPropertyRelative("possibleOutcomes");
            for (int j = 0; j < outcomesProp.arraySize; j++)
            {
                SerializedProperty outcomeProp = outcomesProp.GetArrayElementAtIndex(j);
                SerializedProperty outcomeTextProp = outcomeProp.FindPropertyRelative("outcomeText");
                outcomeTextProp.stringValue = $"event.result.{newId}.{i + 1}.{j + 1}";
            }
        }
    }
}

// EventManagerWindow 클래스는 그대로 유지
#endif
