using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class BlueprintSlotSaveWrapper
{
    public List<BlueprintSaveData> blueprintSlots;
    public List<List<Vector2Int>> occupiedTilesPerSlot;
    public List<bool> isValidBP;
}

/// <summary>
/// 도안 슬롯 저장소 (최대 4개).
/// Scene 전역에서 접근 가능
/// </summary>
public class BlueprintSlotManager : MonoBehaviour
{
    public static BlueprintSlotManager Instance;

    ///<summary>
    /// 현재 선택된 슬롯 index (0~3)
    /// </summary>
    public int currentSlotIndex = -1;

    /// <summary>
    /// 현재 유저 함선으로 적용 중인 슬롯 index (0번에 기본 함선 주어질 것이기 때문)
    /// </summary>
    public int appliedSlotIndex = 0;

    ///<summary>
    /// 슬롯별 저장된 설계도
    /// </summary>
    public List<BlueprintSaveData> blueprintSlots = new(4);

    /// <summary>
    /// 슬롯 별 그리드 관리
    /// </summary>
    public List<HashSet<Vector2Int>> occupiedTilesPerSlot = new(4);

    /// <summary>
    /// 유효한 도안인지 (즉시 함선 변경 가능)
    /// </summary>
    public List<bool> isValidBP = new(4);

    [SerializeField] private GameObject customize0Panel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        // 4칸 초기화
        for (int i = blueprintSlots.Count; i < 4; i++)
            blueprintSlots.Add(new BlueprintSaveData(new(), new(), -1));

        // 그리드 관리
        for (int i = occupiedTilesPerSlot.Count; i < 4; i++)
            occupiedTilesPerSlot.Add(new HashSet<Vector2Int>());

        for (int i = isValidBP.Count; i < 4; i++)
            isValidBP.Add(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Customize")
            if (customize0Panel != null)
                InitializeSlotButtonColor();
    }

    /// <summary>
    /// 모두 회색으로 초기화
    /// </summary>
    private void InitializeSlotButtonColor()
    {
        Debug.LogError("slotmanager 버튼 초기화");
        Customize_0_Controller controller0 = customize0Panel.GetComponent<Customize_0_Controller>();

        for (int i = 0; i < blueprintSlots.Count; i++)
            controller0.UpdateSlotButtonColor(i, isValidBP[i]);
    }

    public void RegisterCustomizePanel(GameObject panel)
    {
        customize0Panel = panel;
        InitializeSlotButtonColor();
    }

    /// <summary>
    /// 현재 도안을 저장
    /// </summary>
    public void SaveBlueprintToCurrentSlot(BlueprintSaveData data)
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= blueprintSlots.Count)
            return;

        blueprintSlots[currentSlotIndex] = data;
    }

    /// <summary>
    /// 해당 슬롯의 도안을 반환
    /// </summary>
    public BlueprintSaveData GetBlueprintAt(int index)
    {
        if (index < 0 || index >= blueprintSlots.Count)
            return null;

        BlueprintSaveData data = blueprintSlots[index];
        if (data == null || (data.rooms == null && data.weapons == null))
            return null;

        return data;
    }

    /// <summary>
    /// 방의 점유 타일을 선택된 슬롯 도안에 저장
    /// </summary>
    /// <param name="occupied"></param>
    public void SaveOccupiedTilesToCurrentSlot(HashSet<Vector2Int> occupied)
    {
        if (currentSlotIndex < 0 || currentSlotIndex >= occupiedTilesPerSlot.Count)
            return;

        occupiedTilesPerSlot[currentSlotIndex] = new HashSet<Vector2Int>(occupied);
    }

    /// <summary>
    /// 도안의 점유 타일 호출
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public HashSet<Vector2Int> GetOccupiedTiles(int index)
    {
        if (index < 0 || index >= occupiedTilesPerSlot.Count)
            return new();

        return new HashSet<Vector2Int>(occupiedTilesPerSlot[index]);
    }

    public void SaveAllBlueprints()
    {
        BlueprintSlotSaveWrapper wrapper = new()
        {
            blueprintSlots = blueprintSlots,
            isValidBP = isValidBP,
            occupiedTilesPerSlot = new()
        };

        foreach (HashSet<Vector2Int> tileSet in occupiedTilesPerSlot)
        {
            wrapper.occupiedTilesPerSlot.Add(new List<Vector2Int>(tileSet));
        }

        string json = JsonUtility.ToJson(wrapper);
        File.WriteAllText(Application.persistentDataPath + "/blueprints.json", json);
    }

    public void LoadAllBlueprints()
    {
        string path = Application.persistentDataPath + "/blueprints.json";
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        BlueprintSlotSaveWrapper wrapper = JsonUtility.FromJson<BlueprintSlotSaveWrapper>(json);

        blueprintSlots = wrapper.blueprintSlots;
        isValidBP = wrapper.isValidBP;
        occupiedTilesPerSlot = new();

        // foreach (List<Vector2Int> tileList in wrapper.occupiedTilesPerSlot)
        // {
        //     if (tileList == null)
        //         continue;
        //     occupiedTilesPerSlot.Add(new HashSet<Vector2Int>(tileList));
        // }
    }
}
