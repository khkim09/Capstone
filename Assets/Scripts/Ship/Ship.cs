using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Ship Info")]
    [SerializeField] private string shipName = "Milky";
    [SerializeField] private float hull = 100f;
    [SerializeField] private float maxHull = 100f;
    [SerializeField] private float fuel = 500f;
    [SerializeField] private float maxFuel = 1000f;
    [SerializeField] private int scrap = 100; // 스크랩(재화)
    [SerializeField] private Vector2Int gridSize = new(20, 20);
    [SerializeField] private float shields = 0f;
    [SerializeField] private float maxShields = 0f;
    [SerializeField] private float dodgeChance = 0f;
    [SerializeField] private float hullDamageReduction = 0f;

    private readonly Dictionary<Vector2Int, Room> roomGrid = new();

    [Header("Room Prefabs")]
    [SerializeField] private EngineRoom engineRoomPrefab;
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

    [Header("Weapons")]
    [SerializeField] private List<ShipWeapon> weapons = new();
    [SerializeField] private int maxWeaponSlots = 4;

    [Header("Crew")]
    [SerializeField] private List<CrewMember> crew = new();
    [SerializeField] private int maxCrew = 6; // 최대 선원 수

    private readonly List<Door> doors = new();
    private Planet destinationPlanet;
    private int totalYears = 0; // 총 경과 연도

    private readonly List<Room> rooms = new();

    // 실시간 계산 속성
    public float HullPercentage => hull / maxHull * 100f;
    public float FuelPercentage => fuel / maxFuel * 100f;
    public bool CanWarp => GetRoomOfType<EngineRoom>() != null && GetRoomOfType<CockpitRoom>() != null;

    private void Awake()
    {
        // 초기화
        InitializeShip();
    }

    private void Update()
    {
        // ShipWeapon은 자체 Update()를 사용하므로 별도 업데이트 호출 제거
        ManagePower();
    }

    private void InitializeShip()
    {
        // 기본 함선 설정
        SetupDefaultShip();
    }

    public bool AddRoom(Room room, Vector2Int position)
    {
        if (!IsValidPosition(position, room.size))
            return false;

        room.position = position;
        rooms.Add(room);

        // 그리드에 방 등록
        for (int x = 0; x < room.size.x; x++)
        {
            for (int y = 0; y < room.size.y; y++)
            {
                Vector2Int gridPos = position + new Vector2Int(x, y);
                roomGrid[gridPos] = room;
            }
        }

        room.OnPlaced();
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

    // 함선 무기 공격 메서드 (간단화 버전)
    public void FireWeapon(int weaponIndex, Ship targetShip)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count)
            return;

        ShipWeapon weapon = weapons[weaponIndex];
        Debug.Log($"{weapon.weaponName} fired.");

        // 단순히 weapon.damage를 targetShip에 적용합니다.
        float finalDamage = weapon.damage;
        targetShip.TakeDamage(finalDamage);
    }

    // 선체 피해 적용 (weaponType 관련 로직 제거)
    public void TakeDamage(float damage)
    {
        ShieldRoom shieldRoom = GetRoomOfType<ShieldRoom>();
        if (shieldRoom != null && shieldRoom.currentShields > 0)
        {
            shieldRoom.TakeShieldDamage(1);
            return;
        }

        damage *= 1f - hullDamageReduction / 100f;
        hull -= damage;
        ApplyDamageToRandomRoom(damage);

        if (hull <= 0)
        {
            hull = 0;
            OnShipDestroyed();
        }
    }

    private void ApplyDamageToRandomRoom(float damage)
    {
        if (rooms.Count == 0) return;

        int randomIndex = Random.Range(0, rooms.Count);
        Room targetRoom = rooms[randomIndex];
        targetRoom.TakeDamage(damage);
    }

    public bool RemoveRoom(Room room)
    {
        if (!rooms.Contains(room))
            return false;

        for (int x = 0; x < room.size.x; x++)
        {
            for (int y = 0; y < room.size.y; y++)
            {
                Vector2Int gridPos = room.position + new Vector2Int(x, y);
                roomGrid.Remove(gridPos);
            }
        }

        rooms.Remove(room);
        Destroy(room.gameObject);
        return true;
    }

    private void ManagePower()
    {
        PowerRoom powerRoom = GetRoomOfType<PowerRoom>();
        if (powerRoom == null || !powerRoom.IsOperational())
            return;

        float availablePower = powerRoom.powerGeneration;

        List<Room> criticalRooms = new()
        {
            GetRoomOfType<OxygenRoom>(),
            GetRoomOfType<EngineRoom>(),
            GetRoomOfType<CockpitRoom>()
        };

        foreach (Room room in criticalRooms)
        {
            if (room != null)
            {
                float powerNeeded = room.GetPowerConsumption();
                if (availablePower >= powerNeeded)
                {
                    room.PowerUp();
                    availablePower -= powerNeeded;
                }
                else
                {
                    room.PowerDown();
                }
            }
        }

        List<Room> otherRooms = rooms
            .Where(r => !criticalRooms.Contains(r) && r != powerRoom)
            .OrderByDescending(r => r.GetPowerConsumption())
            .ToList();

        foreach (Room room in otherRooms)
        {
            float powerNeeded = room.GetPowerConsumption();
            if (availablePower >= powerNeeded)
            {
                room.PowerUp();
                availablePower -= powerNeeded;
            }
            else
            {
                room.PowerDown();
            }
        }
    }

    private float CalculateFuelCost(float distance)
    {
        float baseCost = distance * 10f;
        EngineRoom engineRoom = GetRoomOfType<EngineRoom>();
        CockpitRoom cockpitRoom = GetRoomOfType<CockpitRoom>();

        float efficiencyBonus = 0f;
        if (engineRoom != null && engineRoom.IsOperational())
            efficiencyBonus += engineRoom.fuelEfficiency;
        if (cockpitRoom != null && cockpitRoom.IsOperational())
            efficiencyBonus += cockpitRoom.fuelEfficiency;

        efficiencyBonus = Mathf.Min(efficiencyBonus, 50f);
        baseCost *= 1f - efficiencyBonus / 100f;

        return baseCost;
    }

    public float GetDodgeChance()
    {
        float totalDodge = 0f;
        EngineRoom engineRoom = GetRoomOfType<EngineRoom>();
        if (engineRoom != null && engineRoom.IsOperational())
            totalDodge += engineRoom.dodgeRate;

        CockpitRoom cockpitRoom = GetRoomOfType<CockpitRoom>();
        if (cockpitRoom != null && cockpitRoom.IsOperational())
            totalDodge += cockpitRoom.dodgeRate;

        return totalDodge;
    }

    private void OnShipDestroyed()
    {
        Debug.Log("Ship destroyed!");
        // 게임 오버 처리 로직
    }

    public T GetRoomOfType<T>() where T : Room
    {
        foreach (Room room in rooms)
        {
            if (room is T typedRoom)
                return typedRoom;
        }
        return null;
    }

    private bool IsValidPosition(Vector2Int pos, Vector2Int size)
    {
        if (pos.x < 0 || pos.y < 0 ||
            pos.x + size.x > gridSize.x ||
            pos.y + size.y > gridSize.y)
            return false;

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int checkPos = pos + new Vector2Int(x, y);
                if (roomGrid.ContainsKey(checkPos))
                    return false;
            }
        }
        return true;
    }

    public void SetupDefaultShip()
    {
        EngineRoom engineRoom = Instantiate(engineRoomPrefab);
        AddRoom(engineRoom, new Vector2Int(0, 0));

        PowerRoom powerRoom = Instantiate(powerRoomPrefab);
        AddRoom(powerRoom, new Vector2Int(3, 0));

        CockpitRoom cockpitRoom = Instantiate(cockpitRoomPrefab);
        AddRoom(cockpitRoom, new Vector2Int(0, 3));

        OxygenRoom oxygenRoom = Instantiate(oxygenRoomPrefab);
        AddRoom(oxygenRoom, new Vector2Int(3, 3));

        StorageRoom storageRoom = Instantiate(storageRoomPrefab);
        storageRoom.storageType = StorageType.Normal;
        AddRoom(storageRoom, new Vector2Int(6, 0));

        CrewQuartersRoom crewQuarters = Instantiate(crewQuartersRoomPrefab);
        AddRoom(crewQuarters, new Vector2Int(6, 3));

        WeaponControlRoom weaponRoom = Instantiate(weaponControlRoomPrefab);
        AddRoom(weaponRoom, new Vector2Int(9, 0));

        AddDoor(new Vector2Int(2, 1), false);
        AddDoor(new Vector2Int(1, 2), true);
        AddDoor(new Vector2Int(4, 1), false);
        AddDoor(new Vector2Int(4, 3), true);
        AddDoor(new Vector2Int(7, 1), false);
        AddDoor(new Vector2Int(7, 3), true);

        // 기본 무기 추가 - 새 ShipWeapon 필드에 맞게 초기화
        AddWeapon(new ShipWeapon
        {
            weaponName = "SLS-1 레이저건",
            damage = 80,
            fireRate = 1f,
            range = 100f
        });
    }

    public bool UpgradeRoom(Room room)
    {
        if (room.upgradeLevel >= room.maxUpgradeLevel)
            return false;

        float cost = room.upgradeCosts[room.upgradeLevel];
        if (scrap < cost)
            return false;

        scrap -= (int)cost;
        room.upgradeLevel++;
        return true;
    }

    public bool RefuelShip(float amount, int cost)
    {
        if (fuel >= maxFuel)
            return false;
        if (scrap < cost)
            return false;

        scrap -= cost;
        fuel = Mathf.Min(fuel + amount, maxFuel);
        return true;
    }

    public bool RepairHull(float amount, int cost)
    {
        if (hull >= maxHull)
            return false;
        if (scrap < cost)
            return false;

        scrap -= cost;
        hull = Mathf.Min(hull + amount, maxHull);
        return true;
    }

    public bool BuyItem(TradableItem item, int quantity)
    {
        int totalCost = item.currentPrice * quantity;
        if (scrap < totalCost)
            return false;
        if (!HasStorageSpaceFor(item, quantity))
            return false;

        scrap -= totalCost;
        AddItemToStorage(item, quantity);
        return true;
    }

    public bool SellItem(TradableItem item, int quantity)
    {
        if (!HasItem(item, quantity))
            return false;
        int totalValue = item.currentPrice * quantity;
        RemoveItemFromStorage(item, quantity);
        scrap += totalValue;
        return true;
    }

    private bool HasStorageSpaceFor(TradableItem item, int quantity)
    {
        foreach (Room room in rooms)
        {
            if (room is StorageRoom storage)
            {
                if (IsStorageCompatibleWithItem(storage, item))
                    return true;
            }
        }
        return false;
    }

    private bool IsStorageCompatibleWithItem(StorageRoom storage, TradableItem item)
    {
        switch (item.storageType)
        {
            case ItemStorageType.Normal:
                return storage.storageType == StorageType.Normal;
            case ItemStorageType.Temperature:
                return storage.storageType == StorageType.Temperature;
            case ItemStorageType.Animal:
                return storage.storageType == StorageType.Animal;
            default:
                return false;
        }
    }

    private bool HasItem(TradableItem item, int quantity)
    {
        return true;
    }

    private void AddItemToStorage(TradableItem item, int quantity)
    {
    }

    private void RemoveItemFromStorage(TradableItem item, int quantity)
    {
    }

    public bool AddCrewMember(CrewMember newCrew)
    {
        if (crew.Count >= maxCrew)
            return false;
        crew.Add(newCrew);
        UpdateCrewQuarters();
        return true;
    }

    public bool RemoveCrewMember(CrewMember crewToRemove)
    {
        if (!crew.Contains(crewToRemove))
            return false;
        crew.Remove(crewToRemove);
        UpdateCrewQuarters();
        return true;
    }

    private void UpdateCrewQuarters()
    {
        int totalCapacity = 0;
        foreach (Room room in rooms)
        {
            if (room is CrewQuartersRoom crewQuarters)
                totalCapacity += crewQuarters.maxCrewCapacity;
        }
        maxCrew = totalCapacity;
    }

    public void ApplyMoraleBonusToAllCrew(float bonus)
    {
        foreach (CrewMember crewMember in crew)
            crewMember.AddMoraleBonus(bonus);
    }

    public int GetScrap()
    {
        return scrap;
    }

    public void AddScrap(int amount)
    {
        scrap += amount;
    }

    public bool SpendScrap(int amount)
    {
        if (scrap < amount) return false;
        scrap -= amount;
        return true;
    }

    public float GetHull()
    {
        return hull;
    }

    public float GetMaxHull()
    {
        return maxHull;
    }

    public float GetFuel()
    {
        return fuel;
    }

    public float GetMaxFuel()
    {
        return maxFuel;
    }

    public int GetCrewCount()
    {
        return crew.Count;
    }

    public int GetMaxCrew()
    {
        return maxCrew;
    }

    public int GetTotalYears()
    {
        return totalYears;
    }

    public List<Room> GetAllRooms()
    {
        return rooms;
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
