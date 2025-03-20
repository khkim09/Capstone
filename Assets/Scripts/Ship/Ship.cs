using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Serialization;

public enum ItemStorageType
{
    Normal,
    Temperature,
    Animal
}

public class Ship : MonoBehaviour
{
    [Header("Ship Info")] [SerializeField] private string shipName = "Milky";
    [SerializeField] private float hull = 100f;
    [SerializeField] private float maxHull = 100f;
    [SerializeField] private float fuel = 500f;
    [SerializeField] private float maxFuel = 1000f;
    [SerializeField] private int COMA = 100; // 스크랩(재화)
    [SerializeField] private Vector2Int gridSize = new(20, 20);
    [SerializeField] private float shields = 0f;
    [SerializeField] private float maxShields = 0f;
    [SerializeField] private float dodgeChance = 0f;
    [SerializeField] private float hullDamageReduction = 0f;

    private readonly Dictionary<Vector2Int, Room> roomGrid = new();

    [FormerlySerializedAs("engineRoomBasePrefab")] [Header("Room Prefabs")] [SerializeField]
    private EngineRoom engineRoomPrefab;

    [FormerlySerializedAs("powerRoomBasePrefab")] [SerializeField]
    private PowerRoom powerRoomPrefab;

    [FormerlySerializedAs("shieldRoomBasePrefab")] [SerializeField]
    private ShieldRoom shieldRoomPrefab;

    [FormerlySerializedAs("oxygenRoomBasePrefab")] [SerializeField]
    private OxygenRoom oxygenRoomPrefab;

    [FormerlySerializedAs("cockpitRoomBasePrefab")] [SerializeField]
    private CockpitRoom cockpitRoomPrefab;

    [FormerlySerializedAs("weaponControlRoomBasePrefab")] [SerializeField]
    private WeaponControlRoom weaponControlRoomPrefab;

    [FormerlySerializedAs("ammunitionRoomBasePrefab")] [SerializeField]
    private AmmunitionRoom ammunitionRoomPrefab;

    [FormerlySerializedAs("storageRoomBasePrefab")] [SerializeField]
    private StorageRoom storageRoomPrefab;

    [FormerlySerializedAs("crewQuartersRoomBasePrefab")] [SerializeField]
    private CrewQuartersRoom crewQuartersRoomPrefab;

    [FormerlySerializedAs("lifeSupportRoomBasePrefab")] [SerializeField]
    private LifeSupportRoom lifeSupportRoomPrefab;

    [SerializeField] private Door doorPrefab;

    [Header("Weapons")] [SerializeField] private List<ShipWeapon> weapons = new();
    [SerializeField] private int maxWeaponSlots = 4;

    [Header("Crew")] [SerializeField] private List<CrewMember> crew = new();
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
        if (!IsValidPosition(position, room.GetSize()))
            return false;

        room.position = position;
        rooms.Add(room);

