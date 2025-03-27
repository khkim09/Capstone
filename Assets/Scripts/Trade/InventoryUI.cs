using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    // ScrollView Content로 사용할 컨테이너 (Hierarchy에서 Drag & Drop)
    [SerializeField] private Transform contentPanel;
    // 인벤토리 아이템 프리팹 (Project에서 Drag & Drop)
    [SerializeField] private GameObject inventoryItemPrefab;

    // Storage 컴포넌트 참조 (플레이어의 창고나 인벤토리를 관리하는 객체)
    private Storage storage;

    private void Start()
    {
        // Storage 컴포넌트를 씬에서 찾거나 직접 할당
        storage = FindObjectOfType<Storage>();

        // 인벤토리 UI 초기화
        PopulateInventory();
    }

    /// <summary>
    /// Storage에 저장된 모든 아이템을 기반으로 UI를 동적으로 생성합니다.
    /// </summary>
    public void PopulateInventory()
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
        }
    }
}
