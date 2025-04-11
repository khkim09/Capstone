using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TradingItem 클래스는 거래 가능한 아이템의 데이터를 담고 있으며,
/// 아이템의 속성(행성 코드, 티어, 이름, 상태, 분류 등)과 가격 관련 기능을 제공합니다.
/// </summary>
[Serializable]
public class TradingItem : MonoBehaviour
{
    /// <summary>
    /// 아이템 상태
    /// </summary>
    [Tooltip("아이템 상태")] public ItemState itemState;

    [SerializeField] private TradingItemData itemData;

    public int amount;

    private SpriteRenderer spriteRenderer;

    private Image itemImage;

    // 한 번 계산된 최종 가격을 캐싱할 변수
    private float? cachedPrice = null;

    // 처음 호출되었는지 여부 (한 번만 초기화할지 판단)
    private bool priceInitialized = false;

    private void Start()
    {
    }

    public void Initialize(TradingItemData data, int quantity, ItemState state = ItemState.Normal)
    {
        itemData = data;
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        InitializeTradingShape();
        spriteRenderer.sortingOrder = SortingOrderConstants.TradingItem;
        itemState = state;

        amount = quantity;
        Math.Clamp(amount, 0, data.capacity);
        amount = 1; // 테스트용 하나
        // TODO: 만약 최대치보다 많으면 생성을 못하게 하거나, 두 개 생성해서 나눠야함
    }

    /// <summary>
    /// 현재 가격을 반환합니다.
    /// 아직 초기화되지 않았다면 RecalculatePrice()를 호출하여 basePrice ± fluctuation 범위에서
    /// 랜덤으로 결정한 후, 동일한 값을 반환합니다.
    /// </summary>
    /// <returns>현재 계산된 가격</returns>
    public float GetCurrentPrice()
    {
        // 최초 한 번만 가격 결정
        if (!priceInitialized)
        {
            RecalculatePrice();
            priceInitialized = true;
        }

        return cachedPrice.Value;
    }

    /// <summary>
    /// 외부에서 가격을 갱신할 필요가 있을 때 호출합니다.
    /// basePrice ± fluctuation 범위 내에서 새로 랜덤으로 가격을 계산합니다.
    /// 음수가 되지 않도록 보정합니다.
    /// </summary>
    public void RecalculatePrice()
    {
    }

    /// <summary>
    /// 원본 가격(BasePrice)을 반환합니다.
    /// 변동폭이나 기타 가격 계산 로직과 무관하게, JSON에 정의된 기본 가격을 그대로 반환합니다.
    /// 인벤토리 아이템의 가격을 나타낼 때 사용됩니다.
    /// </summary>
    /// <returns>아이템의 기본 가격(basePrice)</returns>
    public float GetBasePrice()
    {
        return itemData.costBase;
    }

    public TradingItemData GetItemData()
    {
        return itemData;
    }

    public void InitializeTradingShape()
    {
        string boxName = "lot";
        Sprite[] sprites = Resources.LoadAll<Sprite>($"Sprites/Item/{boxName}");

        spriteRenderer.sprite = sprites[itemData.shape];
    }
}
