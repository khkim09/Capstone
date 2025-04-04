using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 함선 내의 모든 방의 기본 클래스.
/// 방의 상태, 작동 여부, 산소 레벨, 데미지 처리 등을 관리합니다.
/// </summary>
public abstract class Room : MonoBehaviour, IShipStatContributor
{
    /// <summary>방의 데이터 ScriptableObject 참조.</summary>
    [SerializeField] public RoomData roomData;

    /// <summary>격자상의 방 위치 (좌측 상단 기준).</summary>
    public Vector2Int position;

    /// <summary>현재 방의 업그레이드 레벨.</summary>
    public int currentLevel;

    /// <summary>현재 체력.</summary>
    [SerializeField][HideInInspector] public float currentHitPoints;

    /// <summary>방의 타입.</summary>
    public RoomType roomType;

    /// <summary>이 방이 데미지를 받을 수 있는지 여부.</summary>
    private bool isDamageable;

    /// <summary>인접한 방 리스트.</summary>
    [HideInInspector] protected List<Room> adjacentRooms = new();

    /// <summary>인접한 방 리스트.</summary>
    [HideInInspector] protected List<Door> connectedDoors = new();

    /// <summary>방 작동 시 시각 효과 파티클.</summary>
    [Header("방 효과")][SerializeField] protected ParticleSystem roomParticles;

    /// <summary>방 작동 시 사운드 효과.</summary>
    [SerializeField] protected AudioSource roomSound;

    /// <summary>방의 상태가 변경되었을 때 호출되는 이벤트.</summary>
    public event Action<Room> OnRoomStateChanged;

    /// <summary>현재 방에 존재하는 선원 목록.</summary>
    public List<CrewBase> crewInRoom = new();

    /// <summary>방이 활성화되어 있는지 여부.</summary>
    protected bool isActive = true;

    /// <summary>전력이 공급되고 있는지 여부.</summary>
    protected bool isPowered;

    /// <summary>전력 공급이 요청되었는지 여부.</summary>
    protected bool isPowerRequested;

    /// <summary>방의 시각적 렌더러.</summary>
    protected SpriteRenderer roomRenderer; // 방 렌더러

    /// <summary>해당 방이 실제로 배치되었는지 여부.</summary>
    public bool isPlaced { get; protected set; }

    /// <summary>소속된 Ship 참조.</summary>
    protected Ship parentShip;

