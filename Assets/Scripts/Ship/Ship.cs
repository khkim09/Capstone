using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

/// <summary>
/// 함선의 전체 기능과 상태를 관리하는 클래스.
/// 방 배치, 시스템 초기화, 전투 처리, 자원 계산, 스탯 갱신 등의 기능을 포함합니다. 수정 확인
/// </summary>
public class Ship : MonoBehaviour
{
    /// <summary>
    /// 유저의 함선인지 여부 (true : 유저 함선, false : 적 함선)
    /// </summary>
    public bool isPlayerShip;

    [Header("Ship Info")][SerializeField] public string shipName = "Milky";

    /// <summary>
    /// 함선의 격자 크기 (방 배치 제한 범위).
    /// </summary>
    [SerializeField] private Vector2Int gridSize = new(60, 60);

    /// <summary>
    /// 디버그용 스탯 및 상태 정보를 출력할지 여부입니다.
    /// </summary>
    [SerializeField] private bool showDebugInfo = true;

    /// <summary>
    /// 각 격자 좌표에 배치된 룸 정보를 저장하는 그리드 맵.
    /// </summary>
    private readonly Dictionary<Vector2Int, Room> roomGrid = new();

    /// <summary>
    /// 함선을 이루고 있는 이미 배치된 모든 룸 객체 리스트.
    /// </summary>
    public List<Room> allRooms = new();

    /// <summary>전체 함선 무기 리스트</summary>
    public List<ShipWeapon> allWeapons = new();

    /// <summary>전체 아군 선원 리스트</summary>
    public List<CrewMember> allCrews = new();

    /// <summary>내 배의 전체 적군 선원 리스트</summary>
    public List<CrewMember> allEnemies = new();

    /// <summary>
    /// 아직 선원에게 적용시키지 않은 장비 목록
    /// </summary>
    public HashSet<EquipmentItem> unUsedItems = new();

    [SerializeField] private DoorData doorData;

    /// <summary>
    /// 함선의 모든 문에 대한 레벨 (전체 적용)
    /// </summary>
    private int doorLevel;

    /// <summary>
    /// 외갑판 데이터
    /// </summary>
    [Header("외갑판 설정")][SerializeField] public OuterHullData outerHullData;

    /// <summary>
    /// 외갑판 prefab
    /// </summary>
    [SerializeField] public GameObject outerHullPrefab;

    /// <summary>
    /// 룸 타입별로 분류된 룸 리스트 딕셔너리.
    /// 예: LifeSupport, Engine, Cockpit 등.
    /// </summary>
    private readonly Dictionary<RoomType, List<Room>> roomsByType = new();

    /// <summary>
    /// 현재 함선의 스탯 상태를 저장하는 딕셔너리.
    /// 예: 산소 생성량, 연료 효율, 쉴드 최대치 등.
    /// </summary>
    private readonly Dictionary<ShipStat, float> currentStats = new();

    /// <summary>
    /// 각 룸이 기여한 스탯 값을 저장하는 디버그용 데이터.
    /// 룸 이름을 키로 사용하여 스탯 기여도를 추적합니다.
    /// </summary>
    private readonly Dictionary<string, Dictionary<ShipStat, float>> roomContributions = new();

    /// <summary>
    /// 함선에 등록된 모든 시스템들.
    /// 타입을 키로 하여 각 ShipSystem 서브클래스를 관리합니다.
    /// </summary>
    private Dictionary<Type, ShipSystem> systems = new();

    private OuterHullSystem outerHullSystem;
    private WeaponSystem weaponSystem;
    private OxygenSystem oxygenSystem;
    private CrewSystem crewSystem;
    private HitPointSystem hitpointSystem;
    private MoraleSystem moraleSystem;
    private PowerSystem powerSystem;
    private StorageSystem storageSystem;
    private ShieldSystem shieldSystem;

    public OuterHullSystem OuterHullSystem => outerHullSystem;

    public WeaponSystem WeaponSystem => weaponSystem;

    public OxygenSystem OxygenSystem => oxygenSystem;

    public CrewSystem CrewSystem => crewSystem;

    public HitPointSystem HitpointSystem => hitpointSystem;

    public MoraleSystem MoraleSystem => moraleSystem;

    public PowerSystem PowerSystem => powerSystem;

    public StorageSystem StorageSystem => storageSystem;

    public ShieldSystem ShieldSystem => shieldSystem;

    public event Action OnStatsChanged;
    public event Action OnRoomChanged;

    /// <summary>
    /// 함선의 초기 상태를 설정합니다.
    /// 기본 스탯 설정 및 시스템 초기화를 수행합니다.
    /// </summary>
    private void Awake()
    {
    }

    /// <summary>
    /// 게임 시작 시 호출되며, 테스트 룸 배치 및 시스템 초기화를 수행합니다.
    /// </summary>
    private void Start()
    {
        // NOTE: 여기서 Initialize 호출 금지. 로드된 함선의 이중 초기화 때문에 그렇다. 초기화가 필요하면 필요한 곳에서 .Initialize 호출할 것
    }

    public void Initialize()
    {
        InitializeBaseStats();

        InitializeSystems();

        RecalculateAllStats();

        doorLevel = 1;
    }


