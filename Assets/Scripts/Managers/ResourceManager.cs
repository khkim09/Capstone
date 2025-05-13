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
    public enum ResourceValueType
    {
        Int,
        Float
    }

    /// <summary>
    /// 자원 변경 이벤트 델리게이트입니다.
    /// </summary>
    /// <param name="type">변경된 자원 타입.</param>
    /// <param name="newAmount">변경 후 자원 수치.</param>
    public delegate void ResourceChangedHandler(ResourceType type, float newAmount);

    [SerializeField] private List<ResourceData> resources = new();

    public static ResourceManager Instance { get; private set; }

    public int COMA => GetCOMA();
    public float Fuel => GetFuel();

    public int Missile => GetMissle();

    public int Hypersonic => GetHypersonic();

    public int Missle => GetMissle();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (resources.Count == 0)
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                ResourceValueType valueType = GetValueType(type);

                ResourceData data = new() { type = type, valueType = valueType };

                if (valueType == ResourceValueType.Float)
                {
                    data.floatAmount = GetDefaultAmount(type);
                    data.maxFloatAmount = float.MaxValue;
                }
                else
                {
                    data.intAmount = (int)GetDefaultAmount(type);
                    data.maxIntAmount = int.MaxValue;
                }

                resources.Add(data);
            }
    }

    private int GetCOMA()
    {
        ResourceData data = resources.Find(r => r.type == ResourceType.COMA);
        return data?.intAmount ?? 0;
    }

    private float GetFuel()
    {
        ResourceData data = resources.Find(r => r.type == ResourceType.Fuel);
        return data?.floatAmount ?? 0;
    }

    private int GetMissle()
    {
        ResourceData data = resources.Find(r => r.type == ResourceType.Missile);
        return data?.intAmount ?? 0;
    }

    private int GetHypersonic()
    {
        ResourceData data = resources.Find(r => r.type == ResourceType.Hypersonic);
        return data?.intAmount ?? 0;
    }

    private float GetDefaultAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Fuel:
                return 100;
            case ResourceType.COMA:
                return 100000;
            case ResourceType.Hypersonic:
                return 0;
            case ResourceType.Missile:
                return 0;
            default:
                return 0;
        }
    }

    private float GetMaxAmount(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.COMA:
            case ResourceType.Missile:
            case ResourceType.Hypersonic:
                return int.MaxValue;
            case ResourceType.Fuel:
            default:
                return float.MaxValue;
        }
    }

    private ResourceValueType GetValueType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Fuel:
                return ResourceValueType.Float;
            case ResourceType.COMA:
            case ResourceType.Hypersonic:
            case ResourceType.Missile:
                return ResourceValueType.Int;
            default:
                return ResourceValueType.Float;
        }
    }

    public void ChangeResource(ResourceType type, float amount)
    {
        ResourceData data = resources.Find(r => r.type == type);

        if (data != null)
        {
            float currentAmount = data.GetAmount();
            float newAmount = currentAmount + amount;

            data.SetAmount(newAmount);
            OnResourceChanged?.Invoke(type, data.GetAmount());

            if (data.GetAmount() <= 0)
                HandleResourceDepletion(type);
        }

        Debug.Log($"Resource {type} changed: {amount}");
    }

    public void SetResource(ResourceType type, float newValue)
    {
        ResourceData data = resources.Find(r => r.type == type);
        if (data != null)
        {
            data.SetAmount(newValue);
            OnResourceChanged?.Invoke(type, data.GetAmount());
        }
    }


    private void HandleResourceDepletion(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Fuel:
                if (Random.value < 0.3f)
                    GameManager.Instance.ChangeGameState(GameState.GameOver);
                Debug.Log("No fuel left! Ship is stranded.");
                break;
        }
    }

    [Serializable]
    public class ResourceData
    {
        public ResourceType type;
        public ResourceValueType valueType;

        public float floatAmount;
        public int intAmount;

        public float maxFloatAmount;
        public int maxIntAmount;

        public float GetAmount()
        {
            return valueType == ResourceValueType.Float ? floatAmount : intAmount;
        }

        public void SetAmount(float value)
        {
            if (valueType == ResourceValueType.Float)
                floatAmount = Mathf.Clamp(value, 0, maxFloatAmount);
            else
                intAmount = Mathf.Clamp((int)value, 0, maxIntAmount);
        }
    }
}
