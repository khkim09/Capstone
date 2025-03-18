using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Ship Info")] [SerializeField] private string shipName = "USS Enterprise";
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
        // 무기 쿨다운 업데이트
        UpdateWeapons(Time.deltaTime);

        // 전력 관리
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
        for (int y = 0; y < room.size.y; y++)
        {
            Vector2Int gridPos = position + new Vector2Int(x, y);
            roomGrid[gridPos] = room;
        }

        room.OnPlaced();
        return true;
    }

    public bool AddDoor(Vector2Int position, bool isVertical)
    {
        // 문 위치 유효성 검사
        if (position.x < 0 || position.y < 0 || position.x >= gridSize.x || position.y >= gridSize.y)
            return false;

        // 문 생성
        Door door = Instantiate(doorPrefab);
        door.transform.position = new Vector3(position.x, position.y, 0);

        // 문 방향 설정
        if (isVertical) door.transform.rotation = Quaternion.Euler(0, 0, 90);

        doors.Add(door);
        return true;
    }

    // 무기 시스템 함수들
    private void UpdateWeapons(float deltaTime)
    {
        // 탄약고 찾기
        AmmunitionRoom ammoRoom = GetRoomOfType<AmmunitionRoom>();
        float reloadBonus = ammoRoom != null && ammoRoom.IsOperational() ? ammoRoom.reloadSpeedBonus : 0f;

        // 모든 무기 쿨다운 업데이트
        foreach (ShipWeapon weapon in weapons) weapon.Update(deltaTime, reloadBonus);
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

    // 함선 무기 공격 메서드
    public void FireWeapon(int weaponIndex, Ship targetShip)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count) return;

        ShipWeapon weapon = weapons[weaponIndex];
        if (!weapon.isReady) return;

        // 무기 발사
        weapon.Fire();

        // 명중률 계산
        WeaponControlRoom weaponRoom = GetRoomOfType<WeaponControlRoom>();
        float accuracyBonus = weaponRoom != null && weaponRoom.IsOperational() ? weaponRoom.accuracyBonus : 0f;
        float finalAccuracy = weapon.accuracy + accuracyBonus;

        // 타겟 회피율 계산
        float targetDodge = targetShip.GetDodgeChance();

        // 명중 여부 결정
        bool hit = Random.Range(0f, 100f) <= finalAccuracy * (1f - targetDodge / 100f);

        if (hit)
        {
            // 피해량 계산
            AmmunitionRoom ammoRoom = GetRoomOfType<AmmunitionRoom>();
            float damageBonus = ammoRoom != null && ammoRoom.IsOperational() ? ammoRoom.weaponDamageBonus : 0f;
            float finalDamage = weapon.damage * (1f + damageBonus / 100f);

            // 타겟에게 데미지 적용
            targetShip.TakeDamage(finalDamage, weapon.weaponType);
        }
    }

    public void TakeDamage(float damage, WeaponType weaponType)
    {
        // 방어막이 있는 경우 방어막 먼저 감소
        ShieldRoom shieldRoom = GetRoomOfType<ShieldRoom>();
        int currentShields = shieldRoom != null ? shieldRoom.currentShields : 0;

        // 레일건은 방어막을 추가 피해로 관통
        float shieldMultiplier = weaponType == WeaponType.Railgun ? 1.5f : 1f;

        if (currentShields > 0)
        {
            shieldRoom.TakeShieldDamage(1); // 1회 공격에 방어막 1 감소

            // 방어막을 뚫고 나머지 데미지 계산
            if (weaponType == WeaponType.Railgun)
                damage *= 0.5f; // 방어막을 뚫은 후 선체 피해는 50%
            else
                return; // 일반 무기는 방어막을 뚫지 못하고 방어막만 감소
        }

        // 선체 방어력(외갑판) 적용
        damage *= 1f - hullDamageReduction / 100f;

        // 선체 피해 적용
        hull -= damage;

        // 임의의 방에 피해 적용
        ApplyDamageToRandomRoom(damage);

        // 선체 파괴 체크
        if (hull <= 0)
        {
            hull = 0;
            OnShipDestroyed();
        }
    }

    // 무작위 방 피해 적용
    private void ApplyDamageToRandomRoom(float damage)
    {
        if (rooms.Count == 0) return;

        // 무작위로 방 선택
        int randomIndex = Random.Range(0, rooms.Count);
        Room targetRoom = rooms[randomIndex];

        // 방에 피해 적용
        targetRoom.TakeDamage(damage);
    }

    // 방 제거 함수
    public bool RemoveRoom(Room room)
    {
        if (!rooms.Contains(room))
            return false;

        // 그리드에서 방 제거
        for (int x = 0; x < room.size.x; x++)
        for (int y = 0; y < room.size.y; y++)
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
        // 전력실 찾기
        PowerRoom powerRoom = GetRoomOfType<PowerRoom>();
        if (powerRoom == null || !powerRoom.IsOperational())
            return;

        // 총 가용 전력 계산
        float availablePower = powerRoom.powerGeneration;

        // 필수 시스템 먼저 전력 공급
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

        // 나머지 방에 전력 공급
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
        float baseCost = distance * 10f; // 기본 연료 소모량

        // 엔진실과 조종실의 연료 효율 보너스 적용
        EngineRoom engineRoom = GetRoomOfType<EngineRoom>();
        CockpitRoom cockpitRoom = GetRoomOfType<CockpitRoom>();

        float efficiencyBonus = 0f;

        if (engineRoom != null && engineRoom.IsOperational()) efficiencyBonus += engineRoom.fuelEfficiency;

        if (cockpitRoom != null && cockpitRoom.IsOperational()) efficiencyBonus += cockpitRoom.fuelEfficiency;

        // 연료 효율 보너스 적용 (최대 50%까지 절감)
        efficiencyBonus = Mathf.Min(efficiencyBonus, 50f);
        baseCost *= 1f - efficiencyBonus / 100f;

        return baseCost;
    }

    // 회피율 계산
    public float GetDodgeChance()
    {
        float totalDodge = 0f;

        // 엔진실 회피율
        EngineRoom engineRoom = GetRoomOfType<EngineRoom>();
        if (engineRoom != null && engineRoom.IsOperational()) totalDodge += engineRoom.dodgeRate;

        // 조종실 회피율
        CockpitRoom cockpitRoom = GetRoomOfType<CockpitRoom>();
        if (cockpitRoom != null && cockpitRoom.IsOperational()) totalDodge += cockpitRoom.dodgeRate;

        return totalDodge;
    }

    // 함선 파괴 처리
    private void OnShipDestroyed()
    {
        // 함선 파괴 처리 로직
        Debug.Log("Ship destroyed!");
        // 게임 오버 이벤트 등 처리
        // 게임 오버 이벤트 발생
        //GameManager.instance.GameOver();
    }

    // 특정 타입의 방 찾기
    public T GetRoomOfType<T>() where T : Room
    {
        foreach (Room room in rooms)
            if (room is T typedRoom)
                return typedRoom;

        return null;
    }


    private bool IsValidPosition(Vector2Int pos, Vector2Int size)
    {
        // 경계 확인
        if (pos.x < 0 || pos.y < 0 ||
            pos.x + size.x > gridSize.x ||
            pos.y + size.y > gridSize.y)
            return false;

        // 다른 방과 겹치는지 확인
        for (int x = 0; x < size.x; x++)
        for (int y = 0; y < size.y; y++)
        {
            Vector2Int checkPos = pos + new Vector2Int(x, y);
            if (roomGrid.ContainsKey(checkPos))
                return false;
        }

        return true;
    }

    // 기본 함선 설정
    public void SetupDefaultShip()
    {
        // 필수 방들 생성

        // 1. 엔진룸
        EngineRoom engineRoom = Instantiate(engineRoomPrefab);
        AddRoom(engineRoom, new Vector2Int(0, 0));

        // 2. 전력실
        PowerRoom powerRoom = Instantiate(powerRoomPrefab);
        AddRoom(powerRoom, new Vector2Int(3, 0));

        // 3. 조종실
        CockpitRoom cockpitRoom = Instantiate(cockpitRoomPrefab);
        AddRoom(cockpitRoom, new Vector2Int(0, 3));

        // 4. 산소실
        OxygenRoom oxygenRoom = Instantiate(oxygenRoomPrefab);
        AddRoom(oxygenRoom, new Vector2Int(3, 3));

        // 5. 기본 창고
        StorageRoom storageRoom = Instantiate(storageRoomPrefab);
        storageRoom.storageType = StorageType.Normal;
        AddRoom(storageRoom, new Vector2Int(6, 0));

        // 6. 선원 숙소
        CrewQuartersRoom crewQuarters = Instantiate(crewQuartersRoomPrefab);
        AddRoom(crewQuarters, new Vector2Int(6, 3));

        // 7. 무기 제어실
        WeaponControlRoom weaponRoom = Instantiate(weaponControlRoomPrefab);
        AddRoom(weaponRoom, new Vector2Int(9, 0));

        // 방과 방 사이에 문 추가
        AddDoor(new Vector2Int(2, 1), false); // 엔진실-전력실 사이
        AddDoor(new Vector2Int(1, 2), true); // 엔진실-조종실 사이
        AddDoor(new Vector2Int(4, 1), false); // 전력실-창고 사이
        AddDoor(new Vector2Int(4, 3), true); // 산소실-창고 사이
        AddDoor(new Vector2Int(7, 1), false); // 창고-무기제어실 사이
        AddDoor(new Vector2Int(7, 3), true); // 선원숙소-무기제어실 사이

        // 기본 무기 추가
        AddWeapon(new ShipWeapon
        {
            weaponName = "SLS-1 레이저건",
            damage = 80,
            accuracy = 70,
            reloadTime = 7,
            currentCooldown = 0,
            weaponType = WeaponType.Laser
        });
    }

    // 업그레이드 함수들
    public bool UpgradeRoom(Room room)
    {
        // 이미 최대 레벨인지 확인
        if (room.upgradeLevel >= room.maxUpgradeLevel)
            return false;

        // 비용 확인
        float cost = room.upgradeCosts[room.upgradeLevel];
        if (scrap < cost)
            return false;

        // 업그레이드 적용
        scrap -= (int)cost;
        room.upgradeLevel++;

        return true;
    }

    // 연료 및 선체 수리
    public bool RefuelShip(float amount, int cost)
    {
        // 이미 연료가 가득 찼는지 확인
        if (fuel >= maxFuel)
            return false;

        // 스크랩 충분한지 확인
        if (scrap < cost)
            return false;

        // 연료 충전
        scrap -= cost;
        fuel = Mathf.Min(fuel + amount, maxFuel);

        return true;
    }

    public bool RepairHull(float amount, int cost)
    {
        // 이미 선체가 수리되었는지 확인
        if (hull >= maxHull)
            return false;

        // 스크랩 충분한지 확인
        if (scrap < cost)
            return false;

        // 선체 수리
        scrap -= cost;
        hull = Mathf.Min(hull + amount, maxHull);

        return true;
    }

    // 무역 함수
    public bool BuyItem(TradableItem item, int quantity)
    {
        // 비용 계산
        int totalCost = item.currentPrice * quantity;

        // 스크랩 충분한지 확인
        if (scrap < totalCost)
            return false;

        // 창고 공간 확인 및 아이템 저장
        if (!HasStorageSpaceFor(item, quantity))
            return false;

        // 거래 완료
        scrap -= totalCost;
        AddItemToStorage(item, quantity);

        return true;
    }

    public bool SellItem(TradableItem item, int quantity)
    {
        // 아이템 소유 여부 확인
        if (!HasItem(item, quantity))
            return false;

        // 판매 금액 계산
        int totalValue = item.currentPrice * quantity;

        // 거래 완료
        RemoveItemFromStorage(item, quantity);
        scrap += totalValue;

        return true;
    }

    // 창고 관련 함수들
    private bool HasStorageSpaceFor(TradableItem item, int quantity)
    {
        // 적절한 창고 찾기
        foreach (Room room in rooms)
            if (room is StorageRoom storage)
                // 아이템 타입에 맞는 창고인지 확인
                if (IsStorageCompatibleWithItem(storage, item))
                    // 창고 공간 확인 로직
                    // (여기서는 간단하게 구현)
                    return true;

        return false;
    }

    private bool IsStorageCompatibleWithItem(StorageRoom storage, TradableItem item)
    {
        // 아이템 타입에 따라 적합한 창고 확인
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
        // 창고에서 아이템 확인
        // (실제 구현 필요)
        return true;
    }

    private void AddItemToStorage(TradableItem item, int quantity)
    {
        // 창고에 아이템 추가
        // (실제 구현 필요)
    }


    private void RemoveItemFromStorage(TradableItem item, int quantity)
    {
        // 창고에서 아이템 제거
        // (실제 구현 필요)
    }


    // 선원 관리 함수들
    public bool AddCrewMember(CrewMember newCrew)
    {
        if (crew.Count >= maxCrew)
            return false;

        crew.Add(newCrew);

        // 선원 숙소 업데이트
        UpdateCrewQuarters();

        return true;
    }

    public bool RemoveCrewMember(CrewMember crewToRemove)
    {
        if (!crew.Contains(crewToRemove))
            return false;

        crew.Remove(crewToRemove);

        // 선원 숙소 업데이트
        UpdateCrewQuarters();

        return true;
    }

    private void UpdateCrewQuarters()
    {
        // 함선 내 선원 숙소 찾기 및 최대 인원 계산
        int totalCapacity = 0;

        foreach (Room room in rooms)
            if (room is CrewQuartersRoom crewQuarters)
                totalCapacity += crewQuarters.maxCrewCapacity;

        // 최대 선원 수 업데이트
        maxCrew = totalCapacity;
    }

    // 선원 전체에 사기 보너스 적용
    public void ApplyMoraleBonusToAllCrew(float bonus)
    {
        foreach (CrewMember crewMember in crew) crewMember.AddMoraleBonus(bonus);
    }

    // 게터/세터 함수들
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
