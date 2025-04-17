#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Unity 에디터에서 함선 템플릿을 관리하기 위한 에디터 윈도우
/// </summary>
public class ShipTemplateEditor : EditorWindow
{
    private Ship targetShip;
    private string templateId = "new_template";
    private string displayName = "New Template";
    private string description = "A new ship template";
    private string category = "Default";
    private int difficulty = 1;

    private Vector2 scrollPosition;
    private bool previewTemplate = false;

    private string[] difficultyLabels = { "Very Easy (1)", "Easy (2)", "Normal (3)", "Hard (4)", "Very Hard (5)" };

    [MenuItem("Tools/Ship Template Editor")]
    public static void ShowWindow()
    {
        GetWindow<ShipTemplateEditor>("Ship Template Editor");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("Ship Template Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        DrawTargetShipSection();

        EditorGUILayout.Space(20);
        DrawTemplateMetadataSection();

        EditorGUILayout.Space(20);
        DrawTemplateActionsSection();

        EditorGUILayout.Space(20);
        DrawTemplateLoadSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawTargetShipSection()
    {
        EditorGUILayout.LabelField("Target Ship", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        targetShip = (Ship)EditorGUILayout.ObjectField("Ship Object", targetShip, typeof(Ship), true);
        EditorGUI.EndChangeCheck();

        if (targetShip == null)
            EditorGUILayout.HelpBox("Please assign a Ship object to create or load templates.", MessageType.Warning);
    }

    private void DrawTemplateMetadataSection()
    {
        EditorGUILayout.LabelField("Template Metadata", EditorStyles.boldLabel);

        templateId = EditorGUILayout.TextField("Template ID", templateId);
        if (string.IsNullOrEmpty(templateId))
            EditorGUILayout.HelpBox("Template ID cannot be empty.", MessageType.Error);

        displayName = EditorGUILayout.TextField("Display Name", displayName);

        description = EditorGUILayout.TextArea(description, GUILayout.Height(60));

        category = EditorGUILayout.TextField("Category", category);

        difficulty = EditorGUILayout.Popup("Difficulty", difficulty - 1, difficultyLabels) + 1;
    }

    private void DrawTemplateActionsSection()
    {
        EditorGUILayout.LabelField("Template Actions", EditorStyles.boldLabel);

        GUI.enabled = targetShip != null && !string.IsNullOrEmpty(templateId);

        if (GUILayout.Button("Create Template from Ship")) CreateTemplateFromShip();

        GUILayout.Space(10);

        if (GUILayout.Button("Save Ship to File")) SaveShipToFile();

        if (GUILayout.Button("Load Ship from File")) LoadShipFromFile();

        GUI.enabled = true;
    }

    private void DrawTemplateLoadSection()
    {
        EditorGUILayout.LabelField("Load Template", EditorStyles.boldLabel);

        if (ShipTemplateSystem.Instance == null)
        {
            EditorGUILayout.HelpBox("ShipTemplateSystem not found in scene. Some features may not work.",
                MessageType.Warning);

            if (GUILayout.Button("Create ShipTemplateSystem")) CreateShipTemplateSystem();

            return;
        }

        // 카테고리 및 템플릿 목록
        Dictionary<string, List<ShipTemplateSystem.ShipTemplateMeta>> allTemplates = new();

        foreach (string cat in ShipTemplateSystem.Instance.GetAllCategories())
            allTemplates[cat] = ShipTemplateSystem.Instance.GetTemplatesInCategory(cat);

        // 미리보기 토글
        previewTemplate = EditorGUILayout.Toggle("Preview Templates", previewTemplate);

        foreach (string category in allTemplates.Keys)
        {
            EditorGUILayout.LabelField($"Category: {category}", EditorStyles.boldLabel);

            foreach (ShipTemplateSystem.ShipTemplateMeta template in allTemplates[category])
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField($"{template.displayName} (Difficulty: {template.difficulty})");

                GUI.enabled = targetShip != null;
                if (GUILayout.Button("Load", GUILayout.Width(100))) LoadTemplateToShip(template.templateId);
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                if (previewTemplate)
                {
                    EditorGUILayout.LabelField($"ID: {template.templateId}");
                    EditorGUILayout.LabelField($"Description: {template.description}");
                    EditorGUILayout.LabelField($"Path: {template.jsonPath}");
                    EditorGUILayout.Space(5);
                }
            }

            EditorGUILayout.Space(10);
        }

        if (GUILayout.Button("Reload All Templates")) ShipTemplateSystem.Instance.ReloadAllTemplates();
    }

    private void CreateTemplateFromShip()
    {
        if (targetShip == null || string.IsNullOrEmpty(templateId))
            return;

        bool success = ShipTemplateSystem.Instance.CreateTemplateFromShip(
            targetShip, templateId, displayName, description, category, difficulty);

        if (success)
            EditorUtility.DisplayDialog("Success", $"Template '{displayName}' created successfully.", "OK");
        else
            EditorUtility.DisplayDialog("Error", "Failed to create template.", "OK");
    }

    private void SaveShipToFile()
    {
        if (targetShip == null)
            return;

        string path = EditorUtility.SaveFilePanel(
            "Save Ship Template",
            Application.dataPath,
            $"{templateId}.json",
            "json");

        if (string.IsNullOrEmpty(path))
            return;

        bool success = targetShip.SaveToFile(path);

        if (success)
            EditorUtility.DisplayDialog("Success", $"Ship saved to {path}", "OK");
        else
            EditorUtility.DisplayDialog("Error", "Failed to save ship.", "OK");
    }

    private void LoadShipFromFile()
    {
        if (targetShip == null)
            return;

        string path = EditorUtility.OpenFilePanel(
            "Load Ship Template",
            Application.dataPath,
            "json");

        if (string.IsNullOrEmpty(path))
            return;

        bool success = targetShip.LoadFromFile(path);

        if (success)
            EditorUtility.DisplayDialog("Success", $"Ship loaded from {path}", "OK");
        else
            EditorUtility.DisplayDialog("Error", "Failed to load ship.", "OK");
    }

    private void LoadTemplateToShip(string templateId)
    {
        if (targetShip == null)
            return;

        bool success = targetShip.LoadFromTemplate(templateId);

        if (success)
            EditorUtility.DisplayDialog("Success", $"Template '{templateId}' loaded successfully.", "OK");
        else
            EditorUtility.DisplayDialog("Error", $"Failed to load template '{templateId}'.", "OK");
    }

    private void CreateShipTemplateSystem()
    {
        GameObject go = new("ShipTemplateSystem");
        go.AddComponent<ShipTemplateSystem>();
        Selection.activeGameObject = go;
        EditorUtility.DisplayDialog("Success", "ShipTemplateSystem created.", "OK");
    }
}
#endif
