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

    [Header("Room Prefabs")] [SerializeField]
    private EngineRoom engineRoomPrefab;

    [SerializeField] private PowerRoom powerRoomPrefab;
    [SerializeField] private ShieldRoom shieldRoomPrefab;
    [SerializeField] private OxygenRoom oxygenRoomPrefab;
    [SerializeField] private CockpitRoom cockpitRoomPrefab;
    [SerializeField] private WeaponControlRoom weaponControlRoomPrefab;
    [SerializeField] private AmmunitionRoom ammunitionRoomPrefab;
    [SerializeField] private StorageRoom storageRoomPrefab;
    [SerializeField] private CrewQuartersRoom crewQuartersRoomPrefab;
    [SerializeField] private LifeSupportRoom lifeSupportRoomPrefab;
    [SerializeField] private Door doorPrefab;

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
        if (position.x < 0 || position.y < 0 || position.x >= gridSize.x || position.y >= gridSize.y)
            return false;

        Door door = Instantiate(doorPrefab);
        door.transform.position = new Vector3(position.x, position.y, 0);

        if (isVertical)
            door.transform.rotation = Quaternion.Euler(0, 0, 90);

        doors.Add(door);
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

        Debug.LogWarning($"Stat {statType} not found!");
        return 0f;
    }

    public Dictionary<ShipStat, float> GetRoomContributions(string roomName)
    {
        if (roomContributions.TryGetValue(roomName, out Dictionary<ShipStat, float> contributions))
            return contributions;

        return new Dictionary<ShipStat, float>();
    }

    // ===== Power Management =====
    public bool RequestPowerForRoom(Room room, bool powerOn)
    {
        if (!powerOn)
        {
            // 전원을 끄는 경우는 항상 성공
            room.SetPowerStatus(false, false);
            RecalculateAllStats();
            return true;
        }

        // 전원을 켜려는 경우, 충분한 전력이 있는지 확인
        float availablePower = GetStat(ShipStat.PowerCapacity);
        float usedPower = GetStat(ShipStat.PowerUsing);
        float remainingPower = availablePower - usedPower;
        float requiredPower = room.GetPowerConsumption();

        if (remainingPower >= requiredPower)
        {
            // 충분한 전력이 있으면 전원 켜기
            room.SetPowerStatus(true, true);
            RecalculateAllStats();
            return true;
        }
        else
        {
            // 전력이 부족하면 요청만 설정하고 실제로는 켜지 않음
            room.SetPowerStatus(false, true);
            return false;
        }
    }

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

    public void FireWeapon(int weaponIndex, Ship targetShip)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count)
            return;

        ShipWeapon weapon = weapons[weaponIndex];
        Debug.Log($"{weapon.weaponName} fired.");

        // Add dodge calculation
        float dodgeChance = targetShip.GetStat(ShipStat.DodgeChance);
        if (UnityEngine.Random.value < dodgeChance / 100f)
        {
            Debug.Log($"{targetShip.shipName} dodged the attack!");
            return;
        }

        float finalDamage = weapon.damage;
        targetShip.TakeDamage(finalDamage);
    }

    public void TakeDamage(float damage)
    {
        // TODO : 방어막 관련 계산 수행
        // ShieldRoom shieldRoom = GetRoomOfType<ShieldRoom>();
        // if (shieldRoom != null && shieldRoom.GetCurrentShields() > 0)
        // {
        //     shieldRoom.TakeShieldDamage(damage);
        //     return;
        // }

        // Apply hull damage reduction

        /*
         *
        float hullDamageReduction = GetStat(ShipStat.DamageReduction);
        damage *= 1f - hullDamageReduction / 100f;

        hull -= damage;
        ApplyDamageToRandomRoom(damage);

        if (hull <= 0)
        {
            hull = 0;
            OnShipDestroyed();
        }
         *
         */
    }

    private void ApplyDamageToRandomRoom(float damage)
    {
        if (allRooms.Count == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, allRooms.Count);
        Room targetRoom = allRooms[randomIndex];
        targetRoom.TakeDamage(damage * 0.5f); // Only apply part of the damage to the room
    }

    private void OnShipDestroyed()
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

    public void ApplyMoraleBonusToAllCrew(float bonus)
    {
        foreach (CrewMember crewMember in crew)
            crewMember.AddMoraleBonus(bonus);
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
}
