using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Ship Info")] [SerializeField] private string shipName = "Milky";
    [SerializeField] private float hull = 100f;
    [SerializeField] private float maxHull = 100f;
    [SerializeField] private Vector2Int gridSize = new(20, 20);
    [SerializeField] private bool showDebugInfo = true;

    [Header("Weapons")] [SerializeField] private List<ShipWeapon> weapons = new();
    [SerializeField] private int maxWeaponSlots = 4;

    [Header("Crew")] [SerializeField] private List<CrewMember> crew = new();
    [SerializeField] private int maxCrew = 6;

    private readonly Dictionary<Vector2Int, Room> roomGrid = new();
    private readonly List<Door> doors = new();
    private readonly List<Room> allRooms = new();
    private readonly Dictionary<RoomType, List<Room>> roomsByType = new();
    private readonly Dictionary<ShipStat, float> currentStats = new();
    private readonly Dictionary<string, Dictionary<ShipStat, float>> roomContributions = new();

    private Dictionary<Type, ShipSystem> systems = new();

    public event Action OnStatsChanged;

    private void Awake()
    {
        InitializeBaseStats();
    }

    private void Start()
    {
        RecalculateAllStats();
    }

    private void Update()
    {
        // 모든 시스템 업데이트
        foreach (ShipSystem system in systems.Values) system.Update(Time.deltaTime);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        foreach (Room room in allRooms)
            if (room != null)
                room.OnRoomStateChanged -= OnRoomStateChanged;
    }

    // ===== Room Management =====

    public bool AddRoom(Room room, Vector2Int position)
    {
        // TODO: 테스트용으로 임시로 검사 안함
        //  if (!IsValidPosition(position, room.GetSize()))
        //    return false;

        room.position = position;
        allRooms.Add(room);

        // Add to type dictionary
        if (!roomsByType.ContainsKey(room.roomType))
            roomsByType[room.roomType] = new List<Room>();
        roomsByType[room.roomType].Add(room);

        // Register for events
        room.OnRoomStateChanged += OnRoomStateChanged;

        // Add to grid
        for (int x = 0; x < room.GetSize().x; x++)
        for (int y = 0; y < room.GetSize().y; y++)
        {
            Vector2Int gridPos = position + new Vector2Int(x, y);
            roomGrid[gridPos] = room;
        }

        room.OnPlaced();

        // TODO: MoraleManager에서 사기 계산하기 해야됨

        // Recalculate stats
        RecalculateAllStats();

        return true;
    }

    public bool RemoveRoom(Room room)
    {
        if (!allRooms.Contains(room))
            return false;

        // Remove from grid
        for (int x = 0; x < room.GetSize().x; x++)
        for (int y = 0; y < room.GetSize().y; y++)
        {
            Vector2Int gridPos = room.position + new Vector2Int(x, y);
            roomGrid.Remove(gridPos);
        }

        // Remove from room type dictionary
        if (roomsByType.ContainsKey(room.roomType))
            roomsByType[room.roomType].Remove(room);

        // Unregister event
        room.OnRoomStateChanged -= OnRoomStateChanged;

        // Remove from list
        allRooms.Remove(room);
        Destroy(room.gameObject);

        // TODO: MoraleManager에서 사기 계산하기 해야됨

        // Recalculate stats
        RecalculateAllStats();

        return true;
    }

    private bool IsValidPosition(Vector2Int pos, Vector2Int size)
    {
        if (pos.x < 0 || pos.y < 0 ||
            pos.x + size.x > gridSize.x ||
            pos.y + size.y > gridSize.y)
            return false;

        for (int x = 0; x < size.x; x++)
        for (int y = 0; y < size.y; y++)
        {
            Vector2Int checkPos = pos + new Vector2Int(x, y);
            if (roomGrid.ContainsKey(checkPos))
                return false;
        }

        return true;
    }

    public bool AddDoor(Vector2Int position, bool isVertical)
    {
        return true;
    }

    public T GetRoomOfType<T>() where T : Room
    {
        foreach (Room room in allRooms)
            if (room is T typedRoom)
                return typedRoom;
        return null;
    }

    public List<Room> GetRoomsByType(RoomType type)
    {
        if (roomsByType.ContainsKey(type))
            return roomsByType[type];
        return new List<Room>();
    }

    private void InitializeSystems()
    {
        // TODO : 방 시스템 만들 때마다 여기에 등록
        RegisterSystem(new ShieldSystem());
        RegisterSystem(new WeaponSystem());
        RegisterSystem(new OuterHullSystem());
        RegisterSystem(new HitPointSystem());
    }

    private T RegisterSystem<T>(T system) where T : ShipSystem
    {
        system.Initialize(this);
        systems[typeof(T)] = system;
        return system;
    }

    public T GetSystem<T>() where T : ShipSystem
    {
        if (systems.TryGetValue(typeof(T), out ShipSystem system)) return system as T;
        return null;
    }

    private void InitializeBaseStats()
    {
        currentStats.Clear();

        // 기본 스탯 설정
        currentStats[ShipStat.DodgeChance] = 0f;
        currentStats[ShipStat.HitPointsMax] = 0f;
        currentStats[ShipStat.FuelEfficiency] = 0f;
        currentStats[ShipStat.FuelConsumption] = 0f;
        currentStats[ShipStat.ShieldMaxAmount] = 0f;
        currentStats[ShipStat.PowerUsing] = 0f;
        currentStats[ShipStat.PowerCapacity] = 0f;
        currentStats[ShipStat.OxygenGeneratePerSecond] = 0f;
        currentStats[ShipStat.OxygenUsingPerSecond] = 0f;
        currentStats[ShipStat.ShieldRespawnTime] = 0f;
        currentStats[ShipStat.ShieldRegeneratePerSecond] = 0f;
        currentStats[ShipStat.HealPerSecond] = 0f;
        currentStats[ShipStat.CrewCapacity] = 0f;
        currentStats[ShipStat.DamageReduction] = 0f;
    }

    private void OnRoomStateChanged(Room room)
    {
        if (showDebugInfo)
            Debug.Log($"Room state changed: {room.name}");

        RecalculateAllStats();
    }

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

        // TODO: 방을 제외한 ShipStatContributions 반영해야함 (ex : 외갑판)

        // 특별한 스탯 처리 (필요시)
        ProcessSpecialStats();

        // 디버깅 정보 출력
        if (showDebugInfo)
            PrintDebugStatInfo();

        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    // TODO: 스탯 추가할 때마다 합연산인지 곱연산인지 분류

    private bool IsAdditiveStatType(ShipStat statType)
    {
        switch (statType)
        {
            case ShipStat.DodgeChance:
            case ShipStat.FuelEfficiency:
            case ShipStat.FuelConsumption:
            case ShipStat.PowerUsing:
            case ShipStat.PowerCapacity:
            case ShipStat.HitPointsMax:
            case ShipStat.ShieldMaxAmount:
            case ShipStat.OxygenGeneratePerSecond:
            case ShipStat.OxygenUsingPerSecond:
            case ShipStat.ShieldRespawnTime:
            case ShipStat.ShieldRegeneratePerSecond:
            case ShipStat.HealPerSecond:
            case ShipStat.CrewCapacity:
            case ShipStat.DamageReduction:

                return true;
            default:
                return false;
        }
    }

    private bool IsMultiplicativeStatType(ShipStat statType)
    {
        switch (statType)
        {
            default:
                return false;
        }
    }

    private void ProcessSpecialStats()
    {
        // 다른 특수 스탯 상호작용...
    }

    private void PrintDebugStatInfo()
    {
        Debug.Log($"=== {shipName} Ship Stats ===");
        foreach (KeyValuePair<ShipStat, float> stat in currentStats)
            Debug.Log($"{stat.Key}: {stat.Value}");

        Debug.Log("=== Room Contributions ===");
        foreach (KeyValuePair<string, Dictionary<ShipStat, float>> room in roomContributions)
        {
            Debug.Log($"{room.Key}:");
            foreach (KeyValuePair<ShipStat, float> stat in room.Value)
                Debug.Log($"  {stat.Key}: {stat.Value}");
        }
    }

    public float GetStat(ShipStat statType)
    {
        if (currentStats.TryGetValue(statType, out float value))
            return value;

        return 0f;
    }

    public Dictionary<ShipStat, float> GetRoomContributions(string roomName)
    {
        if (roomContributions.TryGetValue(roomName, out Dictionary<ShipStat, float> contributions))
            return contributions;

        return new Dictionary<ShipStat, float>();
    }

    // ===== Power Management =====


    // ===== Combat System =====
    public bool AddWeapon(ShipWeapon weapon)
    {
        if (weapons.Count >= maxWeaponSlots)
            return false;

        weapons.Add(weapon);
        return true;
    }

    public bool RemoveWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count)
            return false;

        weapons.RemoveAt(index);
        return true;
    }

    public void TakeDamage(float damage, bool isMissileAttack)
    {
        // TODO: 미사일의 경우 주위 8칸 80% 데미지 받는 코드 추가해야됨.
        //      그럼 랜덤 방이 아니라 랜덤 칸을 뽑고 데미지를 입히는 방식이어야되는 것 아닌가?
        HitPointSystem hitPointSystem = GetSystem<HitPointSystem>();
        hitPointSystem.ChangeHitPoint(-damage);

        if (hitPointSystem.GetHitPoint() <= 0f) OnShipDestroyed();
        ApplyDamageToRandomRoom(damage);
    }

    private void ApplyDamageToRandomRoom(float damage)
    {
        List<Room> validRooms = allRooms.FindAll(room => room.GetHealthPercentage() > 0);

        if (validRooms.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, validRooms.Count);
        Room targetRoom = validRooms[randomIndex];

        targetRoom.TakeDamage(damage * 0.5f);
    }


    public void OnShipDestroyed()
    {
        Debug.Log($"Ship {shipName} destroyed!");
        // Implement game over logic
    }

    // ===== Warp System =====

    public bool Warp()
    {
        float fuelCost = CalculateWarpFuelCost();

        if (ResourceManager.Instance.GetResource(ResourceType.Fuel) < fuelCost)
            return false;

        List<Room> engineRooms = GetRoomsByType(RoomType.Engine);
        List<Room> cockpitRooms = GetRoomsByType(RoomType.Cockpit);

        // Check if crew requirement is met
        foreach (Room engineRoom in engineRooms)
            if (!engineRoom.HasEnoughCrew())
                return false;

        foreach (Room cockpitRoom in cockpitRooms)
            if (!cockpitRoom.HasEnoughCrew())
                return false;

        ResourceManager.Instance.ChangeResource(ResourceType.Fuel, -fuelCost);
        return true;
    }

    public float CalculateWarpFuelCost()
    {
        float fuelCost = GetStat(ShipStat.FuelConsumption);
        float fuelEfficiency = GetStat(ShipStat.FuelEfficiency);
        fuelCost *= 1 - fuelEfficiency / 100f;

        if (showDebugInfo)
            Debug.Log($"Warp fuel cost: {fuelCost}");

        return fuelCost;
    }

    // ===== Ship Repairs =====

    public bool RepairHull(float amount, int cost)
    {
        /*
         *   if (hull >= maxHull)
            return false;

        if (!ResourceManager.Instance.SpendResource(ResourceType.COMA, cost))
            return false;

        hull = Mathf.Min(hull + amount, maxHull);
         */

        return true;
    }

    // ===== Crew Management =====

    public bool AddCrewMember(CrewMember newCrew)
    {
        if (crew.Count >= maxCrew)
            return false;

        crew.Add(newCrew);
        return true;
    }

    public bool RemoveCrewMember(CrewMember crewToRemove)
    {
        if (!crew.Contains(crewToRemove))
            return false;

        crew.Remove(crewToRemove);

        return true;
    }

    // ===== Getters =====

    public float GetHull()
    {
        return hull;
    }

    public float GetMaxHull()
    {
        return maxHull;
    }

    public int GetCrewCount()
    {
        return crew.Count;
    }

    public int GetMaxCrew()
    {
        return maxCrew;
    }

    public List<Room> GetAllRooms()
    {
        return allRooms;
    }

    public List<Door> GetAllDoors()
    {
        return doors;
    }

    public List<CrewMember> GetAllCrew()
    {
        return crew;
    }

    public List<ShipWeapon> GetAllWeapons()
    {
        return weapons;
    }

    public float GetCurrentHitPoints()
    {
        return GetSystem<HitPointSystem>().GetHitPoint();
    }
}
