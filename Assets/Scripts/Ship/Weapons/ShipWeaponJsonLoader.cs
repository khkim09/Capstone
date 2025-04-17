#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class ShipWeaponJsonLoader : EditorWindow
{
    private string jsonFilePath = "Assets/StreamingAssets/Weapons.json";
    private string effectJsonPath = "Assets/StreamingAssets/WeaponEffects.json";
    private string outputFolder = "Assets/ScriptableObjects/Weapons";
    private ShipWeaponDatabase databaseAsset;
    private bool overwriteExisting = true;

    private string spriteFolderPath = "Weapons";

    private int updatedWeapons;
    private int newWeapons;

    [MenuItem("Game/JSON to Weapon Converter")]
    public static void ShowWindow()
    {
        GetWindow<ShipWeaponJsonLoader>("JSON to Weapon Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert Weapon JSON to ScriptableObjects", EditorStyles.boldLabel);

        if (GUILayout.Button("Select Weapon JSON File"))
            jsonFilePath = EditorUtility.OpenFilePanel("Select Weapon JSON File", "", "json,txt");

        EditorGUILayout.LabelField("Weapon File:", jsonFilePath);

        if (GUILayout.Button("Select Effect JSON File"))
            effectJsonPath = EditorUtility.OpenFilePanel("Select Effect JSON File", "", "json,txt");

        EditorGUILayout.LabelField("Effect File:", effectJsonPath);

        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);
        spriteFolderPath = EditorGUILayout.TextField("Sprite Folder (in Resources):", spriteFolderPath);

        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);

        if (overwriteExisting)
            EditorGUILayout.HelpBox("Warning: Existing assets will be overwritten.", MessageType.Warning);

        databaseAsset =
            EditorGUILayout.ObjectField("Weapon Database:", databaseAsset, typeof(ShipWeaponDatabase), false) as
                ShipWeaponDatabase;

        if (GUILayout.Button("Create New Database")) CreateNewDatabase();

        GUI.enabled = !string.IsNullOrEmpty(jsonFilePath) && !string.IsNullOrEmpty(effectJsonPath) &&
                      databaseAsset != null;

        if (GUILayout.Button("Convert JSON to ScriptableObjects")) ConvertJsonToScriptableObjects();

        GUI.enabled = true;

        if (updatedWeapons > 0 || newWeapons > 0)
        {
            GUILayout.Label("Conversion Results:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Updated Weapons:", updatedWeapons.ToString());
            EditorGUILayout.LabelField("New Weapons:", newWeapons.ToString());
            EditorGUILayout.LabelField("Total Weapons:", databaseAsset.allWeapons.Count.ToString());
        }
    }

    private void CreateNewDatabase()
    {
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        ShipWeaponDatabase newDatabase = CreateInstance<ShipWeaponDatabase>();
        string assetPath = Path.Combine(outputFolder, "WeaponDatabase.asset");
        AssetDatabase.CreateAsset(newDatabase, assetPath);
        AssetDatabase.SaveAssets();

        databaseAsset = newDatabase;
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newDatabase;
    }

    private class EffectInfo
    {
        public string type;
        public string description;
    }

    private Dictionary<int, EffectInfo> LoadEffectMap(string path)
    {
        Dictionary<int, EffectInfo> map = new();
        string json = File.ReadAllText(path);
        JArray array = JArray.Parse(json);

        foreach (JToken item in array)
        {
            int id = (int)item["id"];
            string type = (string)item["type"];
            string desc = (string)item["description"];

            map[id] = new EffectInfo { type = type, description = desc };
        }

        return map;
    }

    private void ConvertJsonToScriptableObjects()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || string.IsNullOrEmpty(effectJsonPath) || databaseAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "JSON 파일과 Database 에셋을 모두 지정해 주세요.", "OK");
            return;
        }

        string jsonText = File.ReadAllText(jsonFilePath);
        Dictionary<int, EffectInfo> effectMap = LoadEffectMap(effectJsonPath);

        try
        {
            JObject jsonObj = JObject.Parse(jsonText);

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            Undo.RecordObject(databaseAsset, "Update Weapon Database");

            Dictionary<int, ShipWeaponData> existingWeapons = new();
            if (overwriteExisting)
                foreach (ShipWeaponData item in databaseAsset.allWeapons)
                    if (item != null)
                        existingWeapons[item.id] = item;

            if (!overwriteExisting)
                databaseAsset.allWeapons.Clear();

            updatedWeapons = 0;
            newWeapons = 0;

            foreach (KeyValuePair<string, JToken> pair in jsonObj)
            {
                string key = pair.Key;
                JToken weaponToken = pair.Value;

                int weaponId = (int)weaponToken["id"];
                string weaponName = (string)weaponToken["name"];
                string assetPath = Path.Combine(outputFolder, $"Weapon_{weaponId}.asset");

                ShipWeaponData weaponSO = null;
                bool isNew = true;

                if (overwriteExisting && existingWeapons.TryGetValue(weaponId, out ShipWeaponData existing))
                {
                    weaponSO = existing;
                    assetPath = AssetDatabase.GetAssetPath(weaponSO);
                    Undo.RecordObject(weaponSO, "Update Weapon");
                    isNew = false;
                    updatedWeapons++;
                }
                else
                {
                    weaponSO = CreateInstance<ShipWeaponData>();
                    newWeapons++;
                }

                weaponSO.id = weaponId;
                weaponSO.weaponName = weaponName;
                weaponSO.description = (string)weaponToken["description"];
                weaponSO.damage = (float)(double)weaponToken["damage"];
                weaponSO.cooldownPerSecond = (float)(double)weaponToken["cooldown_per_second"];
                weaponSO.cost = (int)weaponToken["cost"];

                string weaponTypeStr = (string)weaponToken["type"];
                if (Enum.TryParse<ShipWeaponType>(weaponTypeStr, true, out ShipWeaponType weaponType))
                    weaponSO.weaponType = weaponType;

                string warheadTypeStr = (string)weaponToken["warhead_type"];
                if (Enum.TryParse<WarheadType>(warheadTypeStr, true, out WarheadType warheadType))
                    weaponSO.warheadType = warheadType;

                if (weaponToken["effect_id"] != null)
                {
                    int effectId = (int)weaponToken["effect_id"];


                    if (effectMap.TryGetValue(effectId, out EffectInfo effectInfo))
                    {
                        if (Enum.TryParse<ShipWeaponEffectType>(effectInfo.type, true,
                                out ShipWeaponEffectType parsedEffect))
                            weaponSO.effectType = parsedEffect;

                        weaponSO.effectDescription = effectInfo.description;
                    }
                }

                if (weaponToken["effect_power"] != null)
                    weaponSO.effectPower = (float)(double)weaponToken["effect_power"];

                Sprite sprite = Resources.Load<Sprite>($"{spriteFolderPath}/weapon_{weaponId}");
                if (sprite != null)
                    weaponSO.weaponSprite = sprite;

                string[] directions = { "north", "east", "south" };
                for (int i = 0; i < 3; i++)
                {
                    Sprite dirSprite = Resources.Load<Sprite>($"{spriteFolderPath}/weapon_{weaponId}_{directions[i]}");
                    if (dirSprite != null)
                        weaponSO.rotationSprites[i] = dirSprite;
                    else if (i == 1 && weaponSO.weaponSprite != null)
                        weaponSO.rotationSprites[i] = weaponSO.weaponSprite;
                }

                if (isNew)
                {
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CreateAsset(weaponSO, assetPath);
                    databaseAsset.allWeapons.Add(weaponSO);
                }
                else
                {
                    EditorUtility.SetDirty(weaponSO);
                }
            }

            EditorUtility.SetDirty(databaseAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                $"변환 완료!\n업데이트: {updatedWeapons}, 새 무기: {newWeapons}, 총: {databaseAsset.allWeapons.Count}", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"변환 실패: {e.Message}", "OK");
            Debug.LogError(e);
        }
    }
}
#endif
