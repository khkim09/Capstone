#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// JSON 으로 정의된 장비를 자동으로 ScriptableObject 로 Import 해주는 툴
/// </summary>
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

    /// <summary>
    /// 문자열 location을 ItemPlanet enum으로 변환합니다.
    /// </summary>
    /// <param name="locationString">location 문자열</param>
    /// <returns>변환된 ItemPlanet enum</returns>
    private ItemPlanet ParseLocationToPlanet(string locationString)
    {
        if (string.IsNullOrEmpty(locationString))
            return ItemPlanet.Default;

        // 대소문자 구분 없이 매칭
        string location = locationString.Trim().ToUpper();

        switch (location)
        {
            case "SIS":
                return ItemPlanet.SIS;
            case "CCK":
                return ItemPlanet.CCK;
            case "ICM":
                return ItemPlanet.ICM;
            case "RCE":
                return ItemPlanet.RCE;
            case "KTL":
                return ItemPlanet.KTL;
            case "ALL":
                return ItemPlanet.ALL;
            default:
                Debug.LogWarning($"알 수 없는 location 값: {locationString}. Default로 설정됩니다.");
                return ItemPlanet.Default;
        }
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
                equipSO.eqDescription = equipToken["description"] != null ? (string)equipToken["description"] : "";

                // TODO : 나중엔 CSV 에 Global 장비 여부도 추가 후, 여기서 설정을 해줘야 한다.
                equipSO.isGlobalEquip = equipToken["is_global"] != null
                    ? (bool)equipToken["is_global"]
                    : equipType != EquipmentType.AssistantEquipment; // 기본값 설정

                // Location을 ItemPlanet으로 변환하여 설정
                string locationString = equipToken["location"] != null ? (string)equipToken["location"] : "";
                equipSO.planet = ParseLocationToPlanet(locationString);

                // 공격/방어 보너스 설정
                equipSO.eqAttackBonus =
                    equipToken["attack"] != null ? (int)equipToken["attack"] : 0;
                equipSO.eqDefenseBonus =
                    equipToken["defense"] != null ? (int)equipToken["defense"] : 0;
                equipSO.eqHealthBonus =
                    equipToken["hitpoint"] != null ? (int)equipToken["hitpoint"] : 0;

                // 어시스턴트 장비인 경우 스킬 보너스 설정
                if (equipType == EquipmentType.AssistantEquipment)
                {
                    equipSO.eqAdditionalPilotSkill = equipToken["pilot"] != null
                        ? (int)equipToken["pilot"]
                        : 0;
                    equipSO.eqAdditionalEngineSkill = equipToken["engine"] != null
                        ? (int)equipToken["engine"]
                        : 0;
                    equipSO.eqAdditionalPowerSkill = equipToken["power"] != null
                        ? (int)equipToken["power"]
                        : 0;
                    equipSO.eqAdditionalShieldSkill = equipToken["shield"] != null
                        ? (int)equipToken["shield"]
                        : 0;
                    equipSO.eqAdditionalWeaponSkill = equipToken["weapon"] != null
                        ? (int)equipToken["weapon"]
                        : 0;
                    // CSV에서는 weaponcontrol로 되어있지만, JSON에서는 weapon으로 처리

                    equipSO.eqAdditionalAmmunitionSkill = equipToken["ammunition"] != null
                        ? (int)equipToken["ammunition"]
                        : 0;
                    equipSO.eqAdditionalMedBaySkill = equipToken["medbay"] != null
                        ? (int)equipToken["medbay"]
                        : 0;
                    equipSO.eqAdditionalRepairSkill = equipToken["repair"] != null
                        ? (int)equipToken["repair"]
                        : 0;
                }

                // 아이콘 로드 - slice된 스프라이트에서 이름으로 찾기
                string equipmentFileName = "";
                switch (equipType)
                {
                    case EquipmentType.WeaponEquipment:
                        equipmentFileName = "EquipmentWeapon";
                        break;
                    case EquipmentType.ShieldEquipment:
                        equipmentFileName = "EquipmentShield";
                        break;
                    case EquipmentType.AssistantEquipment:
                        equipmentFileName = "EquipmentAssistant";
                        break;
                }

                Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/Item/{equipmentFileName}");

                if (sprites != null && sprites.Length > 0)
                {
                    // 스프라이트 이름 매칭 (equipId를 직접 사용)
                    string spriteName = $"{equipmentFileName}_{equipId}";
                    Sprite targetSprite = Array.Find(sprites, s => s.name == spriteName);

                    if (targetSprite != null)
                        equipSO.eqIcon = targetSprite;
                    else
                        Debug.LogWarning(
                            $"장비 스프라이트를 찾을 수 없습니다. 찾은 이름: {spriteName}, 사용 가능한 스프라이트: {string.Join(", ", Array.ConvertAll(sprites, s => s.name))}");
                }
                else
                {
                    Debug.LogWarning($"장비 스프라이트를 찾을 수 없습니다: Sprites/Item/{equipmentFileName}");
                }

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
