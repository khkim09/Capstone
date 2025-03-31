using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 함선의 전체 기능과 상태를 관리하는 클래스.
/// 방 배치, 시스템 초기화, 전투 처리, 자원 계산, 스탯 갱신 등의 기능을 포함합니다.
/// </summary>
public class Ship : MonoBehaviour
{
    [Header("Ship Info")][SerializeField] private string shipName = "Milky";

    /// <summary>
    /// 함선의 격자 크기 (방 배치 제한 범위).
    /// </summary>
    [SerializeField] private Vector2Int gridSize = new(20, 20);

    /// <summary>
    /// 디버그용 스탯 및 상태 정보를 출력할지 여부입니다.
    /// </summary>
    [SerializeField] private bool showDebugInfo = true;

    /// <summary>
    /// 각 격자 좌표에 배치된 룸 정보를 저장하는 그리드 맵.
    /// </summary>
    private readonly Dictionary<Vector2Int, Room> roomGrid = new();


    /// <summary>
    /// 함선 내의 모든 문(도어) 객체 리스트.
    /// </summary>
    private readonly List<Door> doors = new();

    /// <summary>
    /// 함선을 이루고 있는 이미 배치된 모든 룸 객체 리스트.
    /// </summary>
    public List<Room> allRooms = new();

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

    // 테스트용 룸 프리팹
    public GameObject testRoomPrefab1;
    public GameObject testRoomPrefab2;
    public GameObject testRoomPrefab3;

    public event Action OnStatsChanged;
    public event Action OnRoomChanged;

    /// <summary>
    /// 함선의 초기 상태를 설정합니다.
    /// 기본 스탯 설정 및 시스템 초기화를 수행합니다.
    /// </summary>
    private void Awake()
    {
        InitializeBaseStats();
    }

    /// <summary>
    /// 게임 시작 시 호출되며, 테스트 룸 배치 및 시스템 초기화를 수행합니다.
    /// </summary>
    private void Start()
    {
        Room testRoom1 = Instantiate(testRoomPrefab1).GetComponent<Room>();
        Room testRoom2 = Instantiate(testRoomPrefab2).GetComponent<Room>();
        Room testRoom3 = Instantiate(testRoomPrefab2).GetComponent<Room>();
        InitializeSystems();
        RecalculateAllStats();

        GameManager.Instance.SetPlayerShip(this);
    }

