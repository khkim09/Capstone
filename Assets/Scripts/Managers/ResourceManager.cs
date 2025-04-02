using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 자원을 관리하는 매니저 클래스.
/// 자원의 초기화, 변경, 조회 및 고갈 시 처리까지 담당합니다.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    /// <summary>
    /// 자원 변경 이벤트 델리게이트입니다.
    /// </summary>
    /// <param name="type">변경된 자원 타입.</param>
    /// <param name="newAmount">변경 후 자원 수치.</param>
    public delegate void ResourceChangedHandler(ResourceType type, float newAmount);

    /// <summary>
    /// 현재 보유 중인 자원 목록입니다.
    /// </summary>
    [SerializeField] private List<ResourceData> resources = new();

    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static ResourceManager Instance { get; private set; }

    /// <summary>
    /// 인스턴스를 초기화하고 기본 자원을 세팅합니다.
    /// </summary>
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

    /// <summary>
    /// 자원 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event ResourceChangedHandler OnResourceChanged;

    /// <summary>
    /// 열거형에 정의된 모든 자원 유형을 초기화합니다.
    /// </summary>
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

    /// <summary>
    /// 자원별 기본 시작 수치를 반환합니다.
    /// </summary>
    /// <param name="type">자원 타입.</param>
    /// <returns>기본 수치.</returns>
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

    /// <summary>
    /// 자원별 최대 허용 수치를 반환합니다.
    /// </summary>
    /// <param name="type">자원 타입.</param>
    /// <returns>최대 수치.</returns>
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

    /// <summary>
    /// 특정 자원의 현재 수치를 반환합니다.
    /// </summary>
    /// <param name="type">조회할 자원 타입.</param>
    /// <returns>현재 수치.</returns>
    public float GetResource(ResourceType type)
    {
        ResourceData data = resources.Find(r => r.type == type);
        return data?.amount ?? 0;
    }


    /// <summary>
    /// 특정 자원의 수치를 변경합니다.
    /// 변경 후 이벤트를 발생시키며, 고갈 여부도 검사합니다.
    /// </summary>
    /// <param name="type">변경할 자원 타입.</param>
    /// <param name="amount">변경할 양 (증감).</param>
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

    /// <summary>
    /// 자원이 고갈되었을 때 호출됩니다.
    /// 자원 타입에 따라 특수 처리를 수행합니다.
    /// </summary>
    /// <param name="type">고갈된 자원 타입.</param>
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

    /// <summary>
    /// 자원 데이터를 나타내는 내부 클래스입니다.
    /// 자원의 타입, 현재 수치, 최대 수치를 포함합니다.
    /// </summary>
    [Serializable]
    public class ResourceData
    {
        /// <summary>
        /// 자원의 타입입니다.
        /// </summary>
        public ResourceType type;

        /// <summary>
        /// 현재 자원 수치입니다.
        /// </summary>
        public float amount;

        /// <summary>
        /// 자원의 최대 수치입니다.
        /// </summary>
        public float maxAmount;
    }
}
