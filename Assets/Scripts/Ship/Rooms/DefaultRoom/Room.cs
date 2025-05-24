using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Drawing.Inspector.PropertyDrawers;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 함선 내의 모든 방의 기본 클래스.
/// 방의 상태, 작동 여부, 산소 레벨, 데미지 처리 등을 관리합니다.
/// </summary>
public abstract class Room : MonoBehaviour, IShipStatContributor
{
    /// <summary>방의 데이터 ScriptableObject 참조.</summary>
    [SerializeField] protected RoomData roomData;

    /// <summary>격자상의 방 위치 (좌측 상단 기준).</summary>
    public Vector2Int position;

    /// <summary>현재 방의 업그레이드 레벨.</summary>
    protected int currentLevel;

    /// <summary>현재 체력.</summary>
    [SerializeField] public float currentHitPoints;

    /// <summary>방의 타입.</summary>
    public RoomType roomType;

    /// <summary>이 방이 데미지를 받을 수 있는지 여부.</summary>
    private bool isDamageable;

    /// <summary>인접한 방 리스트.</summary>
    [HideInInspector] protected List<Room> adjacentRooms = new();

    /// <summary>이 방에 연결된 문들</summary>
    [HideInInspector] protected List<Door> connectedDoors = new();

    /// <summary>방의 현재 회전 값</summary>
    [SerializeField] public Constants.Rotations.Rotation currentRotation;

    /// <summary>방 작동 시 시각 효과 파티클.</summary>
    [Header("방 효과")][SerializeField] protected ParticleSystem roomParticles;

    /// <summary>방 작동 시 사운드 효과.</summary>
    [SerializeField] protected AudioSource roomSound;

    /// <summary>방의 상태가 변경되었을 때 호출되는 이벤트.</summary>
    public event Action<Room> OnRoomStateChanged;

    /// <summary>현재 방에 존재하는 선원 목록.</summary>
    public List<CrewMember> crewInRoom = new();

    /// <summary>방이 활성화되어 있는지 여부.</summary>
    public bool isActive = true;

    /// <summary>전력이 공급되고 있는지 여부.</summary>
    protected bool isPowered = true;

    /// <summary>전력 공급이 요청되었는지 여부.</summary>
    protected bool isPowerRequested = false;

    /// <summary>방의 시각적 렌더러.</summary>
    protected SpriteRenderer roomRenderer; // 방 렌더러

    /// <summary>소속된 Ship 참조.</summary>
    public Ship parentShip;

    [SerializeField] private SpriteRenderer icon;

