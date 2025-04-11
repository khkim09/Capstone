#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class JsonToScriptableObjectConverter : EditorWindow
{
    private string jsonFilePath = "Assets/StreamingAssets/TradableItem.json";
    private string outputFolder = "Assets/ScriptableObject/Items";
    private TradingItemDataBase databaseAsset;
    private bool overwriteExisting = true; // 덮어씌우기 옵션 추가

    [MenuItem("Game/JSON to Item Converter")]
    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableObjectConverter>("JSON to Item Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert JSON to ScriptableObjects", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Select JSON File"))
            jsonFilePath = EditorUtility.OpenFilePanel("Select JSON File", "", "json,txt");

        EditorGUILayout.LabelField("Selected File:", jsonFilePath);

        EditorGUILayout.Space();

        outputFolder = EditorGUILayout.TextField("Output Folder:", outputFolder);

        if (!Directory.Exists(outputFolder))
            EditorGUILayout.HelpBox("Output folder does not exist. It will be created.", MessageType.Info);

        EditorGUILayout.Space();

        // 덮어씌우기 옵션 UI 추가
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Assets", overwriteExisting);

        if (overwriteExisting)
            EditorGUILayout.HelpBox(
                "Warning: This will overwrite any existing assets. Any manual changes to existing assets will be lost.",
                MessageType.Warning);

        EditorGUILayout.Space();

        // 기존 데이터베이스 선택 또는 새로 생성
        databaseAsset =
            EditorGUILayout.ObjectField("Item Database:", databaseAsset, typeof(TradingItemDataBase), false) as
                TradingItemDataBase;

        if (GUILayout.Button("Create New Database")) CreateNewDatabase();

        EditorGUILayout.Space();

        GUI.enabled = !string.IsNullOrEmpty(jsonFilePath) && databaseAsset != null;

        if (GUILayout.Button("Convert JSON to ScriptableObjects")) ConvertJsonToScriptableObjects();

        GUI.enabled = true;
    }

    private void CreateNewDatabase()
    {
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

        TradingItemDataBase newDatabase = CreateInstance<TradingItemDataBase>();
        string assetPath = Path.Combine(outputFolder, "ItemDatabase.asset");
        AssetDatabase.CreateAsset(newDatabase, assetPath);
        AssetDatabase.SaveAssets();

        databaseAsset = newDatabase;
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newDatabase;
    }

    private void ConvertJsonToScriptableObjects()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || databaseAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a JSON file and an Item Database asset.", "OK");
            return;
        }

        string jsonText = File.ReadAllText(jsonFilePath);

        try
        {
            // Newtonsoft.Json으로 JSON 파싱 (Dictionary 형태)
            JObject jsonObj = JObject.Parse(jsonText);

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            // 데이터베이스 업데이트 준비
            Undo.RecordObject(databaseAsset, "Update Item Database");

            // 데이터베이스를 비우기 전에 기존 아이템 참조를 캐시하기
            Dictionary<int, TradingItemData> existingItems = new();
            if (overwriteExisting)
                foreach (TradingItemData item in databaseAsset.allItems)
                    if (item != null)
                        existingItems[item.id] = item;

            // 데이터베이스 비우기
            databaseAsset.allItems.Clear();

            // 수정된 아이템과 새 아이템 카운트
            int updatedCount = 0;
            int newCount = 0;

            foreach (KeyValuePair<string, JToken> pair in jsonObj)
            {
                string key = pair.Key;
                JToken itemToken = pair.Value;

                int itemId = (int)itemToken["id"];
                string debug_name = (string)itemToken["debug_name"];
                string itemName = string.IsNullOrEmpty(debug_name) ? $"Item_{itemId}" : debug_name;
                string assetPath = Path.Combine(outputFolder, $"{itemName}.asset");

                TradingItemData itemSO = null;
                bool isNewItem = true;

                // 덮어씌우기가 활성화되었고 해당 ID를 가진 아이템이 있으면 사용
                if (overwriteExisting && existingItems.TryGetValue(itemId, out TradingItemData existingItem))
                {
                    itemSO = existingItem;
                    assetPath = AssetDatabase.GetAssetPath(itemSO); // 이 줄 추가
                    Undo.RecordObject(itemSO, "Update Item Data");
                    isNewItem = false;
                    updatedCount++;
                }

                else
                {
                    itemSO = CreateInstance<TradingItemData>();
                    newCount++;
                }

                // JSON 데이터를 스크립터블 오브젝트로 매핑
                itemSO.id = itemId;
                itemSO.planet = Enum.TryParse<ItemPlanet>((string)itemToken["planet"], out ItemPlanet parsedPlanet)
                    ? parsedPlanet
                    : ItemPlanet.Default;
                itemSO.itemName = (string)itemToken["name"];
                itemSO.debugName = debug_name;
                itemSO.tier = Enum.TryParse<ItemTierLevel>((string)itemToken["tier"], out ItemTierLevel parsedTier)
                    ? parsedTier
                    : ItemTierLevel.Default;
                itemSO.type = Enum.TryParse<ItemCategory>((string)itemToken["type"], out ItemCategory parsedType)
                    ? parsedType
                    : ItemCategory.Default;
                itemSO.temperatureMin = (float)(double)itemToken["temperature_min"];
                itemSO.temperatureMax = (float)(double)itemToken["temperature_max"];
                itemSO.shape = (int)itemToken["shape"];
                itemSO.costBase = (int)itemToken["cost_base"];
                itemSO.capacity = (int)itemToken["capacity"];
                itemSO.costMin = (int)itemToken["cost_min"];
                itemSO.costChangerate = (float)(double)itemToken["cost_changerate"];
                itemSO.costMax = (int)itemToken["cost_max"];
                itemSO.description = (string)itemToken["description"];

                string spriteSheetName = "Goods"; // 고정 시트 이름
                string spriteName = $"{spriteSheetName}_{itemId}";

                Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/Item/{spriteSheetName}");
                Sprite targetSprite = Array.Find(sprites, s => s.name == spriteName);

                if (targetSprite == null)
                    Debug.LogWarning($"스프라이트 {spriteName} 을 찾을 수 없습니다.");
                else
                    itemSO.itemSprite = targetSprite;

                // 새 아이템일 경우만 에셋 생성
                if (isNewItem)
                {
                    // 중복 파일 체크
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CreateAsset(itemSO, assetPath);
                }
                else
                {
                    // 기존 아이템 업데이트
                    EditorUtility.SetDirty(itemSO);
                }

                // 데이터베이스에 아이템 추가
                databaseAsset.allItems.Add(itemSO);
            }

            EditorUtility.SetDirty(databaseAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                $"JSON 변환 완료!\n업데이트된 아이템: {updatedCount}\n새 아이템: {newCount}\n총 아이템: {databaseAsset.allItems.Count}",
                "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to convert JSON: {e.Message}", "OK");
            Debug.LogError($"JSON conversion error: {e}");
        }
    }
}
#endif
