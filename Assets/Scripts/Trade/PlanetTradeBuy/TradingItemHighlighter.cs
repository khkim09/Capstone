using UnityEngine;

/// <summary>
/// TradingItem GameObject에 부착하여,
/// 선택된 아이템만 강조색으로 표시하도록 하는 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(TradingItem))]
public class TradingItemHighlighter : MonoBehaviour
{
    /// <summary>강조 시 사용할 색상입니다.</summary>
    [SerializeField] private Color highlightColor = Color.red;

    private TradingItem tradingItem;
    private SpriteRenderer boxRenderer;
    private Color originalColor;

    /// <summary>
    /// Awake에서 TradingItem, SpriteRenderer, 원래 색상을 캐싱합니다.
    /// </summary>
    private void Awake()
    {
        tradingItem   = GetComponent<TradingItem>();
        boxRenderer   = tradingItem.GetBoxRenderer();
        if (boxRenderer != null)
            originalColor = boxRenderer.color;
    }

    /// <summary>
    /// 강조(on/off) 상태를 설정합니다.
    /// </summary>
    /// <param name="on">true면 highlightColor, false면 원래 색상으로 복원</param>
    public void SetHighlight(bool on)
    {
        if (boxRenderer == null) return;
        boxRenderer.color = on ? highlightColor : originalColor;
    }
}
