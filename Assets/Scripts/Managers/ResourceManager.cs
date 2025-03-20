using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ResourceManager : MonoBehaviour
{
    // 자원 변경 이벤트
    public delegate void ResourceChangedHandler(ResourceType type, float newAmount);

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
                ResourceData data = new()
                {
                    type = type, amount = GetDefaultAmount(type), maxAmount = GetMaxAmount(type)
                };

                resources.Add(data);
            }
    }

    private float GetDefaultAmount(ResourceType type)
    {
        // 자원 유형별 기본값 설정
        switch (type)
        {
            case ResourceType.Fuel:
                return 100;
            case ResourceType.COMA:
                return 0;
            default:
                return 0;
        }
    }

    private float GetMaxAmount(ResourceType type)
    {
        // 자원 유형별 최대 제한값 설정
        switch (type)
        {
            case ResourceType.Fuel:
                return float.MaxValue;
            case ResourceType.COMA:
                return float.MaxValue;
            default:
                return float.MaxValue;
        }
    }

    // 자원 양 조회
    public float GetResource(ResourceType type)
    {
        ResourceData data = resources.Find(r => r.type == type);
        return data?.amount ?? 0;
    }

    // 자원 변경
    public void ChangeResource(ResourceType type, float amount)
    {
        ResourceData data = resources.Find(r => r.type == type);
        if (data != null)
        {
            float newAmount = Mathf.Clamp(data.amount + amount, 0, data.maxAmount);
            if (newAmount != data.amount)
            {
                data.amount = newAmount;
                OnResourceChanged?.Invoke(type, newAmount);

                // 자원 부족 체크
                if (data.amount <= 0) HandleResourceDepletion(type);
            }
        }

        Debug.Log($"Resource {type} changed: {amount}");
        Debug.Log($"Current resource amount: {GetResource(type)}");
    }

    // 자원 완전 고갈 처리
    private void HandleResourceDepletion(ResourceType type)
    {
        // 특정 자원 고갈 시 특별 로직
        switch (type)
        {
            case ResourceType.Fuel:
                // 연료 고갈 시 특별 이벤트 또는 게임 오버
                if (Random.value < 0.3f) GameManager.Instance.ChangeGameState(GameState.GameOver);
                Debug.Log("No fuel left! Ship is stranded.");
                break;
        }
    }

    [Serializable]
    public class ResourceData
    {
        public ResourceType type;
        public float amount;
        public float maxAmount;
    }
}
