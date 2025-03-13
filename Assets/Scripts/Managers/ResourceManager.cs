using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{
    // 자원 변경 이벤트
    public delegate void ResourceChangedHandler(ResourceType type, int newAmount);

    [SerializeField] private List<ResourceData> resources = new();
    public static ResourceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 기본 자원 설정
            InitializeResources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event ResourceChangedHandler OnResourceChanged;

    private void InitializeResources()
    {
        // 열거형에 정의된 모든 자원 유형 초기화
        if (resources.Count == 0)
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                var data = new ResourceData
                {
                    type = type,
                    amount = GetDefaultAmount(type),
                    maxAmount = GetMaxAmount(type)
                };

                resources.Add(data);
            }
    }

    private int GetDefaultAmount(ResourceType type)
    {
        // 자원 유형별 기본값 설정
        switch (type)
        {
            case ResourceType.Fuel:
                return 20;
            case ResourceType.Food:
                return 15;
            case ResourceType.Water:
                return 15;
            case ResourceType.Scrap:
                return 10;
            case ResourceType.Missiles:
                return 8;
            case ResourceType.Gold:
                return 0;
            default:
                return 0;
        }
    }

    private int GetMaxAmount(ResourceType type)
    {
        // 자원 유형별 최대 제한값 설정
        switch (type)
        {
            case ResourceType.Fuel:
                return 50;
            case ResourceType.Food:
                return 40;
            case ResourceType.Water:
                return 40;
            case ResourceType.Scrap:
                return 30;
            case ResourceType.Missiles:
                return 20;
            case ResourceType.Gold:
                return 100;
            default:
                return 999;
        }
    }

    // 자원 양 조회
    public int GetResource(ResourceType type)
    {
        var data = resources.Find(r => r.type == type);
        return data?.amount ?? 0;
    }

    // 자원 변경
    public void ChangeResource(ResourceType type, int amount)
    {
        var data = resources.Find(r => r.type == type);
        if (data != null)
        {
            var newAmount = Mathf.Clamp(data.amount + amount, 0, data.maxAmount);
            if (newAmount != data.amount)
            {
                data.amount = newAmount;
                OnResourceChanged?.Invoke(type, newAmount);

                // 자원 부족 체크
                if (data.amount <= 0) HandleResourceDepletion(type);
            }
        }
    }

    // 자원 완전 고갈 처리
    private void HandleResourceDepletion(ResourceType type)
    {
        // 특정 자원 고갈 시 특별 로직
        switch (type)
        {
            case ResourceType.Food:
                // 음식 고갈 시 모든 승무원에게 피해
                DefaultCrewManagerScript.Instance.ApplyEffectToAllCrew(new CrewEffect
                {
                    effectType = CrewEffectType.Damage,
                    healthChange = -5
                });
                Debug.Log("No food left! Crew is starving.");
                break;

            case ResourceType.Water:
                // 물 고갈 시 모든 승무원에게 상태 효과
                DefaultCrewManagerScript.Instance.ApplyEffectToAllCrew(new CrewEffect
                {
                    effectType = CrewEffectType.StatusChange,
                    statusEffect = CrewStatus.Sick
                });
                Debug.Log("No water left! Crew is getting sick.");
                break;

            case ResourceType.Fuel:
                // 연료 고갈 시 특별 이벤트 또는 게임 오버
                if (Random.value < 0.3f) GameManager.Instance.ChangeGameState(GameState.GameOver);
                Debug.Log("No fuel left! Ship is stranded.");
                break;
        }
    }

    // 일별 자원 소비
    public void ConsumeResourcesForNewDay()
    {
        // 기본 일일 소비량 설정
        var crewCount = DefaultCrewManagerScript.Instance.GetAliveCrewCount();

        ChangeResource(ResourceType.Food, -crewCount);
        ChangeResource(ResourceType.Water, -crewCount);
        ChangeResource(ResourceType.Fuel, -1);
    }

    [Serializable]
    public class ResourceData
    {
        public ResourceType type;
        public int amount;
        public int maxAmount;
    }
}
