#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// RandomQuest ScriptableObject에 대한 커스텀 인스펙터를 제공합니다.
/// 퀘스트의 ID, 상태, 설명, 목표, 보상 등을 에디터에서 시각적으로 관리할 수 있습니다.
/// </summary>
[CustomEditor(typeof(RandomQuest))]
public class RandomQuestEditor : Editor
{
    /// <summary>퀘스트 ID 속성</summary>
    SerializedProperty questIdProp;

    /// <summary>퀘스트 상태 속성</summary>
    SerializedProperty statusProp;

    /// <summary>퀘스트 제목 속성</summary>
    SerializedProperty titleProp;

    /// <summary>퀘스트 설명 속성</summary>
    SerializedProperty descProp;

    /// <summary>퀘스트 목표 리스트 속성</summary>
    SerializedProperty objectivesProp;

    /// <summary>퀘스트 보상 리스트 속성</summary>
    SerializedProperty rewardsProp;

    /// <summary>목표 Foldout UI 상태 배열</summary>
    bool[] objectiveFoldouts;

    /// <summary>보상 Foldout UI 상태 배열</summary>
    bool[] rewardFoldouts;

    /// <summary>
    /// 인스펙터가 활성화될 때 호출되어 SerializedProperty를 초기화합니다.
    /// </summary>
    private void OnEnable()
    {
        questIdProp = serializedObject.FindProperty("questId");
        statusProp = serializedObject.FindProperty("status");
        titleProp = serializedObject.FindProperty("title");
        descProp = serializedObject.FindProperty("description");
        objectivesProp = serializedObject.FindProperty("objectives");
        rewardsProp = serializedObject.FindProperty("rewards");

        InitializeFoldouts();
    }

    /// <summary>
    /// Foldout 배열을 초기화합니다.
    /// 목표와 보상 수 만큼 Foldout 상태를 할당합니다.
    /// </summary>
    private void InitializeFoldouts()
    {
        objectiveFoldouts = new bool[objectivesProp.arraySize];
        rewardFoldouts = new bool[rewardsProp.arraySize];
    }

    /// <summary>
    /// 인스펙터 UI를 커스터마이징하여 퀘스트 속성을 편집할 수 있게 합니다.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("식별 및 상태", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(questIdProp);
        EditorGUILayout.PropertyField(statusProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(titleProp);
        EditorGUILayout.LabelField("설명");
        descProp.stringValue = EditorGUILayout.TextArea(descProp.stringValue, GUILayout.Height(60));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("조건 (Objectives)", EditorStyles.boldLabel);

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
            var objProp = objectivesProp.GetArrayElementAtIndex(i);
            var typeProp = objProp.FindPropertyRelative("objectiveType");
            var targetIdProp = objProp.FindPropertyRelative("targetId");
            var destinationPlanetIdProp = objProp.FindPropertyRelative("destinationPlanetId");
            var descOProp = objProp.FindPropertyRelative("description");
            var reqProp = objProp.FindPropertyRelative("requiredAmount");
            var killCountProp = objProp.FindPropertyRelative("killCount");
            var questCrewNumProp = objProp.FindPropertyRelative("questCrewNum");

            EditorGUILayout.BeginHorizontal();
            objectiveFoldouts[i] = EditorGUILayout.Foldout(objectiveFoldouts[i], $"목표 {i + 1}: {descOProp.stringValue}");
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
                EditorGUILayout.PropertyField(typeProp, new GUIContent("목표 타입"));
                EditorGUILayout.PropertyField(descOProp, new GUIContent("목표 설명"));

                switch ((RandomQuest.QuestObjectiveType)typeProp.enumValueIndex)
                {
                    case RandomQuest.QuestObjectiveType.PirateHunt:
                        EditorGUILayout.PropertyField(killCountProp, new GUIContent("필요 처치 수"));
                        break;

                    case RandomQuest.QuestObjectiveType.ItemTransport:
                        EditorGUILayout.PropertyField(targetIdProp, new GUIContent("대상 아이템 ID"));
                        EditorGUILayout.PropertyField(destinationPlanetIdProp, new GUIContent("목표 행성 ID"));
                        EditorGUILayout.PropertyField(reqProp, new GUIContent("필요 수량"));
                        break;

                    case RandomQuest.QuestObjectiveType.ItemProcurement:
                        EditorGUILayout.PropertyField(targetIdProp, new GUIContent("대상 아이템 ID"));
                        EditorGUILayout.PropertyField(reqProp, new GUIContent("필요 수량"));
                        break;

                    case RandomQuest.QuestObjectiveType.CrewTransport:
                        EditorGUILayout.PropertyField(questCrewNumProp, new GUIContent("임시 선원 수"));
                        EditorGUILayout.PropertyField(destinationPlanetIdProp, new GUIContent("목표 행성 ID"));
                        break;
                }

                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("보상 (COMA 고정)", EditorStyles.boldLabel);

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
                EditorGUILayout.LabelField("보상 타입: COMA (고정)");
                EditorGUILayout.PropertyField(rwProp.FindPropertyRelative("amount"), new GUIContent("수량"));
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.Space(10);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
