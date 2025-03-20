using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    [Header("기본 스탯 설정")] [SerializeField] private float baseDodgeChance = 0f;
    [SerializeField] private float baseHitPoints;
    [SerializeField] private float baseEnergyEfficiency;
    [SerializeField] private float baseFuelCapacity;
    [SerializeField] private float baseFuelConsumptionRate;
    [SerializeField] private float baseWarpSpeed;
    [SerializeField] private float baseSensorRange;
    [SerializeField] private float baseWeaponDamage;
    [SerializeField] private float baseShieldCapacity;
    [SerializeField] private float baseRepairSpeed;

    [Header("디버깅")] [SerializeField] private bool showDebugInfo = false;

    private Dictionary<ShipStat, float> currentStats = new();

    private Dictionary<string, Dictionary<ShipStat, float>> roomContributions = new();
    public event Action OnStatsChanged;

    private List<Room> allRooms = new();

    private Dictionary<RoomType, List<Room>> roomsByType = new();

    public static ShipManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeBaseStats();
    }

    private void Start()
    {
        // 초기 스탯 계산
        RecalculateAllStats();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        foreach (Room room in allRooms)
            if (room != null)
                room.OnRoomStateChanged -= OnRoomStateChanged;
    }

    /// <summary>
    /// 모든 기본 스탯을 초기화합니다.
    /// </summary>
    private void InitializeBaseStats()
    {
        currentStats.Clear();

        // 기본 스탯 설정
        currentStats[ShipStat.DodgeChance] = baseDodgeChance;
        currentStats[ShipStat.HitPoints] = baseHitPoints;
        currentStats[ShipStat.FuelEfficiency] = baseEnergyEfficiency;
        currentStats[ShipStat.FuelConsumption] = baseFuelConsumptionRate;
        currentStats[ShipStat.WarpSpeed] = baseWarpSpeed;
        currentStats[ShipStat.SensorRange] = baseSensorRange;
        currentStats[ShipStat.WeaponDamage] = baseWeaponDamage;
        currentStats[ShipStat.ShieldCapacity] = baseShieldCapacity;
        currentStats[ShipStat.RepairSpeed] = baseRepairSpeed;
    }

    /// <summary>
    /// 방의 상태가 변경되었을 때 호출되는 콜백 메서드
    /// </summary>
    private void OnRoomStateChanged(Room room)
    {
        if (showDebugInfo)
            Debug.Log($"Room state changed: {room.name}");

        RecalculateAllStats();
    }

    /// <summary>
    /// 모든 스탯을 재계산합니다.
    /// </summary>
    public void RecalculateAllStats()
    {
        // 기본값으로 초기화
        InitializeBaseStats();

        // 디버깅용 기여도 초기화
        roomContributions.Clear();

        // 각 방의 기여도 추가
        foreach (Room room in allRooms)
        {
            if (room == null) continue;

            Dictionary<ShipStat, float> contributions = room.GetStatContributions();

            // 디버깅용 기여도 저장
            roomContributions[room.name] = new Dictionary<ShipStat, float>(contributions);

            // 스탯에 기여도 적용
            foreach (KeyValuePair<ShipStat, float> contribution in contributions)
                if (currentStats.ContainsKey(contribution.Key))
                {
                    // 스탯 타입에 따라 다르게 적용 (가산 또는 곱셈)
                    if (IsAdditiveStatType(contribution.Key))
                        // 가산 스탯 (예: 회피율, 에너지 효율 등)
                        currentStats[contribution.Key] += contribution.Value;
                    else if (IsMultiplicativeStatType(contribution.Key))
                        // 곱셈 스탯 (예: 내구도 보너스 %)
                        currentStats[contribution.Key] *= 1 + contribution.Value / 100f;
                }
        }

        // 특별한 스탯 처리 (필요시)
        ProcessSpecialStats();

        // 디버깅 정보 출력
        if (showDebugInfo)
            PrintDebugStatInfo();

        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    /// <summary>
    /// 가산적으로 적용되는 스탯인지 확인
    /// </summary>
    private bool IsAdditiveStatType(ShipStat statType)
    {
        switch (statType)
        {
            case ShipStat.DodgeChance:
            case ShipStat.FuelEfficiency:
            case ShipStat.SensorRange:
            case ShipStat.FuelConsumption:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 곱셈적으로 적용되는 스탯인지 확인 (보통 %로 표현됨)
    /// </summary>
    private bool IsMultiplicativeStatType(ShipStat statType)
    {
        switch (statType)
        {
            case ShipStat.HitPoints:
            case ShipStat.ShieldCapacity:
            case ShipStat.WeaponDamage:
            case ShipStat.RepairSpeed:
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 상호작용이나 특별한 계산이 필요한 스탯 처리
    /// </summary>
    private void ProcessSpecialStats()
    {
        // 예: 연료 소모율은 에너지 효율에 반비례
        float energyEfficiency = currentStats[ShipStat.FuelEfficiency];
        if (energyEfficiency > 0)
        {
            float reduction = energyEfficiency / 100f;
            currentStats[ShipStat.FuelConsumption] *= 1f - Mathf.Min(0.75f, reduction);
        }

        // 다른 특수 스탯 상호작용...
    }

    /// <summary>
    /// 디버깅 정보 출력
    /// </summary>
    private void PrintDebugStatInfo()
    {
        Debug.Log("=== Ship Stats ===");
        foreach (KeyValuePair<ShipStat, float> stat in currentStats) Debug.Log($"{stat.Key}: {stat.Value}");

        Debug.Log("=== Room Contributions ===");
        foreach (KeyValuePair<string, Dictionary<ShipStat, float>> room in roomContributions)
        {
            Debug.Log($"{room.Key}:");
            foreach (KeyValuePair<ShipStat, float> stat in room.Value) Debug.Log($"  {stat.Key}: {stat.Value}");
        }
    }

    /// <summary>
    /// 특정 스탯의 현재 값을 가져옵니다.
    /// </summary>
    public float GetStat(ShipStat statType)
    {
        if (currentStats.TryGetValue(statType, out float value))
            return value;

        Debug.LogWarning($"Stat {statType} not found!");
        return 0f;
    }

    /// <summary>
    /// 특정 방의 스탯 기여도를 가져옵니다 (디버깅용).
    /// </summary>
    public Dictionary<ShipStat, float> GetRoomContributions(string roomName)
    {
        if (roomContributions.TryGetValue(roomName, out Dictionary<ShipStat, float> contributions))
            return contributions;

        return new Dictionary<ShipStat, float>();
    }

    /// <summary>
    /// 새로운 방을 등록합니다.
    /// </summary>
    public void RegisterRoom(Room room)
    {
        Debug.Log("RegisterRoom 호출");
        if (!allRooms.Contains(room))
        {
            allRooms.Add(room);

            // 타입별 사전에 추가
            if (!roomsByType.ContainsKey(room.roomType)) roomsByType[room.roomType] = new List<Room>();
            roomsByType[room.roomType].Add(room);

            room.OnRoomStateChanged += OnRoomStateChanged;
            RecalculateAllStats();
        }
    }

    /// <summary>
    /// 특정 종류의 방들을 빠르게 가져옴.
    /// </summary>
    /// <param name="type">방의 종류</param>
    /// <returns>해당하는 방들 리스트</returns>
    public List<Room> GetRoomsByType(RoomType type)
    {
        if (roomsByType.ContainsKey(type)) return roomsByType[type];
        return new List<Room>();
    }

    /// <summary>
    /// 방을 해제합니다.
    /// </summary>
    public void UnregisterRoom(Room room)
    {
        if (allRooms.Contains(room))
        {
            room.OnRoomStateChanged -= OnRoomStateChanged;
            allRooms.Remove(room);
            RecalculateAllStats();
        }
    }

    public bool Warp()
    {
        float fuelConsumption = GetStat(ShipStat.FuelConsumption);
        float fuelEfficiency = GetStat(ShipStat.FuelEfficiency);
        fuelConsumption *= 1 - fuelEfficiency;

        if (ResourceManager.Instance.GetResource(ResourceType.Fuel) < fuelConsumption) return false;

        List<Room> enginRooms = GetRoomsByType(RoomType.Engine);
        List<Room> cockpitRooms = GetRoomsByType(RoomType.Cockpit);


        foreach (Room enginRoom in enginRooms)
            if (!enginRoom.HasEnoughCrew())
                return false;

        foreach (Room cockpitRoom in cockpitRooms)
            if (!cockpitRoom.HasEnoughCrew())
                return false;

        ResourceManager.Instance.ChangeResource(ResourceType.Fuel, -fuelConsumption);

        return true;
    }
}
