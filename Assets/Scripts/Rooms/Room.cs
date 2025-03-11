using System.Collections.Generic;
using UnityEngine;

public abstract class Room : MonoBehaviour
{
    public Vector2Int size = Vector2Int.one;

    public Vector2Int position;

    [SerializeField] protected float maxHealth = 100f; // 최대 체력
    [SerializeField] protected float currentHealth; // 현재 체력

    [SerializeField] protected OxygenLevel oxygenLevel = OxygenLevel.Normal; // 현재 산소 레벨

    [SerializeField] protected int minCrewRequired; // 작동에 필요한 최소 선원 수

    [SerializeField] protected int maxPowerLevel = 2; // 방의 최대 레벨 (예: 2단계 방어막 방이면 2)

    [SerializeField] protected int[] powerRequirements = { 0, 2, 4 }; // 각 레벨별 필요 전력 (0레벨, 1레벨, 2레벨)

    protected List<Room> adjacentRooms = new(); // 인접한 방들

    protected List<Door> connectedDoors = new(); // 연결된 문들

    protected Dictionary<OxygenLevel, float> crewDamagePerOxygen = new()
    {
        { OxygenLevel.None, 10f },
        { OxygenLevel.Critical, 5f },
        { OxygenLevel.Low, 0f },
        { OxygenLevel.Medium, 0f },
        { OxygenLevel.Normal, 0f }
    };

    protected List<CrewMember> crewInRoom;
    protected int currentPowerLevel; // 현재 실제로 작동 중인 레벨
    protected float efficiency = 1.0f; // 방의 효율 (업그레이드, 상태에 따라 변동)

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
    protected Color lowOxygenColor = new(1f, 0.8f, 0.8f); // 산소 부족시 색상

    protected Color normalColor = Color.white; // 기본 방 색상
    protected float powerConsumption;
    protected SpriteRenderer roomRenderer; // 방 렌더러

    [SerializeField]
    protected SpriteRenderer spriteRenderer;

    public bool isPlaced { get; protected set; }

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

    public virtual void OnCrewEnter(CrewMember crew)
    {
        crewInRoom.Add(crew);
    }

    public virtual void OnCrewExit(CrewMember crew)
    {
        crewInRoom.Remove(crew);
    }


    protected virtual void UpdateRoom()
    {
    } // 매 프레임/틱마다 방의 상태 업데이트

    protected virtual void UpdatePowerLevel()
    {
        // 체력 기반으로 수용 가능한 최대 전력 계산
        var healthRatio = currentHealth / maxHealth;
        var availablePower = Mathf.FloorToInt(powerRequirements[maxPowerLevel] * healthRatio);

        // 가능한 최대 레벨 찾기
        currentPowerLevel = 0;
        isPowered = false;

        for (var level = maxPowerLevel; level > 0; level--)
            if (availablePower >= powerRequirements[level])
            {
                currentPowerLevel = level;
                isPowered = true;
                break;
            }

        // 현재 레벨에 맞는 성능 설정
        ApplyCurrentLevelEffects();
    }

    // 각 방 타입별로 오버라이드하여 레벨별 효과 구현
    protected virtual void ApplyCurrentLevelEffects()
    {
        // 기본 구현은 비어있음
        // 각 방 타입별로 구현
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
        currentHealth = maxHealth;
        crewInRoom = new List<CrewMember>();
    }

    // 선원 관련 함수들
    public int GetCrewCount()
    {
        return crewInRoom.Count;
    }

    public bool HasEnoughCrew()
    {
        return crewInRoom.Count >= minCrewRequired;
    }

    // 상태 체크 함수들
    public bool IsOperational()
    {
        return isActive && isPowered && HasEnoughCrew();
    }

    public bool NeedsRepair()
    {
        return currentHealth < maxHealth;
    }

    public virtual void SetOxygenLevel(OxygenLevel level)
    {
        oxygenLevel = level;
        UpdateRoomVisuals();
        UpdateRoomEffects();
    }

    protected virtual void UpdateRoomVisuals()
    {
        if (roomRenderer == null) return;

        // 산소 레벨에 따른 방 색상 조정
        var targetColor = normalColor;

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
    }


    protected virtual void UpdateRoomEffects()
    {
        // 현재 방에 있는 선원들에게 데미지 적용
        if (crewDamagePerOxygen[oxygenLevel] > 0)
            foreach (var crew in crewInRoom)
            {
                //crew.TakeDamage(crewDamagePerOxygen[(int)oxygenLevel] * Time.deltaTime);
            }

        // 화재 진압 속도 조정
        if (HasFire())
            UpdateFireExtinguishRate(fireExtinguishRatePerLevel[oxygenLevel]);
    }


    // 데미지 처리
    public virtual void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (currentHealth <= 0) OnDisabled();

        // 체력에 따른 효율/전력소비 조정
        efficiency = currentHealth / maxHealth;
        powerConsumption *= efficiency;
    }

    // 수리 처리
    public virtual void Repair(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        efficiency = currentHealth / maxHealth;
    }

    // 방 비활성화시 호출
    protected virtual void OnDisabled()
    {
        isActive = false;
        // 방별 비활성화 처리
    }

    // 전력 관련
    public virtual void PowerUp()
    {
        isPowered = true;
    }

    public virtual void PowerDown()
    {
        isPowered = false;
    }

    // getter 함수들
    public float GetEfficiency()
    {
        return efficiency;
    }

    public float GetPowerConsumption()
    {
        return powerConsumption;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth * 100f;
    }
}
