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

    private readonly Dictionary<Vector2Int, Room> roomGrid = new();
    private readonly List<Door> doors = new();
    private readonly List<Room> allRooms = new();
    private readonly Dictionary<RoomType, List<Room>> roomsByType = new();
    private readonly Dictionary<ShipStat, float> currentStats = new();
    private readonly Dictionary<string, Dictionary<ShipStat, float>> roomContributions = new();

    private Dictionary<Type, ShipSystem> systems = new();

    // 테스트용 룸 프리팹
    public GameObject testRoomPrefab1;
    public GameObject testRoomPrefab2;

    public event Action OnStatsChanged;

    private void Awake()
    {
        InitializeBaseStats();
    }

    private void Start()
    {
        Room testRoom1 = Instantiate(testRoomPrefab1).GetComponent<Room>();
        Room testRoom2 = Instantiate(testRoomPrefab2).GetComponent<Room>();
        InitializeSystems();
        RecalculateAllStats();

        GameManager.Instance.SetPlayerShip(this);
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
        RegisterSystem(new CrewSystem());
        RegisterSystem(new MoraleSystem());
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

        MoraleManager.Instance.SetAllCrewMorale(GetSystem<MoraleSystem>().CalculateGlobalMorale());

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

        // TODO: 방을 제외한 ShipStatContributions 반영해야함 (ex : 외갑판, 선원)
        List<CrewBase> crews = GetSystem<CrewSystem>().GetCrews();

        foreach (CrewBase crew in crews)
        {
            Dictionary<ShipStat, float> crewContributions = crew.GetStatContributions();

            if (crewContributions.TryGetValue(ShipStat.OxygenUsingPerSecond, out float oxygenUsage))
                currentStats[ShipStat.OxygenUsingPerSecond] += oxygenUsage;
        }

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
    public void TakeAttack(float damage, WeaponType weaponType, Vector2Int hitPosition)
    {
        ShieldSystem shieldSystem = GetSystem<ShieldSystem>();
        if (shieldSystem.IsShieldActive()) damage = shieldSystem.TakeDamage(damage, weaponType);
        OuterHullSystem hullSystem = GetSystem<OuterHullSystem>();
        float finalDamage = hullSystem.ReduceDamage(damage);

        if (finalDamage > 0)
        {
            // 함선 전체에 데미지 적용
            TakeDamage(finalDamage);

            // 방과 선원에 대한 데미지 처리
            if (weaponType == WeaponType.Missile)
                // 미사일 폭발 위치를 중심으로 3x3 영역에 데미지
                ApplySplashDamageToRoomsAndCrews(hitPosition, finalDamage * 0.8f);
            else
                // 단일 지점 데미지
                ApplyDamageToRoomAndCrews(hitPosition, finalDamage);
        }
    }


    public void TakeDamage(float damage)
    {
        HitPointSystem hitPointSystem = GetSystem<HitPointSystem>();
        hitPointSystem.ChangeHitPoint(-damage);

        if (hitPointSystem.GetHitPoint() <= 0f) OnShipDestroyed();
    }

    private void ApplyDamageToCrewsInRoom(Room room, float damage)
    {
        List<CrewBase> crewsInRoom = room.crewInRoom;
        foreach (CrewBase crew in crewsInRoom) crew.TakeDamage(damage);
    }

    private void ApplyDamageToRoomAndCrews(Vector2Int position, float damage)
    {
        if (roomGrid.TryGetValue(position, out Room room))
        {
            room.TakeDamage(damage);
            ApplyDamageToCrewsInRoom(room, damage * 0.7f);
        }
    }

    private void ApplySplashDamageToRoomsAndCrews(Vector2Int centerPosition, float damage)
    {
        // 3x3 범위 내의 모든 타일 검사
        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 1; y++)
        {
            Vector2Int checkPos = centerPosition + new Vector2Int(x, y);

            // 해당 위치에 방이 있는지 확인
            if (roomGrid.TryGetValue(checkPos, out Room room))
            {
                // 방 데미지
                room.TakeDamage(damage);

                // 그 방에 있는 선원들에게 데미지 적용
                ApplyDamageToCrewsInRoom(room, damage * 0.7f);
            }
        }
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
        return GetSystem<CrewSystem>().GetCrewCount();
    }

    public int GetMaxCrew()
    {
        return (int)currentStats[ShipStat.CrewCapacity];
    }

    public List<Room> GetAllRooms()
    {
        return allRooms;
    }

    public List<Door> GetAllDoors()
    {
        return doors;
    }

    public List<CrewBase> GetAllCrew()
    {
        return GetSystem<CrewSystem>().GetCrews();
    }

    public List<ShipWeapon> GetAllWeapons()
    {
        return GetSystem<WeaponSystem>().GetWeapons();
    }

    public float GetCurrentHitPoints()
    {
        return GetSystem<HitPointSystem>().GetHitPoint();
    }

    public Vector2Int GetRandomTargetPosition()
    {
        // 실제 구현에서는 함선의 경계 내에서 랜덤 위치 반환
        // 또는 실제 방의 위치 반환
        Room randomRoom = GetRandomTargettableRoom();

        if (randomRoom == null) return Vector2Int.zero;

        return randomRoom.position;
    }

    private Room GetRandomTargettableRoom()
    {
        List<Room> validRooms = GetAllRooms().FindAll(room => room.GetIsDamageable() && room.GetHealthPercentage() > 0);

        if (validRooms.Count == 0) return null;

        int randomIndex = UnityEngine.Random.Range(0, validRooms.Count);
        return validRooms[randomIndex];
    }
}
