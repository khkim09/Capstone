#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class EquipmentJsonLoader : EditorWindow
{
    private string weaponJsonPath = "Assets/StreamingAssets/EquipmentWeapon.json";
    private string shieldJsonPath = "Assets/StreamingAssets/EquipmentShield.json";
    private string assistantJsonPath = "Assets/StreamingAssets/EquipmentAssistant.json";
    private string outputFolder = "Assets/ScriptableObject/Equipment";
    private EquipmentDatabase databaseAsset;
    private bool overwriteExisting = true;

    private int updatedEquipment;
    private int newEquipment;

    [MenuItem("Game/JSON to Equipment Converter")]
    public static void ShowWindow()
    {
        GetWindow<EquipmentJsonLoader>("JSON to Equipment Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert Equipment JSON to ScriptableObjects", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Select Weapon JSON File"))
            weaponJsonPath = EditorUtility.OpenFilePanel("Select Weapon Equipment JSON File", "", "json,txt");

        EditorGUILayout.LabelField("Weapon Equipment File:", weaponJsonPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Select Shield JSON File"))
            shieldJsonPath = EditorUtility.OpenFilePanel("Select Shield Equipment JSON File", "", "json,txt");

        EditorGUILayout.LabelField("Shield Equipment File:", shieldJsonPath);

        EditorGUILayout.Space();

        if (GUILayout.Button("Select Assistant JSON File"))
            assistantJsonPath = EditorUtility.OpenFilePanel("Select Assistant Equipment JSON File", "", "json,txt");

        EditorGUILayout.LabelField("Assistant Equipment File:", assistantJsonPath);

        EditorGUILayout.Space();

        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);

        if (!Directory.Exists(outputFolder))
            EditorGUILayout.HelpBox("Output folder does not exist. It will be created.", MessageType.Info);

        EditorGUILayout.Space();

        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);

        if (overwriteExisting)
            EditorGUILayout.HelpBox("Warning: Existing assets will be overwritten. Any manual changes will be lost.",
                MessageType.Warning);

        EditorGUILayout.Space();

        databaseAsset =
            EditorGUILayout.ObjectField("Equipment Database:", databaseAsset, typeof(EquipmentDatabase), false) as
                EquipmentDatabase;

        if (GUILayout.Button("Create New Database")) CreateNewDatabase();

        EditorGUILayout.Space();

        GUI.enabled = databaseAsset != null;

        if (GUILayout.Button("Convert All JSONs to ScriptableObjects"))
        {
            updatedEquipment = 0;
            newEquipment = 0;

            if (!string.IsNullOrEmpty(weaponJsonPath))
                ConvertJsonToScriptableObjects(weaponJsonPath, EquipmentType.WeaponEquipment);

            if (!string.IsNullOrEmpty(shieldJsonPath))
                ConvertJsonToScriptableObjects(shieldJsonPath, EquipmentType.ShieldEquipment);

            if (!string.IsNullOrEmpty(assistantJsonPath))
                ConvertJsonToScriptableObjects(assistantJsonPath, EquipmentType.AssistantEquipment);

            EditorUtility.DisplayDialog("Success",
                $"변환 완료!\n업데이트: {updatedEquipment}, 새 장비: {newEquipment}, 총: {databaseAsset.allEquipments.Count}",
                "OK");
        }

        GUI.enabled = true;

        if (updatedEquipment > 0 || newEquipment > 0)
        {
            GUILayout.Label("Conversion Results:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Updated Equipment:", updatedEquipment.ToString());
            EditorGUILayout.LabelField("New Equipment:", newEquipment.ToString());
            EditorGUILayout.LabelField("Total Equipment:", databaseAsset.allEquipments.Count.ToString());
        }
    }

    private void CreateNewDatabase()
    {
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        EquipmentDatabase newDatabase = CreateInstance<EquipmentDatabase>();
        string assetPath = Path.Combine(outputFolder, "EquipmentDatabase.asset");
        AssetDatabase.CreateAsset(newDatabase, assetPath);
        AssetDatabase.SaveAssets();

        databaseAsset = newDatabase;
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newDatabase;
    }

    private void ConvertJsonToScriptableObjects(string jsonFilePath, EquipmentType equipType)
    {
        if (string.IsNullOrEmpty(jsonFilePath) || databaseAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "JSON 파일과 Database 에셋을 모두 지정해 주세요.", "OK");
            return;
        }

        if (!File.Exists(jsonFilePath))
        {
            EditorUtility.DisplayDialog("Error", $"파일을 찾을 수 없습니다: {jsonFilePath}", "OK");
            return;
        }

        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            JObject jsonObj = JObject.Parse(jsonText);

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            Undo.RecordObject(databaseAsset, "Update Equipment Database");

            Dictionary<int, EquipmentItem> existingEquipments = new();
            if (overwriteExisting)
                foreach (EquipmentItem item in databaseAsset.allEquipments)
                    if (item != null && item.eqType == equipType)
                        // 여기서는 ID 필드가 없으므로 이름을 기준으로 함
                        existingEquipments.Add(item.GetInstanceID(), item);

            string equipTypeName = equipType.ToString();

            foreach (KeyValuePair<string, JToken> pair in jsonObj)
            {
                string key = pair.Key;
                JToken equipToken = pair.Value;

                int equipId = int.Parse(key); // JSON 키를 ID로 사용
                string equipName = (string)equipToken["name"];
                string assetPath = Path.Combine(outputFolder, $"{equipTypeName}_{equipId}.asset");

                EquipmentItem equipSO = null;
                bool isNew = true;

                // 동일한 이름을 가진 장비 찾기
                foreach (EquipmentItem existingEquip in existingEquipments.Values)
                    if (existingEquip.eqName == equipName && existingEquip.eqType == equipType)
                    {
                        equipSO = existingEquip;
                        assetPath = AssetDatabase.GetAssetPath(equipSO);
                        Undo.RecordObject(equipSO, "Update Equipment");
                        isNew = false;
                        updatedEquipment++;
                        break;
                    }

                if (equipSO == null)
                {
                    equipSO = CreateInstance<EquipmentItem>();
                    newEquipment++;
                }

                // 기본 정보 설정
                equipSO.eqId = equipId;
                equipSO.eqName = equipName;
                equipSO.eqType = equipType;
                equipSO.eqPrice = equipToken["cost"] != null ? (int)equipToken["cost"] : 0;
                equipSO.isGlobalEquip = equipToken["is_global"] != null
                    ? (bool)equipToken["is_global"]
                    : equipType != EquipmentType.AssistantEquipment; // 기본값 설정

                // 공격/방어 보너스 설정
                equipSO.eqAttackBonus =
                    equipToken["attack"] != null ? (float)(double)equipToken["attack"] : 0f;
                equipSO.eqDefenseBonus =
                    equipToken["defense"] != null ? (float)(double)equipToken["defense"] : 0f;
                equipSO.eqHealthBonus =
                    equipToken["hitpoint"] != null ? (float)(double)equipToken["hitpoint"] : 0f;

                // 어시스턴트 장비인 경우 스킬 보너스 설정
                if (equipType == EquipmentType.AssistantEquipment)
                {
                    equipSO.eqAdditionalPilotSkill = equipToken["pilot"] != null
                        ? (float)(double)equipToken["pilot"]
                        : 0f;
                    equipSO.eqAdditionalEngineSkill = equipToken["engine"] != null
                        ? (float)(double)equipToken["engine"]
                        : 0f;
                    equipSO.eqAdditionalPowerSkill = equipToken["power"] != null
                        ? (float)(double)equipToken["power"]
                        : 0f;
                    equipSO.eqAdditionalShieldSkill = equipToken["shield"] != null
                        ? (float)(double)equipToken["shield"]
                        : 0f;
                    equipSO.eqAdditionalWeaponSkill = equipToken["weapon"] != null
                        ? (float)(double)equipToken["weapon"]
                        : 0f;
                    equipSO.eqAdditionalAmmunitionSkill = equipToken["ammunition"] != null
                        ? (float)(double)equipToken["ammunition"]
                        : 0f;
                    equipSO.eqAdditionalMedBaySkill = equipToken["medbay"] != null
                        ? (float)(double)equipToken["medbay"]
                        : 0f;
                    equipSO.eqAdditionalRepairSkill = equipToken["repair"] != null
                        ? (float)(double)equipToken["repair"]
                        : 0f;
                }

                // 아이콘 로드
                string iconPath = $"Sprites/Equipment/{equipTypeName}_{equipId}";
                Sprite sprite = Resources.Load<Sprite>(iconPath);
                if (sprite != null)
                    equipSO.eqIcon = sprite;
                else
                    Debug.LogWarning($"장비 아이콘을 찾을 수 없습니다: {iconPath}");

                if (isNew)
                {
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CreateAsset(equipSO, assetPath);
                    databaseAsset.allEquipments.Add(equipSO);
                }
                else
                {
                    EditorUtility.SetDirty(equipSO);
                }
            }

            EditorUtility.SetDirty(databaseAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"변환 실패: {e.Message}", "OK");
            Debug.LogError($"JSON 변환 에러: {e}");
        }
    }
}
#endif