    /// <summary>
    /// 게임 시작 시 호출되며, 테스트 룸 배치 및 시스템 초기화를 수행합니다.
    /// </summary>
    private void Update()
    {
        // 모든 시스템 업데이트
        foreach (ShipSystem system in systems.Values) system.Update(Time.deltaTime);
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
    /// 룸을 해당 위치에 배치하고 관련 정보를 갱신합니다.
    /// </summary>
    /// <param name="room">추가할 룸 객체.</param>
    /// <param name="position">배치 위치 (왼쪽 상단 기준).</param>
    /// <returns>배치 성공 여부.</returns>
    public bool AddRoom(Room room, Vector2Int position)
    {
        // TODO: 테스트용으로 임시로 검사 안함
        //  if (!IsValidPosition(position, room.GetSize()))
        //    return false;

        room.position = position;
        allRooms.Add(room);

        // 방 갱신
        OnRoomChanged?.Invoke(); // 방 바뀔 때마다 알림

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
        OnRoomChanged?.Invoke(); // 방 갱신

        // TODO: MoraleManager에서 사기 계산하기 해야됨

        // Recalculate stats
        RecalculateAllStats();

        return true;
    }

    /// <summary>
    /// 현재 유저가 함선의 구성 요소로 설치한 모든 방을 List로 전달합니다. (allRooms의 data만 반환)
    /// </summary>
    /// <returns></returns>
    public List<RoomData> GetInstalledRoomDataList()
    {
        Debug.Log("설치한 모든 방의 정보를 가져옴");
        // return allRooms.Select(r => r.roomData).Distinct().ToList(); // 중복 제거 (동일한 방 scriptable object면 무조건 1개로 취급)

        return allRooms.Select(r => r.roomData).ToList(); // 그냥 싹 다 넘김 (중복 체크 X)
    }

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
    /// 지정된 위치에 문(Door)을 추가합니다. (미구현)
    /// </summary>
    /// <param name="position">문을 추가할 격자 위치.</param>
    /// <param name="isVertical">세로 방향이면 true, 가로 방향이면 false.</param>
    /// <returns>성공 여부 (현재는 항상 true).</returns>
    public bool AddDoor(Vector2Int position, bool isVertical)
    {
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

    /// <summary>
    /// 함선에 필요한 시스템들을 초기화하고 등록합니다.
    /// 모든 ShipSystem 서브클래스를 수동으로 등록해야 합니다.
    /// </summary>
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

    /// <summary>
    /// ShipSystem을 등록하고 초기화합니다.
    /// 내부 시스템 딕셔너리에 타입 기반으로 저장됩니다.
    /// </summary>
    /// <typeparam name="T">등록할 시스템 타입.</typeparam>
    /// <param name="system">시스템 인스턴스.</param>
    /// <returns>등록된 시스템 객체.</returns>
    private T RegisterSystem<T>(T system) where T : ShipSystem
    {
        system.Initialize(this);
        systems[typeof(T)] = system;
        return system;
    }

    /// <summary>
    /// 등록된 ShipSystem 중 지정된 타입의 시스템을 반환합니다.
    /// </summary>
    /// <typeparam name="T">요청할 시스템 타입.</typeparam>
    /// <returns>시스템 인스턴스. 없으면 null.</returns>
    public T GetSystem<T>() where T : ShipSystem
    {
        if (systems.TryGetValue(typeof(T), out ShipSystem system)) return system as T;
        return null;
    }

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
    /// 특정 스탯 간의 상호작용이나 추가 계산이 필요한 경우에 호출됩니다.
    /// 예: 파생 스탯 계산, 제한 조건 반영 등.
    /// 현재는 비어 있으며 확장용으로 예약되어 있습니다.
    /// </summary>
    private void ProcessSpecialStats()
    {
        // 다른 특수 스탯 상호작용...
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

    // ===== Power Management =====

    /// <summary>
    /// 무기 및 외부 방어 시스템을 포함한 실제 피해 계산 및 적용을 수행합니다.
    /// </summary>
    /// <param name="damage">입력된 피해량.</param>
    /// <param name="weaponType">공격한 무기 타입.</param>
    /// <param name="hitPosition">피격된 좌표.</param>
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

    /// <summary>
    /// 함선에 직접 피해를 적용합니다.
    /// 체력을 감소시키고, 파괴되었는지 확인합니다.
    /// </summary>
    /// <param name="damage">적용할 피해량.</param>
    public void TakeDamage(float damage)
    {
        HitPointSystem hitPointSystem = GetSystem<HitPointSystem>();
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
        List<CrewBase> crewsInRoom = room.crewInRoom;
        foreach (CrewBase crew in crewsInRoom) crew.TakeDamage(damage);
    }


    /// <summary>
    /// 지정된 위치에 있는 방과 그 안의 크루에게 피해를 적용합니다.
    /// 크루는 방보다 70%의 피해만 받습니다.
    /// </summary>
    /// <param name="position">피격된 격자 좌표.</param>
    /// <param name="damage">적용할 원 피해량.</param>
    private void ApplyDamageToRoomAndCrews(Vector2Int position, float damage)
    {
        if (roomGrid.TryGetValue(position, out Room room))
        {
            room.TakeDamage(damage);
            ApplyDamageToCrewsInRoom(room, damage * 0.7f);
        }
    }


    /// <summary>
    /// 중심 좌표를 기준으로 3x3 영역 내의 모든 방과 크루에게 스플래시 피해를 적용합니다.
    /// 크루는 방보다 70%의 피해만 받습니다.
    /// </summary>
    /// <param name="centerPosition">스플래시 중심 좌표.</param>
    /// <param name="damage">스플래시 피해량.</param>
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

    // ===== Crew Management =====


    // ===== Getters =====

    /// <summary>
    /// 현재 탑승 중인 크루 수를 반환합니다.
    /// </summary>
    public int GetCrewCount()
    {
        return GetSystem<CrewSystem>().GetCrewCount();
    }

    /// <summary>
    /// 최대 크루 수(수용 가능 인원)를 반환합니다.
    /// </summary>
    public int GetMaxCrew()
    {
        return (int)currentStats[ShipStat.CrewCapacity];
    }

    /// <summary>
    /// 모든 룸 정보를 반환합니다.
    /// </summary>
    public List<Room> GetAllRooms()
    {
        return allRooms;
    }

    /// <summary>
    /// 함선에 존재하는 모든 문(Door) 객체를 반환합니다.
    /// </summary>
    /// <returns>Door 객체들의 리스트.</returns>
    public List<Door> GetAllDoors()
    {
        return doors;
    }


    /// <summary>
    /// 현재 함선에 탑승 중인 모든 크루를 반환합니다.
    /// </summary>
    /// <returns>CrewBase 객체들의 리스트.</returns>
    public List<CrewBase> GetAllCrew()
    {
        return GetSystem<CrewSystem>().GetCrews();
    }

    /// <summary>
    /// 현재 탑재 중인 모든 무기를 반환합니다.
    /// </summary>
    public List<ShipWeapon> GetAllWeapons()
    {
        return GetSystem<WeaponSystem>().GetWeapons();
    }

    /// <summary>
    /// 현재 함선의 내구도(Hit Point)를 반환합니다.
    /// </summary>
    /// <returns>현재 남아 있는 함선의 체력 값.</returns>
    public float GetCurrentHitPoints()
    {
        return GetSystem<HitPointSystem>().GetHitPoint();
    }

    /// <summary>
    /// 무작위로 타겟팅 가능한 방의 위치를 반환합니다.
    /// 방이 없는 경우 (모두 파괴되었거나 타겟 불가) Vector2Int.zero를 반환합니다.
    /// </summary>
    /// <returns>타겟팅 가능한 방의 격자 좌표.</returns>
    public Vector2Int GetRandomTargetPosition()
    {
        // 실제 구현에서는 함선의 경계 내에서 랜덤 위치 반환
        // 또는 실제 방의 위치 반환
        Room randomRoom = GetRandomTargettableRoom();

        if (randomRoom == null) return Vector2Int.zero;

        return randomRoom.position;
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

        int randomIndex = UnityEngine.Random.Range(0, validRooms.Count);
        return validRooms[randomIndex];
    }

    /// <summary>
    /// 현재 산소량을 반환합니다.
    /// </summary>
    /// <returns>현재 산소량.</returns>
    public float GetOxygenRate()
    {
        return GetSystem<OxygenSystem>().GetOxygenRate();
    }

    /// <summary>
    /// 현재 산소 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 산소 레벨.</returns>
    public OxygenLevel GetOxygenLevel()
    {
        return GetSystem<OxygenSystem>().GetOxygenLevel();
    }
}
