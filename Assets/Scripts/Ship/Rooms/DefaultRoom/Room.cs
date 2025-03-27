using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 함선 내의 모든 방의 기본 클래스
/// </summary>
public abstract class Room : MonoBehaviour, IShipStatContributor
{
    [SerializeField] protected RoomData roomData; // 기본 타입으로 참조

    [HideInInspector] public Vector2Int position;

    public int currentLevel;

    [HideInInspector] public int maxLevel;

    [SerializeField][HideInInspector] protected float currentHitPoints; // 현재 체력

    [SerializeField][HideInInspector] protected OxygenLevel oxygenLevel = OxygenLevel.Normal; // 현재 산소 레벨

    public RoomType roomType;

    private bool isDamageable;

    [HideInInspector] protected List<Room> adjacentRooms = new(); // 인접한 방들

    [HideInInspector] protected List<Door> connectedDoors = new(); // 연결된 문들

    [Header("방 효과")][SerializeField] protected ParticleSystem roomParticles;
    [SerializeField] protected AudioSource roomSound;

    public event Action<Room> OnRoomStateChanged;

    protected Dictionary<OxygenLevel, float> crewDamagePerOxygen = new()
    {
        { OxygenLevel.None, 10f },
        { OxygenLevel.Critical, 5f },
        { OxygenLevel.Low, 0f },
        { OxygenLevel.Medium, 0f },
        { OxygenLevel.Normal, 0f }
    };

    public List<CrewBase> crewInRoom = new List<CrewBase>();

    protected Dictionary<OxygenLevel, float> fireExtinguishRatePerLevel = new()
    {
        { OxygenLevel.None, 2.0f },
        { OxygenLevel.Critical, 1.5f },
        { OxygenLevel.Low, 1.0f },
        { OxygenLevel.Medium, 1.0f },
        { OxygenLevel.Normal, 1.0f }
    };

    protected bool isActive = true; // 활성화 상태

    protected bool isPowered; // 전력 공급 상태

    protected bool isPowerRequested; // 유저가 파워를 요청했는지 여부

    protected Color lowOxygenColor = new(1f, 0.8f, 0.8f); // 산소 부족시 색상

    protected Color normalColor = Color.white; // 기본 방 색상

    protected SpriteRenderer roomRenderer; // 방 렌더러

    public bool isPlaced { get; protected set; }

    // 직접 Ship 참조 추가
    protected Ship parentShip;

    // 시작 시 초기화 (ShipManager 대신 Ship에 등록)
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

    public virtual void Update()
    {
        UpdateRoom();
    }

    public virtual bool CanBePlaced(Vector2Int newPosition, Ship ship)
    {
        // 기본 배치 규칙 검사
        return true;
    }

    public virtual void OnPlaced()
    {
        isPlaced = true;
    }

    public virtual void OnCrewEnter(CrewBase crew)
    {
        if (!crewInRoom.Contains(crew))
            crewInRoom.Add(crew);
    }

    public virtual void OnCrewExit(CrewBase crew)
    {
        if (crewInRoom.Contains(crew))
            crewInRoom.Remove(crew);
    }

    protected virtual void UpdateRoom()
    {
    } // 매 프레임/틱마다 방의 상태 업데이트

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

    public bool GetIsPowered()
    {
        return isPowered;
    }

    public bool GetIsPowerRequested()
    {
        return isPowerRequested;
    }

    public bool GetIsDamageable()
    {
        return isDamageable;
    }