    /// <summary>
    /// 각 방에 collider 추가, isTrigger = true 설정을 통해 선원 충돌 방해 제거
    /// </summary>
    protected virtual void Start()
    {
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();

            RoomData.RoomLevel levelData = roomData.GetRoomDataByLevel(currentLevel);
            collider.size = new Vector2(levelData.size.x, levelData.size.y);
            collider.isTrigger = true;
        }
    }

    /// <summary>
    /// 매 프레임마다 방의 상태를 업데이트합니다.
    /// </summary>
    public virtual void Update()
    {
        UpdateRoom();
    }

    /// <summary>
    /// 방이 배치 가능한 위치인지 검사합니다.
    /// </summary>
    /// <param name="newPosition">새 위치.</param>
    /// <param name="ship">함선 참조.</param>
    public virtual bool CanBePlaced(Vector2Int newPosition, Ship ship)
    {
        // 기본 배치 규칙 검사
        return true;
    }

    /// <summary>
    /// 선원이 방에 진입할 때 호출됩니다.
    /// </summary>
    public virtual void OnCrewEnter(CrewMember crew)
    {
        if (!crewInRoom.Contains(crew))
            crewInRoom.Add(crew);
    }

    /// <summary>
    /// 선원이 방에서 나갈 때 호출됩니다.
    /// </summary>
    public virtual void OnCrewExit(CrewMember crew)
    {
        if (crewInRoom.Contains(crew))
            crewInRoom.Remove(crew);
    }

    /// <summary>
    /// 방의 우선순위 타일에 따른 그리드에서의 위치 반환 (회전각 적용)
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetRotatedCrewEntryGridPriority()
    {
        RoomData.RoomLevel levelData = roomData.GetRoomDataByLevel(currentLevel);

        // 실제 방의 우선순위 순 타일 위치
        List<Vector2Int> result = new();

        // 회전각 별 타일 우선순위 적용
        switch (currentRotation)
        {
            case Constants.Rotations.Rotation.Rotation0:
                foreach (Vector2Int tile in levelData.crewEntryGridPriority)
                    result.Add(position + tile);
                break;
            case Constants.Rotations.Rotation.Rotation90:
                foreach (Vector2Int tile in levelData.crewEntryGridPriority)
                    result.Add(position + new Vector2Int(tile.y, -tile.x));
                break;
            case Constants.Rotations.Rotation.Rotation180:
                foreach (Vector2Int tile in levelData.crewEntryGridPriority)
                    result.Add(position + new Vector2Int(-tile.x, -tile.y));
                break;
            case Constants.Rotations.Rotation.Rotation270:
                foreach (Vector2Int tile in levelData.crewEntryGridPriority)
                    result.Add(position + new Vector2Int(-tile.y, tile.x));
                break;
            default:
                break;
        }

        return result;
    }

    /// <summary>
    /// 방의 상태를 매 프레임 업데이트합니다.
    /// </summary>
    protected virtual void UpdateRoom()
    {
        // 매 프레임/틱마다 방의 상태 업데이트
    }

    /// <summary>
    /// 다른 방의 속성을 복사합니다.
    /// </summary>
    /// <param name="other">복사할 소스 방</param>
    public virtual void CopyFrom(Room other)
    {
        // 기본 속성 복사
        roomData = other.GetRoomData();
        position = other.position;
        currentLevel = other.GetCurrentLevel();
        currentHitPoints = other.currentHitPoints;
        roomType = other.roomType;
        currentRotation = other.currentRotation;
        isActive = other.isActive;
        isPowered = other.GetIsPowered();
        isPowerRequested = other.GetIsPowerRequested();
    }

    /// <summary>
    /// 이 방이 데미지를 받을 수 있는지 여부를 초기화합니다.
    /// </summary>
    public void InitializeIsDamageable()
    {
        // TODO: 방 종류 추가할 때마다 이 함수에 데미지 여부 등록하기
        switch (roomType)
        {
            case RoomType.Ammunition:
            case RoomType.Cockpit:
            case RoomType.CrewQuarters:
            case RoomType.Engine:
            case RoomType.MedBay:
            case RoomType.Oxygen:
            case RoomType.Power:
            case RoomType.Shield:
            case RoomType.WeaponControl:
                isDamageable = true;
                break;
            case RoomType.Teleporter:
            case RoomType.Corridor:
            case RoomType.LifeSupport:
            case RoomType.Storage:
                isDamageable = false;
                break;
            default:
                break;
        }
    }

    public RoomData GetRoomData()
    {
        return roomData;
    }

    public RoomType GetRoomType()
    {
        return roomType;
    }

    public void SetRoomData(RoomData roomData)
    {
        this.roomData = roomData;
    }

    /// <summary>이 방이 전력 공급 중인지 확인합니다.</summary>
    public bool GetIsPowered()
    {
        return isPowered;
    }

    /// <summary>전력 공급 요청 여부를 확인합니다.</summary>
    public bool GetIsPowerRequested()
    {
        return isPowerRequested;
    }

    /// <summary>이 방이 데미지를 받을 수 있는지 확인합니다.</summary>
    public bool GetIsDamageable()
    {
        return isDamageable;
    }

    /// <summary>방의 이펙트(색상, 파티클 등)를 업데이트합니다.</summary>
    protected virtual void UpdateEffects()
    {
        if (roomRenderer == null) return;

        if (!IsOperational())
        {
            // 비작동 효과
            if (roomParticles != null && roomParticles.isPlaying)
                roomParticles.Stop();

            if (roomSound != null && roomSound.isPlaying)
                roomSound.Stop();
        }
        else
        {
            // 작동 효과
            if (roomParticles != null && !roomParticles.isPlaying)
                roomParticles.Play();

            if (roomSound != null && !roomSound.isPlaying)
                roomSound.Play();

            // 파티클 효과 조정
            if (roomParticles != null)
            {
                ParticleSystem.EmissionModule emission = roomParticles.emission;
                emission.rateOverTime = 10f * currentLevel;
            }
        }
    }


    /// <summary>방을 초기화합니다.</summary>
    public virtual void Initialize(int level)
    {
        currentHitPoints = roomData.GetRoomDataByLevel(level).hitPoint;
        InitializeIsDamageable();
        crewInRoom = new List<CrewMember>();
        currentLevel = level;
        roomRenderer = gameObject.AddComponent<SpriteRenderer>();
        gridSize = roomData.GetRoomDataByLevel(level).size;
        roomType = GetRoomData().GetRoomType();
        roomRenderer.sortingOrder = Constants.SortingOrders.Room;

        //requested initialize 필요
        if (GetPowerConsumption() > 0)
            isPowerRequested = true;

        // 부모 Ship 컴포넌트 찾기
        parentShip = GetComponentInParent<Ship>();
        if (parentShip == null)
            // 부모가 없다면 씬에서 찾기 시도
            parentShip = FindAnyObjectByType<Ship>();

        if (parentShip == null) Debug.LogError($"No Ship found for {name}");
    }

    /// <summary>현재 방에 있는 선원 수를 반환합니다.</summary>
    public int GetCrewCount()
    {
        return crewInRoom.Count;
    }

    /// <summary>필요한 선원이 충분한지 확인합니다.</summary>
    public bool HasEnoughCrew()
    {
        // TODO : 테스트 용으로 일단 true, 나중엔 실제로 선원 세야함
        return true;
        // return crewInRoom.Count >= roomData.GetRoomDataByLevel(currentLevel).crewRequirement;
    }

    /// <summary>방이 작동 가능한 상태인지 확인합니다.</summary>
    public bool IsOperational()
    {
        if (damageCondition == DamageLevel.breakdown)
            return false;
        if (isActive)
        {
            if (isPowerRequested && !isPowered)
                return false;
            return true;
        }
        return false;
    }


    // 전력 관련

    /// <summary>전력 공급 상태를 설정합니다.</summary>
    public virtual void SetPowerStatus(bool powered, bool requested)
    {
        isPowered = powered;
        isPowerRequested = requested;
        UpdateEffects();
        NotifyStateChanged();
    }

    public void BlackOut()
    {
        if (isPowered)
        {
            isPowered = false;
            UpdateEffects();
            UpdateRoomVisual();
        }
    }

    /// <summary>체력 퍼센티지를 반환합니다.</summary>
    public float GetHealthPercentage()
    {
        if (roomData.GetRoomDataByLevel(currentLevel).hitPoint == 0) return 0;
        return currentHitPoints / roomData.GetRoomDataByLevel(currentLevel).hitPoint * 100f;
    }

    // ===== 스탯 시스템 관련 코드 =====

    /// <summary>방이 함선 스탯에 기여하는 값을 반환합니다.</summary>
    public virtual Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 빈 구현 (모든 방이 기본적으로 기여하는 스탯이 없음)
        Dictionary<ShipStat, float> contributions = new();

        contributions[ShipStat.HitPointsMax] = roomData.GetRoomDataByLevel(currentLevel).hitPoint;

        // 방 상태가 작동 불능이면 아무런 기여도 없음
        if (!IsOperational())
            return contributions;



        // 방의 건강 상태에 따라 기여도에 효율 적용
        // 파생 클래스는 이 베이스 메서드를 호출하고 반환된 Dictionary에 값을 추가/조정해야 함

        return contributions;
    }

    /// <summary>다음 레벨에서의 스탯 기여도를 반환합니다.</summary>
    public virtual Dictionary<ShipStat, float> GetNextLevelStatContributions()
    {
        // // 이미 최대 레벨이면 현재 기여도 반환
        // if (currentLevel >= maxLevel)
        //     return GetStatContributions();

        // 임시로 업그레이드 레벨 증가
        int originalLevel = currentLevel;
        currentLevel++;

        // 새 레벨에서의 기여도 가져오기
        Dictionary<ShipStat, float> nextLevelContributions = GetStatContributions();

        // 업그레이드 레벨 복원
        currentLevel = originalLevel;

        return nextLevelContributions;
    }

    /// <summary>방을 업그레이드합니다.</summary>
    public virtual bool Upgrade()
    {
        // 이미 최대 레벨이면 실패
        // if (currentLevel >= maxLevel)
        //    return false;

        // 비용 처리 로직은 여기에 (필요시)

        // 업그레이드 실행
        currentLevel++;

        UpdateRoom();

        // 스탯 기여도 변화 알림
        NotifyStateChanged();

        return true;
    }

    /// <summary>상태 변경을 알립니다.</summary>
    protected virtual void NotifyStateChanged()
    {
        if (workingCrew != null)
        {
            workingCrew.WalkOut();
            workingCrew.TryWork();
        }
        OnRoomStateChanged?.Invoke(this);
    }

    /// <summary>현재 업그레이드 레벨을 반환합니다.</summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
    }

    ///최대 업그레이드 레벨을 반환합니다.
    // public int GetMaxLevel()
    // {
    //     return maxLevel;
    // }

    /// <summary>현재 전력 요구량을 반환합니다.</summary>
    public float GetPowerConsumption()
    {
        return roomData.GetRoomDataByLevel(currentLevel).powerRequirement;
    }

    /// <summary>방의 크기를 반환합니다.</summary>
    public Vector2Int GetSize()
    {
        return roomData.GetRoomDataByLevel(currentLevel).size;
    }

    /// <summary>최대 체력을 반환합니다.</summary>
    public float GetMaxHitPoints()
    {
        return roomData.GetRoomDataByLevel(currentLevel).hitPoint;
    }

    /// <summary>방의 그리드 크기 (에디터 전용).</summary>
    private Vector2Int gridSize = new(2, 2);


    /// <summary>
    /// 방 회전에 따라 문들의 위치와 방향 업데이트
    /// </summary>
    private void UpdateDoorsPositionAndRotation()
    {
        // 기존 가능한 문 위치 정보와 현재 부착된 문들을 비교하여 업데이트
        foreach (Door door in connectedDoors) door.RotateDirectionClockwise();
    }

    // TODO : 나중엔 이런 월드 좌표가 아니라, 싱글턴 그리드 좌표에 맞는 월드 좌표로 변환하는 함수가 필요하다.

    /// <summary>
    /// 회전각을 고려하여 방의 점유 타일 반환
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetOccupiedTiles()
    {
        RoomData.RoomLevel levelData = roomData.GetRoomDataByLevel(GetCurrentLevel());
        Vector2Int roomSize = RoomRotationUtility.GetRotatedSize(levelData.size, currentRotation);

        return RoomRotationUtility.GetOccupiedGridPositions(position, roomSize, currentRotation);
    }

    /// <summary>
    /// 인자 타일이 방의 일부인지 여부 반환
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool OccupiesTile(Vector2Int tile)
    {
        return GetOccupiedTiles().Contains(tile);
    }

    public List<Door> GetConnectedDoors()
    {
        return connectedDoors;
    }

    public List<CrewMember> GetTotalCrewsInRoom()
    {
        List<CrewMember> total = new();

        foreach (CrewMember crew in parentShip.allCrews)
            if (crew.currentRoom == this)
                total.Add(crew);

        foreach (CrewMember enemy in parentShip.allEnemies)
            if (enemy.currentRoom == this)
                total.Add(enemy);

        return total;
    }

    /// <summary>
    /// 현재 레벨의 시각적 요소 (스프라이트 등)를 갱신합니다.
    /// </summary>
    public void UpdateRoomVisual()
    {
        // TODO : 아이콘 체력상황, 타입에 맞게 업데이트

        string color = "";

        if (damageCondition == DamageLevel.breakdown)
            color = "red";
        else
        {
            if (!IsOperational())
                color = "gray";
            else
            {
                if(damageCondition==DamageLevel.scratch)
                    color = "yellow";
                else if(damageCondition==DamageLevel.good)
                {
                    color = "green";
                }
            }
        }

        if (color.Equals(""))
        {
            Debug.LogError(name+": Room has no color assigned.");
            return;
        }


        if (icon == null)
            icon = Instantiate(roomData.GetRoomDataByLevel(currentLevel).roomIconPrefab, transform, false).GetComponent<SpriteRenderer>();

        if (position == new Vector2Int(0, 0)) return;

        List<Vector2Int> tiles = GetOccupiedTiles();

        Vector2 sum = Vector2.zero;

        foreach (Vector2Int tile in tiles) sum += (Vector2)tile; // Vector2Int → Vector2 변환

        Vector2 center = sum / tiles.Count;

        icon.transform.position = parentShip.GetWorldPositionFromGrid(center);
        icon.transform.rotation = Quaternion.identity;

        icon.GetComponent<SpriteRenderer>().sprite =
            Resources.Load<Sprite>($"Sprites/UI/Room Icons/{color}/{roomType.ToString().ToLower()}_{color}");
        icon.GetComponent<SpriteRenderer>().sortingOrder = Constants.SortingOrders.RoomIcon;
        icon.GetComponent<BoxCollider2D>().size = icon.GetComponent<Renderer>().bounds.size;
    }

    #region 수리

    /// <summary>
    /// 데미지 입은 정도
    /// </summary>
    public DamageLevel damageCondition = DamageLevel.good;

    ///<summary>
    /// 수리가 필요한 상태인지 확인합니다.
    /// </summary>
    public bool NeedsRepair()
    {
        if (damageCondition == DamageLevel.breakdown)
            return currentHitPoints < roomData.GetRoomDataByLevel(currentLevel)
                .damageHitPointRate[RoomDamageLevel.DamageLevelTwo];
        else
            return currentHitPoints < roomData.GetRoomDataByLevel(currentLevel).hitPoint;
    }

    #endregion

    /// <summary>지정된 피해만큼 체력을 감소시킵니다.</summary>
    public virtual void TakeDamage(float damage)
    {
        //피해발생 이후에 현재 체력에 따라 시설의 피해 단계를 변화시킨다.
        if (isDamageable)
        {
            currentHitPoints = Mathf.Max(0, currentHitPoints - damage);
            if (currentHitPoints <=
                roomData.GetRoomDataByLevel(currentLevel).damageHitPointRate[RoomDamageLevel.DamageLevelTwo])
            {
                damageCondition = DamageLevel.breakdown;
            }
            //  OnDisabled();
            else if (currentHitPoints <=
                     roomData.GetRoomDataByLevel(currentLevel).damageHitPointRate[RoomDamageLevel.DamageLevelOne])
                damageCondition = DamageLevel.scratch;
        }

        // 스탯 기여도 변화 알림
        NotifyStateChanged();
        UpdateRoomVisual();
    }

    public virtual void DownDamageCondition()
    {
        float damage = 0;
        if (damageCondition == DamageLevel.good)
            damage = currentHitPoints - roomData.GetRoomDataByLevel(currentLevel)
                .damageHitPointRate[RoomDamageLevel.DamageLevelOne];
        else if (damageCondition == DamageLevel.scratch)
            damage = currentHitPoints - roomData.GetRoomDataByLevel(currentLevel)
                .damageHitPointRate[RoomDamageLevel.DamageLevelTwo];
        TakeDamage(damage);
    }

    /// <summary>지정된 양만큼 체력을 회복시킵니다.</summary>
    public void Repair(float amount)
    {
        if (damageCondition == DamageLevel.breakdown)
        {
            currentHitPoints =
                Mathf.Min(roomData.GetRoomDataByLevel(currentLevel).damageHitPointRate[RoomDamageLevel.DamageLevelTwo],
                    currentHitPoints + amount);
        }
        else
        {
            currentHitPoints = Mathf.Min(roomData.GetRoomDataByLevel(currentLevel).hitPoint, currentHitPoints + amount);
            if (currentHitPoints > roomData.GetRoomDataByLevel(currentLevel)
                    .damageHitPointRate[RoomDamageLevel.DamageLevelOne])
            {
                damageCondition = DamageLevel.good;
                UpdateRoomVisual();
            }
        }

        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }

    /// <summary>
    /// 100% 수리
    /// </summary>
    /// <param name="amount"></param>
    public void FullRepair(float amount)
    {
        currentHitPoints = amount;
        damageCondition = DamageLevel.good;

        NotifyStateChanged();
    }


    /// <summary>방이 작동 불능 상태가 되었을 때 호출됩니다.</summary>
    protected virtual void OnDisabled()
    {
        isActive = false;
        // 방별 비활성화 처리

        // TODO: MoraleManager 에서 사기 계산하게 해야됨.


        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }

    #region 작업

    /// <summary>
    /// 작업 방향
    /// </summary>
    public Vector2Int workDirection;

    [SerializeField] private CrewMember _workingCrew;

    public CrewMember workingCrew
    {
        get => _workingCrew;
        set
        {
            _workingCrew = value;
            if (parentShip != null)
                parentShip.RecalculateAllStats();
        }
    }

    public virtual bool CanITouch(CrewMember crew)
    {
        return false;
    }

    #endregion

    public void SetIsActive(bool activate)
    {
        isActive = activate;
        NotifyStateChanged();
        UpdateRoomVisual();
    }
}