    /// <summary>
    /// 방 초기화 및 Ship에 등록하는 Start 메서드.
    /// </summary>
    protected virtual void Start()
    {
        // 부모 Ship 컴포넌트 찾기
        parentShip = GetComponentInParent<Ship>();
        if (parentShip == null)
            // 부모가 없다면 씬에서 찾기 시도
            parentShip = FindAnyObjectByType<Ship>();

        if (parentShip == null) Debug.LogError($"No Ship found for {name}");

        Initialize();
        parentShip.AddRoom(this, new Vector2Int(Random.Range(0, 100), Random.Range(0, 100)));
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
    /// 방이 실제 배치되었을 때 호출됩니다.
    /// </summary>
    public virtual void OnPlaced()
    {
        isPlaced = true;
    }

    /// <summary>
    /// 선원이 방에 진입할 때 호출됩니다.
    /// </summary>
    public virtual void OnCrewEnter(CrewBase crew)
    {
        if (!crewInRoom.Contains(crew))
            crewInRoom.Add(crew);
    }

    /// <summary>
    /// 선원이 방에서 나갈 때 호출됩니다.
    /// </summary>
    public virtual void OnCrewExit(CrewBase crew)
    {
        if (crewInRoom.Contains(crew))
            crewInRoom.Remove(crew);
    }

    /// <summary>
    /// 방의 상태를 매 프레임 업데이트합니다.
    /// </summary>
    protected virtual void UpdateRoom()
    {
    } // 매 프레임/틱마다 방의 상태 업데이트

    /// <summary>
    /// 이 방이 데미지를 받을 수 있는지 여부를 초기화합니다.
    /// </summary>
    public void InitializeIsDamageable()
    {
        // NOTE: 방 종류 추가할 때마다 이 함수에 데미지 여부 등록하기
        switch (roomType)
        {
            case RoomType.Power:
            case RoomType.MedBay:
            case RoomType.Shield:
            case RoomType.Oxygen:
            case RoomType.Engine:
                isDamageable = true;
                break;
            case RoomType.CrewQuarters:
            case RoomType.Storage:
                isDamageable = false;
                break;
            default:
                break;
        }
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


    // getter

    /// <summary>방을 초기화합니다.</summary>
    public virtual void Initialize()
    {
        currentLevel = 1;
        currentHitPoints = roomData.GetRoomData(currentLevel).hitPoint;
        InitializeIsDamageable();
        crewInRoom = new List<CrewBase>();
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
        // return crewInRoom.Count >= roomData.GetRoomData(currentLevel).crewRequirement;
    }

    /// <summary>방이 작동 가능한 상태인지 확인합니다.</summary>
    public bool IsOperational()
    {
        // TODO : 임시로 True 로 설정
        return true;
        // return isActive && isPowered && HasEnoughCrew();
    }

    /// <summary>수리가 필요한 상태인지 확인합니다.</summary>
    public bool NeedsRepair()
    {
        return currentHitPoints < roomData.GetRoomData(currentLevel).hitPoint;
    }

    /// <summary>지정된 피해만큼 체력을 감소시킵니다.</summary>
    public virtual void TakeDamage(float damage)
    {
        currentHitPoints = Mathf.Max(0, currentHitPoints - damage);
        if (currentHitPoints <= 0) OnDisabled();

        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }

    /// <summary>지정된 양만큼 체력을 회복시킵니다.</summary>
    public virtual void Repair(float amount)
    {
        currentHitPoints = Mathf.Min(roomData.GetRoomData(currentLevel).hitPoint, currentHitPoints + amount);

        // 스탯 기여도 변화 알림
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

    // 전력 관련

    /// <summary>전력 공급 상태를 설정합니다.</summary>
    public virtual void SetPowerStatus(bool powered, bool requested)
    {
        isPowered = powered;
        isPowerRequested = requested;
        UpdateEffects();
        NotifyStateChanged();
    }

    /// <summary>체력 퍼센티지를 반환합니다.</summary>
    public float GetHealthPercentage()
    {
        if (roomData.GetRoomData(currentLevel).hitPoint == 0) return 0;
        return currentHitPoints / roomData.GetRoomData(currentLevel).hitPoint * 100f;
    }

    // ===== 스탯 시스템 관련 코드 =====

    /// <summary>방이 함선 스탯에 기여하는 값을 반환합니다.</summary>
    public virtual Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 빈 구현 (모든 방이 기본적으로 기여하는 스탯이 없음)
        Dictionary<ShipStat, float> contributions = new();

        // 방 상태가 작동 불능이면 아무런 기여도 없음
        if (!IsOperational())
            return contributions;

        contributions[ShipStat.HitPointsMax] = roomData.GetRoomData(currentLevel).hitPoint;

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
        OnRoomStateChanged?.Invoke(this);
    }

    /// <summary>현재 업그레이드 레벨을 반환합니다.</summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    ///최대 업그레이드 레벨을 반환합니다.
    // public int GetMaxLevel()
    // {
    //     return maxLevel;
    // }

    /// <summary>현재 전력 요구량을 반환합니다.</summary>
    public float GetPowerConsumption()
    {
        return roomData.GetRoomData(currentLevel).powerRequirement;
    }

    /// <summary>방의 크기를 반환합니다.</summary>
    public Vector2Int GetSize()
    {
        return roomData.GetRoomData(currentLevel).size;
    }

    /// <summary>최대 체력을 반환합니다.</summary>
    public float GetMaxHitPoints()
    {
        return roomData.GetRoomData(currentLevel).hitPoint;
    }

    /// <summary>방의 그리드 크기 (에디터 전용).</summary>
    public Vector2Int gridSize = new(2, 2);
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
        get => (TData)base.roomData;
        set => base.roomData = value;
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
        // 데이터가 없거나 레벨이 변경되었을 때 업데이트
        if (currentRoomLevelData == null ||
            (currentRoomLevelData != null && currentRoomLevelData.level != currentLevel))
            UpdateRoomLevelData();

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
        {
            Debug.LogError($"roomData is null in {GetType().Name}.Start()");
        }
        else
        {
            Initialize();
            // 레벨 데이터 초기화
            UpdateRoomLevelData();
        }
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
    public override void Initialize()
    {
        // roomData가 null인지 확인
        if (roomData == null)
        {
            Debug.LogError($"roomData is null in {GetType().Name}.Initialize()");
            crewInRoom = new List<CrewBase>();
            return;
        }

        UpdateRoomVisual();


        base.Initialize();
        UpdateRoomLevelData();
    }

    /// <summary>
    /// 현재 레벨의 시각적 요소 (스프라이트 등)를 갱신합니다.
    /// </summary>
    private void UpdateRoomVisual()
    {
        roomRenderer = GetComponent<SpriteRenderer>();
        if (roomRenderer != null && GetCurrentLevelData()?.roomSprite != null)
            roomRenderer.sprite = GetCurrentLevelData().roomSprite;
    }
}
