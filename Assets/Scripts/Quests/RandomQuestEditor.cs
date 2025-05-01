#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RandomQuest))]
public class RandomQuestEditor : Editor
{
    SerializedProperty questIdProp;
    SerializedProperty statusProp;
    SerializedProperty titleProp;
    SerializedProperty descProp;
    SerializedProperty objectivesProp;
    SerializedProperty rewardsProp;

    bool[] objectiveFoldouts;
    bool[] rewardFoldouts;

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

    private void InitializeFoldouts()
    {
        objectiveFoldouts = new bool[objectivesProp.arraySize];
        rewardFoldouts = new bool[rewardsProp.arraySize];
    }

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
                        EditorGUILayout.PropertyField(destinationPlanetIdProp, new GUIContent("목표 행성 ID")); // ✅ 표시됨
                        EditorGUILayout.PropertyField(reqProp, new GUIContent("필요 수량"));
                        break;

                    case RandomQuest.QuestObjectiveType.ItemProcurement:
                        EditorGUILayout.PropertyField(targetIdProp, new GUIContent("대상 아이템 ID"));
                        EditorGUILayout.PropertyField(reqProp, new GUIContent("필요 수량")); // 목적 행성 없음
                        break;

                    case RandomQuest.QuestObjectiveType.CrewTransport:
                        EditorGUILayout.PropertyField(killCountProp, new GUIContent("임시 선원 수"));
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