    /// <summary>
    /// 게임 시작 시 호출되며, 테스트 룸 배치 및 시스템 초기화를 수행합니다.
    /// </summary>
    private void Update()
    {
        if (OuterHullSystem != null) OuterHullSystem.Update(Time.deltaTime);
        if (WeaponSystem != null) WeaponSystem.Update(Time.deltaTime);
        if (OxygenSystem != null) OxygenSystem.Update(Time.deltaTime);
        if (CrewSystem != null) CrewSystem.Update(Time.deltaTime);
        if (HitpointSystem != null) HitpointSystem.Update(Time.deltaTime);
        if (MoraleSystem != null) MoraleSystem.Update(Time.deltaTime);
        if (PowerSystem != null) PowerSystem.Update(Time.deltaTime);
        if (StorageSystem != null) StorageSystem.Update(Time.deltaTime);
        if (ShieldSystem != null) ShieldSystem.Update(Time.deltaTime);
    }

    /// <summary>
    /// Ship 오브젝트가 파괴될 때 호출됩니다.
    /// 모든 룸의 이벤트 구독을 해제하여 메모리 누수나 오류를 방지합니다.
    /// </summary>
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        foreach (Room room in allRooms)
            if (room != null)
                room.OnRoomStateChanged -= OnRoomStateChanged;
    }

    // ===== Room Management =====

    /// <summary>
    /// 룸을 생성하고 배치하며, 초기화는 Room의 Initialize 메서드를 사용합니다.
    /// </summary>
    public bool AddRoom(int level, RoomData roomData, Vector2Int position, Constants.Rotations.Rotation rotation)
    {
        // 룸 타입 확인
        RoomType roomType = roomData.GetRoomType();

        // 룸 오브젝트 생성
        GameObject roomObject = new($"Room_{roomData.GetRoomDataByLevel(level).roomName}");
        roomObject.transform.SetParent(transform);


        // 타입에 맞는 컴포넌트 추가
        Room room = AddRoomComponent(roomObject, roomType, roomData);
        if (room == null)
        {
            Debug.LogError($"Failed to create room component for {roomData.GetRoomDataByLevel(1).roomName}");
            Destroy(roomObject);
            return false;
        }

        // 필수 데이터 설정 (Initialize 전에 필요한 기본 설정)
        room.SetRoomData(roomData);
        room.roomType = roomType;
        room.position = position;
        room.currentRotation = rotation;

        // 체력 검사 색깔 판정
        // 타입 검사 종류 판점
        // room.UpdateRoomVisual();

        // 룸 위치 설정
        Vector2Int size = roomData.GetRoomDataByLevel(level).size;
        Vector2Int rotatedSize = RoomRotationUtility.GetRotatedSize(size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(rotatedSize, rotation);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        room.gameObject.transform.position = worldPos + new Vector3(0, 0, 5f);
        room.gameObject.transform.rotation = Quaternion.Euler(0, 0, -(int)rotation * 90);

        room.Initialize(level);

        room.UpdateRoomVisual();

        // 룸 목록에 추가
        allRooms.Add(room);

        // 방 갱신 이벤트 발생
        OnRoomChanged?.Invoke();

        // 타입별 딕셔너리에 추가
        if (!roomsByType.ContainsKey(roomType))
            roomsByType[roomType] = new List<Room>();
        roomsByType[roomType].Add(room);

        // 룸 상태 변경 이벤트 등록
        room.OnRoomStateChanged += OnRoomStateChanged;

        // 그리드에 추가
        AddRoomToGrid(room, size);

        // 스탯 재계산
        RecalculateAllStats();

        return true;
    }

    /// <summary>
    /// 룸 생성 및 그리드 배치
    /// </summary>
    /// <param name="room"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public Room AddRoom(Room room, Vector2Int position = new(),
        Constants.Rotations.Rotation rotation = Constants.Rotations.Rotation.Rotation0)
    {
        // 룸 타입 확인
        RoomType roomType = room.GetRoomType();

        // 각 방 부모로 playerShip 할당
        room.transform.SetParent(transform);

        // 방에 기존 세팅된 position이 있을 경우 (json으로 불러오는 방)
        if (position != Vector2Int.zero)
            room.position = position;
        else // 새롭게 생성하는 방 : room.position을 0으로 세팅
            position = room.position;

        // 방의 회전 값 세팅
        if (rotation == Constants.Rotations.Rotation.Rotation0)
            rotation = room.currentRotation;
        else
            room.currentRotation = rotation;

        // 필수 데이터 설정
        RoomData roomData = room.GetRoomData();

        // 룸 위치 설정
        Vector2Int size = roomData.GetRoomDataByLevel(room.GetCurrentLevel()).size;
        Vector2Int rotatedSize = RoomRotationUtility.GetRotatedSize(size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(rotatedSize, rotation);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        // 실제 룸 배치 작업
        room.gameObject.transform.position = worldPos + new Vector3(0, 0, 5f);
        room.gameObject.transform.rotation = Quaternion.Euler(0, 0, -(int)rotation * 90);

        room.UpdateRoomVisual();

        // 룸 목록에 추가
        allRooms.Add(room);

        // 타입별 딕셔너리에 추가
        if (!roomsByType.ContainsKey(roomType))
            roomsByType[roomType] = new List<Room>();
        roomsByType[roomType].Add(room);

        // 방 갱신 이벤트 발생 (일단 불필요)
        // OnRoomChanged?.Invoke();

        // 룸 상태 변경
        room.OnRoomStateChanged += OnRoomStateChanged;

        // 그리드에 추가
        AddRoomToGrid(room, size);

        // 스탯 재계산
        RecalculateAllStats();

        return room;
    }

    /// <summary>
    /// 그리드에 룸을 추가합니다.
    /// </summary>
    private void AddRoomToGrid(Room room, Vector2Int size)
    {
        List<Vector2Int> occupiedTiles =
            RoomRotationUtility.GetOccupiedGridPositions(room.position, size, room.currentRotation);

        foreach (Vector2Int tile in occupiedTiles)
            roomGrid[tile] = room;
    }

    /// <summary>
    /// 지정된 그리드 위치에 있는 룸을 반환합니다.
    /// </summary>
    public Room GetRoomAtPosition(Vector2Int position)
    {
        if (roomGrid.TryGetValue(position, out Room room))
            return room;
        return null;
    }


    /// <summary>
    /// 룸 타입에 맞는 컴포넌트를 추가합니다.
    /// </summary>
    private Room AddRoomComponent(GameObject roomObject, RoomType roomType, RoomData roomData)
    {
        switch (roomType)
        {
            case RoomType.Power:
                return roomObject.AddComponent<PowerRoom>();
            case RoomType.Cockpit:
                return roomObject.AddComponent<CockpitRoom>();
            case RoomType.Teleporter:
                return roomObject.AddComponent<TeleportRoom>();
            case RoomType.LifeSupport:
                return roomObject.AddComponent<LifeSupportRoom>();
            case RoomType.Ammunition:
                return roomObject.AddComponent<AmmunitionRoom>();
            case RoomType.WeaponControl:
                return roomObject.AddComponent<WeaponControlRoom>();
            case RoomType.MedBay:
                return roomObject.AddComponent<MedBayRoom>();
            case RoomType.Shield:
                return roomObject.AddComponent<ShieldRoom>();
            case RoomType.Engine:
                return roomObject.AddComponent<EngineRoom>();
            case RoomType.Oxygen:
                return roomObject.AddComponent<OxygenRoom>();
            case RoomType.CrewQuarters:
                return roomObject.AddComponent<CrewQuartersRoom>();
            case RoomType.Storage:
                // RoomData에서 StorageType 가져오기
                if (roomData is StorageRoomBaseData storageData)
                {
                    StorageType storageType = storageData.storageType;

                    StorageRoomBase room = storageType switch
                    {
                        StorageType.Regular => roomObject.AddComponent<StorageRoomRegular>(),
                        StorageType.Temperature => roomObject.AddComponent<StorageRoomTemperature>(),
                        StorageType.Animal => roomObject.AddComponent<StorageRoomAnimal>(),
                        _ => roomObject.AddComponent<StorageRoomRegular>()
                    };

                    room.SetStorageType(storageType); // setter 사용
                    return room;
                }

                break;
            case RoomType.Corridor:
                return roomObject.AddComponent<CorridorRoom>();
            default:
                return null;
        }

        return null;
    }


    /// <summary>
    /// 지정한 룸을 함선에서 제거합니다.
    /// </summary>
    /// <param name="room">제거할 룸 객체.</param>
    /// <returns>제거 성공 여부.</returns>
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
        // OnRoomChanged?.Invoke(); // 방 갱신 (일단 불필요)

        Destroy(room.gameObject);

        // TODO: MoraleManager에서 사기 계산하기 해야됨

        // Recalculate stats
        RecalculateAllStats();

        return true;
    }

    public void RemoveAllRooms()
    {
        for (int i = allRooms.Count - 1; i >= 0; i--) RemoveRoom(allRooms[i]);
    }

    #region 설계도

    // ---------------- 함선 커스터마이징 관련 추가 <기현> ----------------

    /// <summary>
    /// 백업해 둘 방 정보들
    /// </summary>
    public List<RoomBackupData> backupRoomDatas = new();

    /// <summary>
    /// 백업해 둘 함선 무기 정보들
    /// </summary>
    public List<WeaponBackupData> backupWeapons = new();

    /// <summary>
    /// 백업해 둘 선원 정보들
    /// </summary>
    public List<BackupCrewData> backupCrewDatas = new();

    /// <summary>
    /// 현재 함선에 포함된 모든 방의 가격 합을 반환
    /// </summary>
    /// <returns>모든 방 가격의 합</returns>
    public int GetTotalShipValue()
    {
        int total = 0;
        foreach (Room room in allRooms) total += room.GetRoomData().GetRoomDataByLevel(room.GetCurrentLevel()).cost;
        return total;
    }

    /// <summary>
    /// 현재 함선의 모든 방에 대해 내구도 100%인지 여부 반환
    /// </summary>
    /// <returns>내구도 100% 여부</returns>
    public bool IsFullHitPoint()
    {
        foreach (Room room in allRooms)
            if (room.currentHitPoints != room.GetMaxHitPoints())
                return false;
        return true;
    }

    /// <summary>
    /// 설계도 작업 전, 기존 선원 백업
    /// </summary>
    public void BackupAllCrews()
    {
        backupCrewDatas.Clear();

        foreach (CrewMember crew in allCrews)
            backupCrewDatas.Add(new BackupCrewData
            {
                race = crew.race,
                crewName = crew.crewName,
                needsOxygen = crew.needsOxygen,
                position = crew.position,
                roomPos = crew.currentRoom.position
            });
    }

    /// <summary>
    /// 설계도 교체 작업 전, 기존 함선 백업
    /// </summary>
    public void BackupCurrentShip()
    {
        backupRoomDatas.Clear();
        backupWeapons.Clear();

        foreach (Room room in allRooms)
            backupRoomDatas.Add(new RoomBackupData
            {
                roomData = room.GetRoomData(),
                level = room.GetCurrentLevel(),
                position = room.position,
                rotation = room.currentRotation
            });

        foreach (ShipWeapon wp in GetAllWeapons())
            backupWeapons.Add(new WeaponBackupData()
            {
                weaponData = wp.weaponData,
                position = wp.GetGridPosition(),
                direction = wp.GetAttachedDirection()
            });
    }

    /// <summary>
    /// 유효성 검사 실패, 기존 함선으로 복구
    /// </summary>
    public void RevertToOriginalShip()
    {
        // 설계도 방 제거
        foreach (Room room in allRooms)
            if (room != null)
                Destroy(room.gameObject);

        allRooms.Clear();

        // 설계도 무기 제거
        foreach (ShipWeapon weapon in allWeapons)
            if (weapon != null)
                Destroy(weapon.gameObject);

        allWeapons.Clear();

        // 기존 함선으로 복구
        foreach (RoomBackupData backupData in backupRoomDatas)
        {
            AddRoom(backupData.level, backupData.roomData, backupData.position, backupData.rotation);
            Room placed = GetRoomAtPosition(backupData.position);
        }

        foreach (WeaponBackupData backupData in backupWeapons)
            AddWeapon(backupData.weaponData.id, backupData.position, backupData.direction);
    }

    /// <summary>
    /// 현재 설계도 함선을 실제 함선 구조로 반영
    /// (방뿐 아니라 무기도 함께 적용)
    /// </summary>
    /// <param name="bpShip">설계도 함선</param>
    public void ReplaceShipFromBlueprint(BlueprintShip bpShip)
    {
        // 1. 기존 선원 모두 삭제
        foreach (CrewMember crew in allCrews)
            Destroy(crew.gameObject);

        allCrews.Clear();

        // 2. 기존 방 삭제
        foreach (Room room in allRooms)
            Destroy(room.gameObject);

        allRooms.Clear();
        roomGrid.Clear(); // 그리드 정보도 초기화
        roomsByType.Clear(); // 타입별 룸 목록도 초기화

        // 3. 기존 무기 삭제
        //    먼저 현재 함선에 있는 모든 무기를 안전하게 리스트로 복사한 뒤 제거
        foreach (ShipWeapon weapon in allWeapons)
            Destroy(weapon.gameObject);
        allWeapons.Clear();

        // 4. 외갑판 레벨 - 설계도 함선의 외갑판 레벨을 실제 함선에 적용
        //    (무기마다 적용하지 않고 함선 전체에 한 번만 적용)
        int blueprintHullLevel = bpShip.GetHullLevel();
        Debug.Log($"Converting blueprint to ship with hull level: {blueprintHullLevel}");
        SetOuterHullLevel(blueprintHullLevel);

        // 5. 설계도 → 함선: 방 배치
        foreach (BlueprintRoom bpRoom in bpShip.GetComponentsInChildren<BlueprintRoom>())
            AddRoom(
                bpRoom.bpLevelIndex,
                bpRoom.bpRoomData,
                bpRoom.bpPosition,
                bpRoom.bpRotation
            );

        // 6. 설계도 → 함선: 무기 배치
        foreach (BlueprintWeapon bpWeapon in bpShip.GetComponentsInChildren<BlueprintWeapon>())
        {
            // ShipWeaponData에서 ID를 꺼내 실제 무기를 생성
            ShipWeapon newWeapon = AddWeapon(
                bpWeapon.bpWeaponData.id,
                bpWeapon.bpPosition,
                bpWeapon.bpAttachedDirection
            );

            // 함선의 외갑판 레벨에 맞게 스프라이트 적용
            // 이미 SetOuterHullLevel이 적용되었으므로 현재 함선의 외갑판 레벨 사용
            if (newWeapon != null) newWeapon.ApplyRotationSprite(GetOuterHullLevel());
        }
    }

    // ---------------- <기현> 여기까지 --------------------

    #endregion

    #region RTS 이동 위한 타일 관리

    #endregion

    /// <summary>
    /// 주어진 위치와 크기로 방을 배치할 수 있는지 검사합니다.
    /// 격자 범위를 초과하거나 겹치는 경우 false를 반환합니다.
    /// </summary>
    /// <param name="pos">좌측 상단 기준 시작 위치.</param>
    /// <param name="size">방의 크기 (가로 x 세로).</param>
    /// <returns>배치 가능하면 true, 불가능하면 false.</returns>
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


    /// <summary>
    /// 특정 타입의 방(Room)을 검색하여 반환합니다.
    /// 해당 타입의 첫 번째 방만 반환되며, 없으면 null입니다.
    /// </summary>
    /// <typeparam name="T">찾고자 하는 Room 타입.</typeparam>
    /// <returns>해당 타입의 Room 또는 null.</returns>
    public T GetRoomOfType<T>() where T : Room
    {
        foreach (Room room in allRooms)
            if (room is T typedRoom)
                return typedRoom;
        return null;
    }

    /// <summary>
    /// 특정 룸 타입(RoomType)에 해당하는 모든 방을 반환합니다.
    /// </summary>
    /// <param name="type">조회할 룸 타입.</param>
    /// <returns>해당 타입의 방 리스트.</returns>
    public List<Room> GetRoomsByType(RoomType type)
    {
        if (roomsByType.ContainsKey(type))
            return roomsByType[type];
        return new List<Room>();
    }

    #region 시스템

    /// <summary>
    /// 함선에 필요한 시스템들을 초기화하고 등록합니다.
    /// 모든 ShipSystem 서브클래스를 수동으로 등록해야 합니다.
    /// </summary>
    private void InitializeSystems()
    {
        // TODO : 방 시스템 만들 때마다 여기에 등록
        shieldSystem = new ShieldSystem();
        weaponSystem = new WeaponSystem();
        outerHullSystem = new OuterHullSystem();
        crewSystem = new CrewSystem();
        moraleSystem = new MoraleSystem();
        storageSystem = new StorageSystem();
        hitpointSystem = new HitPointSystem();
        oxygenSystem = new OxygenSystem();
        powerSystem = new PowerSystem();

        shieldSystem.Initialize(this);
        weaponSystem.Initialize(this);
        outerHullSystem.Initialize(this);
        crewSystem.Initialize(this);
        moraleSystem.Initialize(this);
        storageSystem.Initialize(this);
        hitpointSystem.Initialize(this);
        oxygenSystem.Initialize(this);
        powerSystem.Initialize(this);
    }

    #endregion


    #region 함선 스탯

    /// <summary>
    /// 함선의 기본 스탯을 초기화합니다.
    /// 모든 ShipStat 값을 0으로 설정하여 이후 계산의 기준을 만듭니다.
    /// </summary>
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

    /// <summary>
    /// 현재 함선의 모든 스탯을 다시 계산합니다.
    /// 방, 시스템, 선원의 기여도를 반영합니다.
    /// </summary>
    public void RecalculateAllStats()
    {
        // 기본값으로 초기화
        InitializeBaseStats();

        // 디버깅용 기여도 초기화
        roomContributions.Clear();

        MoraleManager.Instance.SetAllCrewMorale(MoraleSystem.CalculateGlobalMorale());

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
        List<CrewMember> crews = CrewSystem.GetCrews();

        foreach (CrewMember crew in crews)
        {
            Dictionary<ShipStat, float> crewContributions = crew.GetStatContributions();

            if (crewContributions.TryGetValue(ShipStat.OxygenUsingPerSecond, out float oxygenUsage))
                currentStats[ShipStat.OxygenUsingPerSecond] += oxygenUsage;
        }

        // 디버깅 정보 출력
        if (showDebugInfo)
            PrintDebugStatInfo();

        // 스탯 변경 이벤트 발생
        OnStatsChanged?.Invoke();
    }

    // TODO: 스탯 추가할 때마다 합연산인지 곱연산인지 분류


    /// <summary>
    /// 주어진 ShipStat이 가산형(덧셈 방식) 스탯인지 여부를 반환합니다.
    /// 예: 쉴드 최대량, 산소 생성량 등은 단순 덧셈으로 누적됩니다.
    /// </summary>
    /// <param name="statType">확인할 스탯 타입.</param>
    /// <returns>가산형이면 true, 아니면 false.</returns>
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

    /// <summary>
    /// 주어진 ShipStat이 배율형(곱셈 방식) 스탯인지 여부를 반환합니다.
    /// 예: 회피율 보너스 %, 데미지 증폭 % 등이 해당됩니다.
    /// 현재는 정의된 곱셈 스탯이 없습니다.
    /// </summary>
    /// <param name="statType">확인할 스탯 타입.</param>
    /// <returns>배율형이면 true, 아니면 false.</returns>
    private bool IsMultiplicativeStatType(ShipStat statType)
    {
        switch (statType)
        {
            default:
                return false;
        }
    }

    /// <summary>
    /// 현재 함선의 스탯과 룸별 스탯 기여도를 콘솔에 출력합니다.
    /// 디버깅 모드가 활성화된 경우에만 호출되며, 전체 스탯 상태를 확인할 수 있습니다.
    /// </summary>
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

    /// <summary>
    /// 특정 스탯 값을 반환합니다.
    /// </summary>
    /// <param name="statType">조회할 스탯.</param>
    /// <returns>스탯 값. 없으면 0.</returns>
    public float GetStat(ShipStat statType)
    {
        if (currentStats.TryGetValue(statType, out float value))
            return value;

        return 0f;
    }

    /// <summary>
    /// 지정한 룸이 기여한 스탯 정보를 반환합니다.
    /// 주로 디버깅 또는 UI 용도로 사용됩니다.
    /// </summary>
    /// <param name="roomName">조회할 룸의 이름.</param>
    /// <returns>
    /// 해당 룸이 기여한 ShipStat 딕셔너리.
    /// 존재하지 않으면 빈 딕셔너리를 반환합니다.
    /// </returns>
    public Dictionary<ShipStat, float> GetRoomContributions(string roomName)
    {
        if (roomContributions.TryGetValue(roomName, out Dictionary<ShipStat, float> contributions))
            return contributions;

        return new Dictionary<ShipStat, float>();
    }

    #endregion


    /// <summary>
    /// 룸의 상태가 변경되었을 때 호출되는 콜백 함수입니다.
    /// 디버그 로그를 출력하고 전체 스탯을 다시 계산합니다.
    /// </summary>
    /// <param name="room">상태가 변경된 룸 객체.</param>
    private void OnRoomStateChanged(Room room)
    {
        if (showDebugInfo)
            Debug.Log($"Room state changed: {room.name}");

        RecalculateAllStats();
    }


    // ===== Power Management =====


    /// <summary>
    /// 함선이 파괴되었을 때 호출되는 함수입니다.
    /// 게임 오버 로직 등을 처리할 수 있습니다.
    /// </summary>
    public void OnShipDestroyed()
    {
        Debug.Log($"Ship {shipName} destroyed!");
        // Implement game over logic
    }

    // ===== Warp System =====

    public bool Warp()
    {
        float fuelCost = CalculateWarpFuelCost();

        if (ResourceManager.Instance.Fuel < fuelCost)
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

    /// <summary>
    /// 워프 시 필요한 연료 소모량을 계산합니다.
    /// </summary>
    /// <returns>연료 비용.</returns>
    public float CalculateWarpFuelCost()
    {
        float fuelCost = GetStat(ShipStat.FuelConsumption);
        float fuelEfficiency = GetStat(ShipStat.FuelEfficiency);
        fuelCost *= 1 - fuelEfficiency / 100f;

        if (showDebugInfo)
            Debug.Log($"Warp fuel cost: {fuelCost}");

        return fuelCost;
    }

    #region 선원

    /// <summary>
    /// 현재 탑승 중인 크루 수를 반환합니다.
    /// </summary>
    public int GetCrewCount()
    {
        return CrewSystem.GetCrewCount();
    }

    /// <summary>
    /// 최대 크루 수(수용 가능 인원)를 반환합니다.
    /// </summary>
    public int GetMaxCrew()
    {
        return (int)currentStats[ShipStat.CrewCapacity];
    }

    /// <summary>
    /// 현재 함선에 탑승 중인 모든 크루를 반환합니다.
    /// </summary>
    /// <returns>CrewMember 객체들의 리스트.</returns>
    public List<CrewMember> GetAllCrew()
    {
        return CrewSystem.GetCrews();
    }

    /// <summary>
    /// 유저의 선원 리스트 업데이트
    /// </summary>
    public void UpdateCrewList()
    {
        allCrews.Clear();

        foreach (CrewMember cm in GetAllCrew())
            allCrews.Add(cm);
    }

    /// <summary>
    /// 새로운 승무원을 함선에 추가합니다.
    /// </summary>
    /// <param name="newCrew">함선에 추가할 승무원 정보. 이름, 종족, 속성 등을 포함합니다.</param>
    /// <returns>승무원이 성공적으로 추가되었으면 True, 그렇지 않으면 False를 반환합니다.</returns>
    public bool AddCrew(CrewBase newCrew)
    {
        return CrewSystem.AddCrew(newCrew);
    }

    /// <summary>
    /// 선원 제거
    /// </summary>
    /// <param name="crew"></param>
    public void RemoveCrew(CrewMember crew)
    {
        CrewSystem.RemoveCrew(crew);
    }

    /// <summary>
    /// 모든 선원 제거
    /// </summary>
    public void RemoveAllCrews()
    {
        foreach (CrewMember crew in allCrews)
            Destroy(crew.gameObject);

        allCrews.Clear();
        // GetSystem<CrewSystem>().crews.Clear();

        UpdateCrewList();
        // CrewSystem.RemoveAllCrews();
    }

    #endregion


    /// <summary>
    /// 모든 룸 정보를 반환합니다.
    /// </summary>
    public List<Room> GetAllRooms()
    {
        return allRooms;
    }

    /// <summary>
    /// 랜덤한 방 반환
    /// </summary>
    /// <returns></returns>
    public Room GetRandomRoom()
    {
        List<Room> rooms = GetAllRooms();
        if (rooms == null || rooms.Count == 0)
            return null;

        return rooms[Random.Range(0, rooms.Count)];
    }

    public void AllFreeze()
    {
        foreach (CrewMember crew in GetAllCrew())
            if (crew != null)
            {
                crew.Freeze();
                crew.BackToThePeace();
            }
    }

    #region 무기

    /// <summary>
    /// 현재 탑재 중인 모든 무기를 반환합니다.
    /// </summary>
    public List<ShipWeapon> GetAllWeapons()
    {
        return allWeapons;
    }

    public void RemoveAllWeapons()
    {
        foreach (ShipWeapon weapon in allWeapons)
            Destroy(weapon.gameObject);

        allWeapons.Clear();
    }

    /// <summary>
    /// 무기 시스템 수정자를 적용한 후의 실제 피해량을 계산합니다.
    /// </summary>
    /// <param name="damage">수정 전 원래 피해 값.</param>
    /// <returns>무기 특유 효과를 고려한 수정된 피해 값.</returns>
    public float GetActualDamage(float damage)
    {
        return WeaponSystem.GetActualDamage(damage);
    }

    public int GetWeaponCount()
    {
        return WeaponSystem.GetWeaponCount();
    }

    public void RemoveWeapon(ShipWeapon shipWeapon)
    {
        WeaponSystem.RemoveWeapon(shipWeapon);
    }

    public ShipWeapon AddWeapon(int weaponId, Vector2Int gridPosition,
        ShipWeaponAttachedDirection attachDirection = ShipWeaponAttachedDirection.East)
    {
        return WeaponSystem.AddWeapon(weaponId, gridPosition, attachDirection);
    }

    public ShipWeapon AddWeapon(ShipWeapon shipWeapon)
    {
        return WeaponSystem.AddWeapon(shipWeapon);
    }

    #endregion

    #region 체력

    /// <summary>
    /// 현재 함선의 내구도(Hit Point)를 반환합니다.
    /// </summary>
    /// <returns>현재 남아 있는 함선의 체력 값.</returns>
    public float GetCurrentHitPoints()
    {
        return HitpointSystem.GetHitPoint();
    }

    #endregion

    #region 피격

    /// <summary>
    /// 무작위로 타겟팅 가능한 방의 위치를 반환합니다.
    /// 방이 없는 경우 (모두 파괴되었거나 타겟 불가) Vector2Int.zero를 반환합니다.
    /// </summary>
    /// <returns>타겟팅 가능한 방의 격자 좌표.</returns>
    public Vector2Int GetRandomTargetPosition()
    {
        List<Vector2Int> targetPositions = new();
        foreach (Room r in allRooms)
            foreach (Vector2Int tile in r.GetOccupiedTiles())
                targetPositions.Add(tile);

        Vector2Int randomPosition = targetPositions[Random.Range(0, targetPositions.Count)];

        return randomPosition;
    }

    /// <summary>
    /// 공격 대상으로 선택 가능한 방 중 하나를 무작위로 반환합니다.
    /// 피해를 받을 수 있고 체력이 남아 있는 방만 대상이 됩니다.
    /// </summary>
    /// <returns>타겟팅 가능한 방 객체. 없으면 null.</returns>
    private Room GetRandomTargettableRoom()
    {
        List<Room> validRooms = GetAllRooms().FindAll(room => room.GetIsDamageable() && room.GetHealthPercentage() > 0);

        if (validRooms.Count == 0) return null;

        int randomIndex = Random.Range(0, validRooms.Count);
        return validRooms[randomIndex];
    }

    /// <summary>
    /// 무기 및 외부 방어 시스템을 포함한 실제 피해 계산 및 적용을 수행합니다.
    /// </summary>
    /// <param name="damage">입력된 피해량.</param>
    /// <param name="shipWeaponType">공격한 무기 타입.</param>
    /// <param name="hitPosition">피격된 좌표.</param>
    public void TakeAttack(float damage, ShipWeaponType shipWeaponType, Vector2Int hitPosition)
    {
        ShieldSystem shieldSystem = ShieldSystem;
        if (shieldSystem.IsShieldActive()) damage = shieldSystem.TakeDamage(damage, shipWeaponType);
        OuterHullSystem hullSystem = OuterHullSystem;
        float finalDamage = hullSystem.ReduceDamage(damage);

        Debug.Log($"쉴드로 감소된 최종 데미지 : {finalDamage}");

        if (finalDamage > 0)
        {
            // 함선 전체에 데미지 적용
            TakeDamage(finalDamage);

            if (shipWeaponType == ShipWeaponType.Missile)
            {
                // 미사일이 직접 떨어진 위치의 방에만 데미지 적용
                if (roomGrid.TryGetValue(hitPosition, out Room hitRoom))
                {
                    Debug.Log($"피격 방 : {hitRoom}, 피격 지점 : {hitPosition}");
                    hitRoom.TakeDamage(finalDamage);
                }

                // 직접 타격 지점과 주변 8칸에 있는 선원들에게 데미지 적용
                ApplyDamageToCrewsInArea(hitPosition, finalDamage, true); // true = 3x3 영역 스플래시
            }
            else
            {
                // 단일 지점 데미지 - 방에 데미지 적용
                if (roomGrid.TryGetValue(hitPosition, out Room hitRoom))
                {
                    Debug.Log($"피격 방 : {hitRoom}, 피격 지점 : {hitPosition}");
                    hitRoom.TakeDamage(finalDamage);
                }

                // 그 위치에 있는 선원들에게 데미지 적용
                ApplyDamageToCrewsInArea(hitPosition, finalDamage, false); // false = 단일 지점
            }
        }
        InfoPanelChanged?.Invoke();
    }


    /// <summary>
    /// 함선에 직접 피해를 적용합니다.
    /// 체력을 감소시키고, 파괴되었는지 확인합니다.
    /// </summary>
    /// <param name="damage">적용할 피해량.</param>
    public void TakeDamage(float damage)
    {
        HitPointSystem hitPointSystem = HitpointSystem;
        hitPointSystem.ChangeHitPoint(-damage);

        if (hitPointSystem.GetHitPoint() <= 0f) OnShipDestroyed();
    }

    /// <summary>
    /// 지정된 방에 있는 모든 크루에게 피해를 적용합니다.
    /// </summary>
    /// <param name="room">대상 방.</param>
    /// <param name="damage">적용할 피해량.</param>
    private void ApplyDamageToCrewsInRoom(Room room, float damage)
    {
        List<CrewMember> totalCrews = room.GetTotalCrewsInRoom();

        foreach (CrewMember eachCrew in totalCrews)
            eachCrew.TakeDamage(damage);
    }


    /// <summary>
    /// 지정된 위치에 있는 크루에게 데미지를 적용합니다.
    /// </summary>
    /// <param name="position">피격된 격자 좌표.</param>
    /// <param name="damage">적용할 원 피해량.</param>
    private void ApplyDamageToCrewsAtPosition(Vector2Int position, float damage)
    {
        List<CrewMember> crewsAtPosition = CrewSystem.GetCrewsAtPosition(position);
        foreach (CrewMember crew in crewsAtPosition)
        {
            Debug.LogError($"피격 지점 : {position}, {damage} 데미지 선원 : {crew}");
            crew.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 해당하는 좌표에 있는 크루에게 데미지를 적용합니다.
    /// 만약 스플래쉬 데미지일 경우 중심 좌표를 기준으로 3x3 영역(주위 8칸)에는 80%의 데미지를 입힙니다.
    /// </summary>
    /// <param name="position">피격된 격자 좌표.</param>
    /// <param name="damage">적용할 원 피해량.</param>
    /// <param name="isSplash">스플래쉬 데미지 여부.</param>
    private void ApplyDamageToCrewsInArea(Vector2Int position, float damage, bool isSplash)
    {
        // 단일 지점에 있는 선원에게 데미지 적용
        ApplyDamageToCrewsAtPosition(position, damage);


        if (isSplash)
            // 3x3 영역 내 선원들에게 데미지 적용
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    Vector2Int checkPos = position + new Vector2Int(x, y);

                    // 해당 위치에 있는 선원들에게 데미지 적용
                    ApplyDamageToCrewsAtPosition(checkPos, damage * 0.8f);
                }
    }

    #endregion

    #region 산소

    /// <summary>
    /// 현재 산소량을 반환합니다.
    /// </summary>
    /// <returns>현재 산소량.</returns>
    public float GetOxygenRate()
    {
        return OxygenSystem.GetOxygenRate();
    }

    /// <summary>
    /// 현재 산소 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 산소 레벨.</returns>
    public OxygenLevel GetOxygenLevel()
    {
        return OxygenSystem.GetOxygenLevel();
    }

    #endregion

    #region 문

    /// <summary>
    /// 문 데이터를 반환합니다.
    /// </summary>
    /// <returns>문과 관련된 정보 및 구성 데이터를 포함하는 <see cref="DoorData"/> 객체를 반환합니다.</returns>
    public DoorData GetDoorData()
    {
        return doorData;
    }

    /// <summary>
    /// 현재 함선의 문 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 문 레벨 값.</returns>
    public int GetDoorLevel()
    {
        return doorLevel;
    }

    public DoorData.DoorLevel GetCurrentDoorData()
    {
        return doorData.GetDoorData(doorLevel);
    }

    #endregion

    #region Grid

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector3 local = new Vector3(worldPos.x, worldPos.y, 0) - Vector3.zero;
        return new Vector2Int(Mathf.FloorToInt(local.x / Constants.Grids.CellSize),
            Mathf.FloorToInt(local.y / Constants.Grids.CellSize));
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환 ((0, 0) ~ (60, 60) 그리드 내 좌표라 local relative 좌표임)
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return Vector3.zero + new Vector3((gridPos.x + 0.5f) * Constants.Grids.CellSize,
            (gridPos.y + 0.5f) * Constants.Grids.CellSize, 0f);
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환 ((0, 0) ~ (60, 60) 그리드 내 좌표라 local relative 좌표임)
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GridToWorldPosition(Vector2 gridPos)
    {
        return Vector3.zero + new Vector3((gridPos.x + 0.5f) * Constants.Grids.CellSize,
            (gridPos.y + 0.5f) * Constants.Grids.CellSize, 0f);
    }

    /// <summary>
    /// 실제 월드 위치 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        return transform.TransformPoint(GridToWorldPosition(gridPos));
    }

    /// <summary>
    /// 실제 월드 위치 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GetWorldPositionFromGrid(Vector2 gridPos)
    {
        return transform.TransformPoint(GridToWorldPosition(gridPos));
    }

    public Vector2Int GetGridSize()
    {
        return gridSize;
    }

    #endregion

    #region 창고

    public List<TradingItem> GetAllItems()
    {
        return StorageSystem.GetAllItems();
    }

    public void RemoveAllItems()
    {
        StorageSystem.RemoveAllItems();
    }

    #endregion

    #region 외갑판

    /// <summary>
    /// 외갑판 데이터를 반환합니다.
    /// </summary>
    /// <returns>외갑판 데이터</returns>
    public OuterHullData GetOuterHullData()
    {
        return outerHullData;
    }

    /// <summary>
    /// 외갑판 프리팹을 반환합니다.
    /// </summary>
    /// <returns>외갑판 프리팹</returns>
    public GameObject GetOuterHullPrefab()
    {
        return outerHullPrefab;
    }

    /// <summary>
    /// 현재 함선의 외갑판 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 외갑판 레벨 (0-2)</returns>
    public int GetOuterHullLevel()
    {
        return OuterHullSystem.GetOuterHullLevel();
    }

    /// <summary>
    /// 함선의 외갑판 레벨을 설정합니다.
    /// </summary>
    /// <param name="level">설정할 외갑판 레벨 (0-2)</param>
    public void SetOuterHullLevel(int level)
    {
        OuterHullSystem.SetOuterHullLevel(level);
    }

    /// <summary>
    /// 외갑판 시각 효과를 업데이트합니다.
    /// </summary>
    public void UpdateOuterHullVisuals()
    {
        OuterHullSystem outerSystem = OuterHullSystem;
        if (outerSystem != null)
        {
            int currentLevel = outerSystem.GetOuterHullLevel();
            outerSystem.UpdateVisuals(currentLevel);
        }
    }

    /// <summary>
    /// 기존 외갑판 객체들을 모두 제거합니다.
    /// </summary>
    public void ClearExistingHulls()
    {
        OuterHullSystem.ClearExistingHulls();
    }

    #endregion

    #region 함선 이동 (적 함선)

    /// <summary>
    /// 이 함선을 targetShip과 마주보도록 설정하면서 x축으로 offset만큼 띄운 위치로 이동시킴.
    /// 회전은 180도, y 중심 정렬 유지.
    /// </summary>
    /// <param name="targetShip">대상 유저 함선</param>
    /// <param name="xOffset">x축 거리 (보통 80)</param>
    public void MoveShipToFacingTargetShip(Ship targetShip)
    {
        float xOffset = 80f;
        if (targetShip == null || targetShip.allRooms.Count == 0 || allRooms.Count == 0)
        {
            Debug.LogError("MoveShipToFacingTargetShip 실패: 유효하지 않은 ship 구성");
            return;
        }

        // 1. 유저 함선의 중심 월드 좌표 계산
        Vector3 userSum = Vector3.zero;
        foreach (Room room in targetShip.allRooms)
            userSum += room.transform.position;
        Vector2 userCenter = userSum / targetShip.allRooms.Count;

        // 2. 이(적군) 함선의 중심 월드 좌표 계산
        Vector3 enemySum = Vector3.zero;
        foreach (Room room in allRooms)
            enemySum += room.transform.position;
        Vector2 enemyCenter = enemySum / allRooms.Count;

        // 3. 회전 (정면 반대로)
        transform.rotation = Quaternion.Euler(0, 0, 180f);

        // 4. 적 함선과 유저 함선의 월드 좌표 위치 차이 계산
        float diffX = userCenter.x + enemyCenter.x + xOffset;
        float diffY = userCenter.y + enemyCenter.y;

        transform.position = new Vector3(diffX, diffY, 10f);
    }

    #endregion

    #region EnemyInfoPanel

    /// <summary>
    /// EnemyInfoPanel 갱신을 위한 이벤트
    /// </summary>
    public event Action InfoPanelChanged;

    #endregion

}
