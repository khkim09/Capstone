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
    private string jsonFilePath = "Assets/StreamingAssets/ShipWeapon.json";
    private string effectJsonPath = "Assets/StreamingAssets/ShipWeaponSpecialEffect.json";
    private string outputFolder = "Assets/ScriptableObject/ShipWeapon";
    private string effectsFolder = "Assets/ScriptableObject/ShipWeapon/Effects";
    private string typesFolder = "Assets/ScriptableObject/ShipWeapon/Types";
    private string warheadFloder = "Assets/ScriptableObject/ShipWeapon/Warheads";
    private ShipWeaponDatabase databaseAsset;
    private bool overwriteExisting = true;

    private int updatedWeapons;
    private int newWeapons;
    private int updatedEffects;
    private int newEffects;

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
        effectsFolder = EditorGUILayout.TextField("Effects Folder:", effectsFolder);
        typesFolder = EditorGUILayout.TextField("Types Folder:", typesFolder);

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

        if (updatedWeapons > 0 || newWeapons > 0 || updatedEffects > 0 || newEffects > 0)
        {
            GUILayout.Label("Conversion Results:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Updated Weapons:", updatedWeapons.ToString());
            EditorGUILayout.LabelField("New Weapons:", newWeapons.ToString());
            EditorGUILayout.LabelField("Updated Effects:", updatedEffects.ToString());
            EditorGUILayout.LabelField("New Effects:", newEffects.ToString());
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
        public int id;
        public string type;
        public string name;
        public string description;
    }

    private Dictionary<int, EffectInfo> LoadEffectMap(string path)
    {
        Dictionary<int, EffectInfo> map = new();
        string json = File.ReadAllText(path);
        // top-level이 object 형태일 때
        JObject obj = JObject.Parse(json);
        foreach (JProperty prop in obj.Properties())
            // 키가 "0", "1", ... 으로 되어 있다면
            if (int.TryParse(prop.Name, out int id))
            {
                JToken item = prop.Value;
                string type = (string)item["type"];
                string desc = (string)item["description"];
                string name = (string)item["name"] ?? $"Effect {id}";

                map[id] = new EffectInfo { id = id, type = type, name = name, description = desc };
            }

        return map;
    }

    // 기존 무기 타입 데이터를 ID로 찾아서 딕셔너리로 반환
    private Dictionary<int, ShipWeaponTypeData> LoadExistingTypeAssets()
    {
        Dictionary<int, ShipWeaponTypeData> typeAssets = new();

        string[] guids = AssetDatabase.FindAssets("t:ShipWeaponTypeData", new[] { typesFolder });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ShipWeaponTypeData typeAsset = AssetDatabase.LoadAssetAtPath<ShipWeaponTypeData>(assetPath);
            if (typeAsset != null) typeAssets[typeAsset.id] = typeAsset;
        }

        return typeAssets;
    }

    private Dictionary<int, WeaponEffectData> LoadExistingEffectAssets()
    {
        Dictionary<int, WeaponEffectData> effectAssets = new();

        string[] guids = AssetDatabase.FindAssets("t:WeaponEffectData", new[] { effectsFolder });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            WeaponEffectData effectAsset = AssetDatabase.LoadAssetAtPath<WeaponEffectData>(assetPath);
            if (effectAsset != null) effectAssets[effectAsset.id] = effectAsset;
        }

        return effectAssets;
    }

    private Dictionary<int, WarheadTypeData> LoadExistingWarheadAssets()
    {
        Dictionary<int, WarheadTypeData> warheadAssets = new();

        string[] guids = AssetDatabase.FindAssets("t:WarheadTypeData", new[] { warheadFloder });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            WarheadTypeData warheadAsset = AssetDatabase.LoadAssetAtPath<WarheadTypeData>(assetPath);
            if (warheadAsset != null) warheadAssets[warheadAsset.id] = warheadAsset;
        }

        return warheadAssets;
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
        Dictionary<int, WeaponEffectData> effectAssets = LoadExistingEffectAssets();
        Dictionary<int, ShipWeaponTypeData> typeAssets = LoadExistingTypeAssets();
        Dictionary<int, WarheadTypeData> warheadAssets = LoadExistingWarheadAssets();

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

                // type_id로 무기 타입 설정
                if (weaponToken["type_id"] != null)
                {
                    int typeId = (int)weaponToken["type_id"];

                    // ShipWeaponTypeData에서 ID로 찾기
                    if (typeAssets.TryGetValue(typeId, out ShipWeaponTypeData typeData))
                        // TypeData에서 무기 타입 가져오기
                        weaponSO.weaponType = typeData;
                    // 필요하다면 ShipWeaponData에 typeData 참조 추가
                    // weaponSO.weaponTypeData = typeData;
                    else
                        Debug.LogError("함선 무기 타입 데이터가 없음");
                }
                else
                {
                    Debug.LogError("함선 무기 타입 아이디가 없음");
                }


                if (weaponToken["effect_id"] != null)
                {
                    int effectId = (int)weaponToken["effect_id"];

                    // warheadData에서 ID로 찾기
                    if (effectAssets.TryGetValue(effectId, out WeaponEffectData effectData))
                        weaponSO.effectData = effectData;
                    else
                        Debug.LogError("함선 무기 이펙트 데이터가 없음");
                }
                else
                {
                    Debug.LogError("함선 무기 이펙트 아이디가 없음");
                }

                if (weaponToken["warhead_type"] != null)
                {
                    int warheadId = (int)weaponToken["warhead_type"];

                    // warheadData에서 ID로 찾기
                    if (warheadAssets.TryGetValue(warheadId, out WarheadTypeData warheadData))
                        weaponSO.warheadType = warheadData;

                    else
                        Debug.LogError("함선 무기 탄두 데이터가 없음");
                }
                else
                {
                    Debug.LogError("함선 무기 탄두 아이디가 없음");
                }

                if (weaponToken["effect_power"] != null)
                    weaponSO.effectPower = (float)(double)weaponToken["effect_power"];

                // TODO : 무기 스프라이트는 배의 외갑판 레벨이 정해지고 나서 외갑판의 스프라이트와 함께 결정한다.

                // Sprite sprite = Resources.Load<Sprite>($"{spriteFolderPath}/weapon_{weaponId}");
                // if (sprite != null)
                //     weaponSO.weaponSprite = sprite;
                //
                // for (int direction = 0; direction < 3; direction++)
                // {
                //
                //
                //     Sprite dirSprite = Resources.Load<Sprite>($"{spriteFolderPath}/weapon_{weaponId}_{direction}");
                //     if (dirSprite != null)
                //         weaponSO.rotationSprites[direction] = dirSprite;
                //     else if (direction == 1 && weaponSO.weaponSprite != null)
                //         weaponSO.rotationSprites[direction] = weaponSO.weaponSprite;
                // }

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
                $"변환 완료!\n업데이트: {updatedWeapons}, 새 무기: {newWeapons}, 총: {databaseAsset.allWeapons.Count}\n" +
                $"업데이트 효과: {updatedEffects}, 새 효과: {newEffects}", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"변환 실패: {e.Message}", "OK");
            Debug.LogError(e);
        }
    }
}
#endif
