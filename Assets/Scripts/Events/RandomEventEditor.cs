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
    private bool[] outcomeFoldouts;

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
        choicesProp = serializedObject.FindProperty("choices");

        InitializeFoldouts();
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

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(eventIdProp, new GUIContent("이벤트 ID"));
        if (EditorGUI.EndChangeCheck()) // 변경이 있으면
        {
            // ID가 변경되었을 때 타이틀과 설명을 자동으로 업데이트
            int newId = eventIdProp.intValue; // int 값으로 가져옴
            eventTitleProp.stringValue = $"event.name.{newId}";
            eventDescriptionProp.stringValue = $"event.description.{newId}";
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
                    float defaultProb = 100f / outcomesProp.arraySize;
                    for (int j = 0; j < outcomesProp.arraySize; j++)
                    {
                        SerializedProperty probProp =
                            outcomesProp.GetArrayElementAtIndex(j).FindPropertyRelative("probability");
                        probProp.floatValue = defaultProb;
                    }
                }

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

            Debug.Log($"JSON에서 이벤트를 가져왔습니다: {path}");
        }
    }
}

// EventManagerWindow 클래스는 그대로 유지
#endif
