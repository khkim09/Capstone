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
    /// 플레이어의 창고나 인벤토리를 관리하는 Storage 컴포넌트를 참조합니다.
    /// </summary>
    [SerializeField] private Storage storage;

    /// <summary>
    /// MonoBehaviour의 Start 메서드로, 초기화 작업을 수행하고 인벤토리 UI를 생성합니다.
    /// </summary>
    private void Start()
    {
        if (storage == null)
            storage = FindObjectOfType<Storage>();

        PopulateInventory();
    }


    /// <summary>
    /// Storage에 저장된 모든 아이템을 기반으로 UI를 동적으로 생성합니다.
    /// 기존의 아이템 UI를 제거한 후 새롭게 생성하여 중복 생성을 방지합니다.
    /// </summary>
    public void PopulateInventory(bool makeInteractable = true)
    {
        Debug.Log("Populating...");
        // 기존 아이템 UI 제거 (갱신 시 중복 생성을 방지)
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Storage의 storedItems 리스트를 순회하며 UI 생성
        foreach (StoredItem storedItem in storage.storedItems)
        {
            Debug.Log($"Instantiating {storage.storedItems}");
            // 프리팹 인스턴스 생성
            GameObject newItemUI = Instantiate(inventoryItemPrefab, contentPanel);
            InventoryItemUI itemUI = newItemUI.GetComponent<InventoryItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(storedItem);
            }
            /// 슬롯이 클릭 가능할지 여부를 외부에서 결정할 수 있도록 처리
            itemUI.isInteractable = makeInteractable;
        }
    }
}
