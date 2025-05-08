using UnityEngine;

/// <summary>
/// 씬에 있는 모든 TradingItemHighlighter를 관리하며,
/// 선택된 아이템 이름과 일치할 때만 해당 오브젝트를 강조합니다.
/// </summary>
public class StorageHighlightManager : MonoBehaviour
{
    private TradingItemHighlighter[] allHighlighters;

    /// <summary>
    /// Awake 시점에 한 번만 씬에서 TradingItemHighlighter를 찾아 캐싱합니다.
    /// </summary>
    private void Awake()
    {
        allHighlighters = Object.FindObjectsByType<TradingItemHighlighter>(FindObjectsSortMode.None);
    }

    /// <summary>
    /// 전달된 itemName과 일치하는 TradingItem만 강조(on)하고,
    /// 나머지는 강조를 해제(off)합니다.
    /// </summary>
    /// <param name="itemName">하이라이트할 아이템 이름</param>
    public void HighlightItem(string itemName)
    {
        foreach (var high in allHighlighters)
        {
            // TradingItem.GetItemName() 으로 비교
            bool match = high.GetComponent<TradingItem>().GetItemName() == itemName;
            high.SetHighlight(match);
        }
    }

    /// <summary>
    /// 모든 TradingItem의 강조를 해제합니다.
    /// </summary>
    public void ClearHighlights()
    {
        foreach (var high in allHighlighters)
            high.SetHighlight(false);
    }
}
