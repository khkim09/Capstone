using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

/// <summary>
/// 함선 템플릿 관리 시스템
/// 미리 정의된 함선 구성을 불러와 적 함선 생성 등에 활용
/// </summary>
public class ShipTemplateSystem : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스
    /// </summary>
    public static ShipTemplateSystem Instance { get; private set; }

    /// <summary>
    /// 함선 템플릿 카테고리별 저장소
    /// </summary>
    [Serializable]
    public class ShipTemplateCollection
    {
        public string categoryName;
        public List<ShipTemplateMeta> templates = new();
    }

    /// <summary>
    /// 함선 템플릿 메타데이터
    /// </summary>
    [Serializable]
    public class ShipTemplateMeta
    {
        public string templateId;
        public string displayName;
        public string description;
        public int difficulty; // 1-5 난이도
        public string previewImagePath;
        public string jsonPath; // 실제 템플릿 JSON 파일 경로
    }

    /// <summary>
    /// 카테고리별 함선 템플릿 목록
    /// </summary>
    private Dictionary<string, ShipTemplateCollection> templateCollections = new();

    /// <summary>
    /// 템플릿 베이스 경로 (Resources 폴더 내)
    /// </summary>
    private const string TemplateBasePath = "ShipTemplates";

    /// <summary>
    /// 저장된 플레이어 함선 경로
    /// </summary>
    private string playerShipSavePath;

    private void Awake()
    {
        // 싱글톤 패턴 적용
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 저장 경로 설정
            playerShipSavePath = Path.Combine(Application.persistentDataPath, "PlayerShip.json");

            // 템플릿 초기화
            LoadAllTemplateCollections();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 모든 함선 템플릿 컬렉션 불러오기
    /// </summary>
    private void LoadAllTemplateCollections()
    {
        templateCollections.Clear();

        // Resources 폴더에서 모든 컬렉션 JSON 파일 불러오기
        TextAsset[] collectionAssets = Resources.LoadAll<TextAsset>($"{TemplateBasePath}/Collections");

        foreach (TextAsset asset in collectionAssets)
            try
            {
                ShipTemplateCollection collection = JsonConvert.DeserializeObject<ShipTemplateCollection>(asset.text);
                if (collection != null && !string.IsNullOrEmpty(collection.categoryName))
                {
                    templateCollections[collection.categoryName] = collection;
                    Debug.Log($"함선 템플릿 컬렉션 로드: {collection.categoryName}, 템플릿 수: {collection.templates.Count}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"템플릿 컬렉션 로드 오류: {asset.name} - {e.Message}");
            }
    }

    /// <summary>
    /// 모든 카테고리 이름 목록 반환
    /// </summary>
    public List<string> GetAllCategories()
    {
        return new List<string>(templateCollections.Keys);
    }

    /// <summary>
    /// 특정 카테고리의 템플릿 목록 반환
    /// </summary>
    public List<ShipTemplateMeta> GetTemplatesInCategory(string category)
    {
        if (templateCollections.TryGetValue(category, out ShipTemplateCollection collection))
            return collection.templates;

        return new List<ShipTemplateMeta>();
    }

    /// <summary>
    /// 템플릿 ID로 메타데이터 찾기
    /// </summary>
    public ShipTemplateMeta GetTemplateMetaById(string templateId)
    {
        foreach (ShipTemplateCollection collection in templateCollections.Values)
        foreach (ShipTemplateMeta template in collection.templates)
            if (template.templateId == templateId)
                return template;

        return null;
    }

    /// <summary>
    /// 템플릿 ID로 함선 생성
    /// </summary>
    public bool CreateShipFromTemplate(string templateId, Ship targetShip)
    {
        ShipTemplateMeta meta = GetTemplateMetaById(templateId);
        if (meta == null)
        {
            Debug.LogError($"함선 템플릿을 찾을 수 없음: {templateId}");
            return false;
        }

        // 템플릿 JSON 파일 로드
        TextAsset templateAsset = Resources.Load<TextAsset>($"{TemplateBasePath}/{meta.jsonPath}");
        if (templateAsset == null)
        {
            Debug.LogError($"템플릿 JSON 파일을 찾을 수 없음: {meta.jsonPath}");
            return false;
        }

        // 역직렬화하여 함선 생성
        return ShipSerialization.DeserializeShip(templateAsset.text, targetShip);
    }

    /// <summary>
    /// 현재 플레이어 함선 저장
    /// </summary>
    public bool SavePlayerShip(Ship playerShip)
    {
        return ShipSerialization.SaveShipToFile(playerShip, playerShipSavePath);
    }

    /// <summary>
    /// 저장된 플레이어 함선 불러오기
    /// </summary>
    public bool LoadPlayerShip(Ship targetShip)
    {
        if (!File.Exists(playerShipSavePath))
        {
            Debug.Log("저장된 플레이어 함선 파일이 없음. 기본 함선 사용");
            return false;
        }

        return ShipSerialization.LoadShipFromFile(playerShipSavePath, targetShip);
    }

    /// <summary>
    /// 새 템플릿을 생성하고 저장 (에디터 기능)
    /// </summary>
    public bool CreateTemplateFromShip(Ship ship, string templateId, string displayName, string description,
        string category, int difficulty)
    {
#if UNITY_EDITOR
        // 메타데이터 생성
        ShipTemplateMeta meta = new()
        {
            templateId = templateId,
            displayName = displayName,
            description = description,
            difficulty = Mathf.Clamp(difficulty, 1, 5),
            jsonPath = $"Templates/{templateId}.json",
            previewImagePath = $"Previews/{templateId}.png"
        };

        // 템플릿 JSON 생성
        string json = ShipSerialization.SerializeShip(ship);

        // 저장 경로
        string resourcesPath = AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(this))
            .Replace("ShipTemplateSystem.cs", "")
            .Replace("Scripts", "Resources");

        string templatesDir = Path.Combine(resourcesPath, TemplateBasePath, "Templates");
        Directory.CreateDirectory(templatesDir);

        // 템플릿 JSON 저장
        string templatePath = Path.Combine(templatesDir, $"{templateId}.json");
        File.WriteAllText(templatePath, json);

        // 카테고리 컬렉션 업데이트
        if (!templateCollections.TryGetValue(category, out ShipTemplateCollection collection))
        {
            collection = new ShipTemplateCollection
            {
                categoryName = category, templates = new List<ShipTemplateMeta>()
            };
            templateCollections[category] = collection;
        }

        // 기존 같은 ID 템플릿 제거
        collection.templates.RemoveAll(t => t.templateId == templateId);

        // 새 템플릿 추가
        collection.templates.Add(meta);

        // 컬렉션 파일 저장
        string collectionDir = Path.Combine(resourcesPath, TemplateBasePath, "Collections");
        Directory.CreateDirectory(collectionDir);

        string collectionPath = Path.Combine(collectionDir, $"{category}Collection.json");
        string collectionJson = JsonConvert.SerializeObject(collection, Formatting.Indented);
        File.WriteAllText(collectionPath, collectionJson);

        // 애셋 데이터베이스 갱신
        AssetDatabase.Refresh();

        Debug.Log($"함선 템플릿 생성 및 저장 완료: {templateId}");
        return true;
#else
        Debug.LogError("템플릿 생성은 에디터 모드에서만 가능합니다.");
        return false;
#endif
    }

    /// <summary>
    /// 에디터용 모든 템플릿 다시 불러오기
    /// </summary>
    public void ReloadAllTemplates()
    {
        LoadAllTemplateCollections();
    }
}