        // 그리드에 방 등록
        for (int x = 0; x < room.GetSize().x; x++)
        for (int y = 0; y < room.GetSize().y; y++)
        {
            Vector2Int gridPos = position + new Vector2Int(x, y);
            roomGrid[gridPos] = room;
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

        for (int x = 0; x < room.GetSize().x; x++)
        for (int y = 0; y < room.GetSize().y; y++)
        {
            Vector2Int gridPos = room.position + new Vector2Int(x, y);
            roomGrid.Remove(gridPos);
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

        float availablePower = powerRoom.GetCurrentLevelData().powerRequirement;

        List<Room> criticalRooms = new()
        {
            GetRoomOfType<OxygenRoom>(), GetRoomOfType<EngineRoom>(), GetRoomOfType<CockpitRoom>()
        };

        foreach (Room room in criticalRooms)
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

    private void OnShipDestroyed()
    {
        Debug.Log("Ship destroyed!");
        // 게임 오버 처리 로직
    }

    public T GetRoomOfType<T>() where T : Room
    {
        foreach (Room room in rooms)
            if (room is T typedRoom)
                return typedRoom;
        return null;
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
        AddWeapon(new ShipWeapon { weaponName = "SLS-1 레이저건", damage = 80, fireRate = 1f, range = 100f });
    }


    public bool RefuelShip(float amount, int cost)
    {
        if (fuel >= maxFuel)
            return false;
        if (COMA < cost)
            return false;

        COMA -= cost;
        fuel = Mathf.Min(fuel + amount, maxFuel);
        return true;
    }

    public bool RepairHull(float amount, int cost)
    {
        if (hull >= maxHull)
            return false;
        if (COMA < cost)
            return false;

        COMA -= cost;
        hull = Mathf.Min(hull + amount, maxHull);
        return true;
    }

    public bool BuyItem(TradableItem item, int quantity)
    {
        // GetCurrentPrice()를 사용하여 가격 변동 적용 및 정수형으로 변환
        int totalCost = Mathf.RoundToInt(item.GetCurrentPrice() * quantity);

        // 스크랩 충분한지 확인
        if (COMA < totalCost)
            return false;

        // 창고 공간 확인 (호환성 체크 로직이 필요하면 수정)
        if (!HasStorageSpaceFor(item, quantity))
            return false;

        // 거래 완료
        COMA -= totalCost;
        AddItemToStorage(item, quantity);

        return true;
    }


    public bool SellItem(TradableItem item, int quantity)
    {
        // 아이템 소유 여부 확인
        if (!HasItem(item, quantity))
            return false;

        // 판매 단가는 현재 가격의 90%로 가정 (10% 할인)
        int totalValue = Mathf.RoundToInt(item.GetCurrentPrice() * 0.9f * quantity);

        // 거래 완료
        RemoveItemFromStorage(item, quantity);
        COMA += totalValue;

        return true;
    }

    // TradableItem의 category 값을 기반으로 ItemStorageType으로 매핑하는 함수
// TradableItem의 category 값을 StorageType으로 매핑하는 헬퍼 함수
    private StorageType GetMappedStorageType(TradableItem item)
    {
        switch (item.category)
        {
            case "향신료":
                return StorageType.Normal;
            case "소재":
                return StorageType.Temperature;
            case "보석":
                return StorageType.Normal;
            case "사치품":
                return StorageType.Normal;
            case "식량":
                return StorageType.Temperature;
            case "동물":
                return StorageType.Animal;
            case "유물":
                return StorageType.Normal;
            case "광물":
                return StorageType.Normal;
            case "무기":
                return StorageType.Normal;
            default:
                return StorageType.Normal;
        }
    }

// StorageRoom과 TradableItem 간 호환성 검사 함수
    private bool IsStorageCompatibleWithItem(StorageRoom storage, TradableItem item)
    {
        // 기존 StorageType enum을 사용하여 비교
        return storage.storageType == GetMappedStorageType(item);
    }


// 창고에 아이템 추가 가능한 공간이 충분한지 검사
private bool HasStorageSpaceFor(TradableItem item, int quantity)
{
    foreach (Room room in rooms)
    {
        if (room is StorageRoom storageRoom)
        {
            if (IsStorageCompatibleWithItem(storageRoom, item))
            {
                // Storage 컴포넌트 가져오기
                Storage storageComponent = storageRoom.GetComponent<Storage>();
                if (storageComponent != null)
                {
                    int currentQuantity = storageComponent.GetItemQuantity(item);
                    int available = item.maxStackAmount - currentQuantity;
                    if (available >= quantity)
                        return true;
                }
            }
        }
    }
    return false;
}

// 창고에 해당 아이템이 충분히 있는지 검사
private bool HasItem(TradableItem item, int quantity)
{
    foreach (Room room in rooms)
    {
        if (room is StorageRoom storageRoom)
        {
            if (IsStorageCompatibleWithItem(storageRoom, item))
            {
                Storage storageComponent = storageRoom.GetComponent<Storage>();
                if (storageComponent != null)
                {
                    int currentQuantity = storageComponent.GetItemQuantity(item);
                    if (currentQuantity >= quantity)
                        return true;
                }
            }
        }
    }
    return false;
}

// 창고에 아이템을 추가 (여러 StorageRoom에 걸쳐 추가 가능)
private void AddItemToStorage(TradableItem item, int quantity)
{
    int remaining = quantity;
    foreach (Room room in rooms)
    {
        if (room is StorageRoom storageRoom)
        {
            if (IsStorageCompatibleWithItem(storageRoom, item))
            {
                Storage storageComponent = storageRoom.GetComponent<Storage>();
                if (storageComponent != null)
                {
                    int currentQuantity = storageComponent.GetItemQuantity(item);
                    int available = item.maxStackAmount - currentQuantity;
                    if (available > 0)
                    {
                        int toAdd = Mathf.Min(available, remaining);
                        storageComponent.AddItem(item, toAdd);
                        remaining -= toAdd;
                        if (remaining <= 0)
                            break;
                    }
                }
            }
        }
    }
}

// 창고에서 아이템을 제거 (여러 StorageRoom에서 분할 제거 가능)
private void RemoveItemFromStorage(TradableItem item, int quantity)
{
    int remaining = quantity;
    foreach (Room room in rooms)
    {
        if (room is StorageRoom storageRoom)
        {
            if (IsStorageCompatibleWithItem(storageRoom, item))
            {
                Storage storageComponent = storageRoom.GetComponent<Storage>();
                if (storageComponent != null)
                {
                    int currentQuantity = storageComponent.GetItemQuantity(item);
                    if (currentQuantity > 0)
                    {
                        int toRemove = Mathf.Min(currentQuantity, remaining);
                        storageComponent.RemoveItem(item, toRemove);
                        remaining -= toRemove;
                        if (remaining <= 0)
                            break;
                    }
                }
            }
        }
    }
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
            if (room is CrewQuartersRoom crewQuarters)
                totalCapacity += crewQuarters.maxCrewCapacity;
        maxCrew = totalCapacity;
    }

    public void ApplyMoraleBonusToAllCrew(float bonus)
    {
        foreach (CrewMember crewMember in crew)
            crewMember.AddMoraleBonus(bonus);
    }

    public int GetCOMA()
    {
        return COMA;
    }

    public void AddCOMA(int amount)
    {
        COMA += amount;
    }

    public bool SpendCOMA(int amount)
    {
        if (COMA < amount) return false;
        COMA -= amount;
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
