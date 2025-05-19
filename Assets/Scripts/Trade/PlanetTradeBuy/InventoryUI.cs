using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// InventoryUI는 스크롤 뷰의 콘텐츠 패널에 인벤토리 아이템 UI를 동적으로 생성하고 관리하는 클래스입니다.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    /// <summary>
    /// ScrollView의 Content 패널을 참조합니다. (Hierarchy에서 Drag & Drop)
    /// </summary>
    [SerializeField] private Transform contentPanel;

    /// <summary>
    /// 인벤토리 아이템 프리팹을 참조합니다. (Project에서 Drag & Drop)
    /// </summary>
    [SerializeField] private GameObject inventoryItemPrefab;

    /// <summary>
    /// 플레이어의 창고를 관리하는 Storage 컴포넌트를 참조합니다.
    /// </summary>
    [SerializeField] private Storage storage;

    /// <summary>
    /// MonoBehaviour의 Start 메서드로, 초기화 작업을 수행하고 인벤토리 UI를 생성합니다.
    /// </summary>
    private void Start()
    {
        if (storage == null)
            storage = Object.FindFirstObjectByType<Storage>();

        PopulateInventory();
    }

    /// <summary>
    /// Storage에 저장된 모든 아이템을 기반으로 UI를 동적으로 생성합니다.
    /// 기존의 아이템 UI를 제거한 후 새롭게 생성하여 중복 생성을 방지합니다.
    /// </summary>
    /// <param name="makeInteractable">생성된 슬롯의 클릭 가능 여부 설정</param>
    public void PopulateInventory(bool makeInteractable = true)
    {
        Debug.Log("[InventoryUI] Populating Inventory UI...");

        // 기존 아이템 UI 제거
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Storage에서 모든 아이템 가져오기 (구매가 포함)
        List<(TradingItemData itemData, int quantity, float purchasePrice)> itemList = storage.GetAllItems();

        // 각 아이템에 대해 UI 슬롯 생성
        foreach (var (itemData, quantity, purchasePrice) in itemList)
        {
            GameObject newItemUI = Instantiate(inventoryItemPrefab, contentPanel);
            InventoryItemUI itemUI = newItemUI.GetComponent<InventoryItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(itemData, quantity, purchasePrice);
                itemUI.isInteractable = makeInteractable;
            }
        }
    }

    /// <summary>
    /// 인벤토리 패널을 비활성화합니다.
    /// </summary>
    public void CloseSelfPanel()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