/// <summary>
/// 함선 내의 특화된 방 타입을 위한 제네릭 기본 클래스.
/// RoomData 및 RoomLevel 타입에 따라 세부 동작과 데이터를 확장할 수 있도록 설계되었습니다.
/// </summary>
/// <typeparam name="TData">특정 방 타입에 대한 RoomData 타입.</typeparam>
/// <typeparam name="TLevel">RoomData에 포함된 레벨별 데이터 타입.</typeparam>
public abstract class Room<TData, TLevel> : Room
    where TData : RoomData
    where TLevel : RoomData.RoomLevel
{
    /// <summary>
    /// 방의 RoomData를 명확한 제네릭 타입으로 재정의한 프로퍼티.
    /// </summary>
    public new TData roomData
    {
        get => (TData)GetRoomData();
        set => SetRoomData(value);
    }

    /// <summary>
    /// 현재 방 레벨에 해당하는 데이터 캐시.
    /// </summary>
    protected TLevel currentRoomLevelData;

    /// <summary>
    /// 현재 방의 레벨에 해당하는 TLevel 데이터를 반환합니다.
    /// 내부적으로 필요 시 데이터를 갱신합니다.
    /// </summary>
    /// <returns>현재 레벨의 방 데이터.</returns>
    public TLevel GetCurrentLevelData()
    {
        return currentRoomLevelData;
    }

    /// <summary>
    /// 레벨 데이터 캐시를 갱신합니다.
    /// roomData에서 현재 레벨에 맞는 데이터를 가져오고, 시각적 요소도 함께 갱신됩니다.
    /// </summary>
    protected virtual void UpdateRoomLevelData()
    {
        if (roomData != null)
        {
            currentRoomLevelData = (roomData as RoomData<TLevel>).GetTypedRoomData(currentLevel);
            if (currentRoomLevelData == null)
                Debug.LogWarning($"No {GetType().Name} data found for level {currentLevel}");

            UpdateRoomVisual();
        }
    }

    /// <summary>
    /// Start 시 roomData가 유효하다면 초기화 및 레벨 데이터 갱신을 수행합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // roomData가 null이면 초기화 건너뛰기
        if (roomData == null)
            Debug.LogError($"roomData is null in {GetType().Name}.Start()");
        else
            // 레벨 데이터 초기화
            UpdateRoomLevelData();
    }

    /// <summary>
    /// 방을 업그레이드한 후 레벨 데이터도 갱신합니다.
    /// </summary>
    /// <returns>업그레이드 성공 여부.</returns>
    public override bool Upgrade()
    {
        bool result = base.Upgrade();
        if (result) UpdateRoomLevelData();
        return result;
    }

    /// <summary>
    /// 방 초기화 시 기본 초기화 이후 레벨 데이터도 갱신합니다.
    /// </summary>
    public override void Initialize(int level)
    {
        base.Initialize(level);

        if (roomData == null)
        {
            Debug.LogError($"roomData is null in {GetType().Name}.Initialize()");
            crewInRoom = new List<CrewMember>();
            return;
        }


        InitializeIsDamageable();
        //   InitializeDoor();

        UpdateRoomLevelData();
    }

    /// <summary>
    /// 현재 레벨의 시각적 요소 (스프라이트 등)를 갱신합니다.
    /// </summary>
    public void UpdateRoomVisual()
    {
        base.UpdateRoomVisual();

        TLevel levelData = GetCurrentLevelData();
        if (roomRenderer != null && levelData?.roomSprite != null) roomRenderer.sprite = levelData.roomSprite;
    }

    private void InitializeDoor()
    {
        // 현재 레벨에 해당하는 방 데이터 가져오기
        RoomData.RoomLevel roomLevel = roomData.GetRoomDataByLevel(currentLevel);
        if (roomLevel == null || roomLevel.possibleDoorPositions == null || roomLevel.possibleDoorPositions.Count == 0)
        {
            Debug.LogWarning($"No door positions defined for room {name} at level {currentLevel}");
            return;
        }

        // Ship에서 문 데이터 가져오기
        DoorData shipDoorData = parentShip.GetDoorData();
        int doorLevel = parentShip.GetDoorLevel();

        if (shipDoorData == null)
        {
            Debug.LogError("Ship door data not found. Make sure it's assigned in Ship component.");
            return;
        }
    }


}

public enum DamageLevel
{
    good = 0,
    scratch = 1,
    breakdown = 2
}