    protected virtual void UpdateEffects()
    {
        if (roomRenderer == null) return;

        // 산소 레벨에 따른 방 색상 조정
        Color targetColor = normalColor;

        switch (oxygenLevel)
        {
            case OxygenLevel.Low:
                targetColor = Color.Lerp(normalColor, lowOxygenColor, 0.3f);
                break;
            case OxygenLevel.Critical:
                targetColor = Color.Lerp(normalColor, lowOxygenColor, 0.6f);
                break;
            case OxygenLevel.None:
                targetColor = Color.Lerp(normalColor, lowOxygenColor, 1f);
                break;
        }

        roomRenderer.color = targetColor;

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

    protected virtual bool HasFire()
    {
        // 화재 존재 여부 확인 로직
        return false;
    }

    protected virtual void UpdateFireExtinguishRate(float rate)
    {
        // 화재 진압 속도 조정 로직
    }

    // getter
    public OxygenLevel GetOxygenLevel()
    {
        return oxygenLevel;
    }

    public virtual void Initialize()
    {
        currentLevel = 1;
        currentHitPoints = roomData.GetRoomData(currentLevel).hitPoint;
        InitializeIsDamageable();
        crewInRoom = new List<CrewBase>();
    }

    // 선원 관련 함수들
    public int GetCrewCount()
    {
        return crewInRoom.Count;
    }

    public bool HasEnoughCrew()
    {
        // TODO : 테스트 용으로 일단 true, 나중엔 실제로 선원 세야함
        return true;
        // return crewInRoom.Count >= roomData.GetRoomData(currentLevel).crewRequirement;
    }

    // 상태 체크 함수들
    public bool IsOperational()
    {
        // TODO : 임시로 True 로 설정
        return true;
        // return isActive && isPowered && HasEnoughCrew();
    }

    public bool NeedsRepair()
    {
        return currentHitPoints < roomData.GetRoomData(currentLevel).hitPoint;
    }

    public virtual void SetOxygenLevel(OxygenLevel level)
    {
        oxygenLevel = level;
        UpdateEffects();

        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }


    // 데미지 처리
    public virtual void TakeDamage(float damage)
    {
        currentHitPoints = Mathf.Max(0, currentHitPoints - damage);
        if (currentHitPoints <= 0) OnDisabled();

        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }

    // 수리 처리
    public virtual void Repair(float amount)
    {
        currentHitPoints = Mathf.Min(roomData.GetRoomData(currentLevel).hitPoint, currentHitPoints + amount);

        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }

    // 방 비활성화시 호출
    protected virtual void OnDisabled()
    {
        isActive = false;
        // 방별 비활성화 처리

        // TODO: MoraleManager 에서 사기 계산하게 해야됨.


        // 스탯 기여도 변화 알림
        NotifyStateChanged();
    }

    // 전력 관련

    // Room.cs에 추가할 메서드
    public virtual void SetPowerStatus(bool powered, bool requested)
    {
        isPowered = powered;
        isPowerRequested = requested;
        UpdateEffects();
        NotifyStateChanged();
    }


    public float GetHealthPercentage()
    {
        return currentHitPoints / roomData.GetRoomData(currentLevel).hitPoint * 100f;
    }

    // ===== 스탯 시스템 관련 코드 =====

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 반환
    /// 각 파생 클래스에서 구현해야 함
    /// </summary>
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

    /// <summary>
    /// 다음 업그레이드 레벨에서의 스탯 기여도 미리보기 반환
    /// </summary>
    public virtual Dictionary<ShipStat, float> GetNextLevelStatContributions()
    {
        // 이미 최대 레벨이면 현재 기여도 반환
        if (currentLevel >= maxLevel)
            return GetStatContributions();

        // 임시로 업그레이드 레벨 증가
        int originalLevel = currentLevel;
        currentLevel++;

        // 새 레벨에서의 기여도 가져오기
        Dictionary<ShipStat, float> nextLevelContributions = GetStatContributions();

        // 업그레이드 레벨 복원
        currentLevel = originalLevel;

        return nextLevelContributions;
    }

    /// <summary>
    /// 방 업그레이드 - 스탯 시스템 관련 코드 추가
    /// </summary>
    public virtual bool Upgrade()
    {
        // 이미 최대 레벨이면 실패
        if (currentLevel >= maxLevel)
            return false;

        // 비용 처리 로직은 여기에 (필요시)

        // 업그레이드 실행
        currentLevel++;

        UpdateRoom();

        // 스탯 기여도 변화 알림
        NotifyStateChanged();

        return true;
    }

    /// <summary>
    /// 상태 변경을 Ship에 알림
    /// </summary>
    protected virtual void NotifyStateChanged()
    {
        OnRoomStateChanged?.Invoke(this);
    }

    /// <summary>
    /// 현재 업그레이드 레벨 반환
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// 최대 업그레이드 레벨 반환
    /// </summary>
    public int GetMaxLevel()
    {
        return maxLevel;
    }

    public float GetPowerConsumption()
    {
        return roomData.GetRoomData(currentLevel).powerRequirement;
    }

    public Vector2Int GetSize()
    {
        return roomData.GetRoomData(currentLevel).size;
    }

    public float GetMaxHitPoints()
    {
        return roomData.GetRoomData(currentLevel).hitPoint;
    }




    // 전투 관련
    public Vector2Int gridSize = new Vector2Int(2, 2);
}

// 제네릭 버전의 Room 클래스
/// <summary>
/// 함선 내의 특화된 방 타입을 위한 제네릭 기본 클래스
/// </summary>
public abstract class Room<TData, TLevel> : Room
    where TData : RoomData
    where TLevel : RoomData.RoomLevel
{
    // 명확한 타입으로 재정의 (property 사용)
    public new TData roomData
    {
        get => (TData)base.roomData;
        set => base.roomData = value;
    }

    // 현재 방 레벨 데이터 캐싱
    protected TLevel currentRoomLevelData;

    // 타입 캐스팅 없이 특화된 레벨 데이터 가져오기
    public TLevel GetCurrentLevelData()
    {
        // 데이터가 없거나 레벨이 변경되었을 때 업데이트
        if (currentRoomLevelData == null ||
            (currentRoomLevelData != null && currentRoomLevelData.level != currentLevel))
            UpdateRoomLevelData();

        return currentRoomLevelData;
    }

    /// <summary>
    /// 모든 방 타입에서 공통으로 사용할 수 있는 레벨 데이터 업데이트 메서드
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

    // Start에서 초기 레벨 데이터 로드
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

    // 레벨 업그레이드 시 데이터 업데이트
    public override bool Upgrade()
    {
        bool result = base.Upgrade();
        if (result) UpdateRoomLevelData();
        return result;
    }

    // 초기화 시 레벨 데이터 업데이트
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

    private void UpdateRoomVisual()
    {
        roomRenderer = GetComponent<SpriteRenderer>();
        if (roomRenderer != null && GetCurrentLevelData()?.roomSprite != null)
            roomRenderer.sprite = GetCurrentLevelData().roomSprite;
    }
}
