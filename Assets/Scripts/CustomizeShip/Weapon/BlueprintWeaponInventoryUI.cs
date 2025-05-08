using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무기 인벤토리 UI: ShipWeaponDatabase에서 모든 무기를 불러와 UI로 나열.
/// </summary>
public class BlueprintWeaponInventoryUI : MonoBehaviour
{
    /// <summary>
    /// 무기 데이터베이스 참조
    /// </summary>
    [Header("데이터베이스 참조")] public ShipWeaponDatabase weaponDatabase;

    [Header("UI Reference")]
    /// <summary>
    /// 모든 무기 콘텐츠 넣을 공간
    /// </summary>
    public Transform contentRoot; // ScrollView > Viewport > Content

    /// <summary>
    /// 설치를 위한 무기 버튼 prefab
    /// </summary>
    public GameObject weaponButtonPrefab;

    /// <summary>
    /// 무기 드래그 관리 handler
    /// </summary>
    public BlueprintWeaponDragHandler dragHandler;

    /// <summary>
    /// 카테고리 분류 handler
    /// </summary>
    public BlueprintCategoryButtonHandler categoryButtonHandler;

    /// <summary>
    /// 특정 무기 타입만 표시할지 여부
    /// </summary>
    [Header("무기 타입 필터")] public bool filterByType = false;

    /// <summary>
    /// 표시할 무기 타입 (filterByType이 true일 경우에만 사용)
    /// </summary>
    public ShipWeaponType weaponTypeFilter;

    /// <summary>
    /// 무기 목록 로드 (카테고리 버튼에서 호출됨)
    /// </summary>
    public void LoadWeapons()
    {
        // 데이터베이스가 없으면 무시
        if (weaponDatabase == null)
        {
            Debug.LogError("ShipWeaponDatabase가 설정되지 않았습니다.");
            return;
        }

        // 기존 무기 버튼 모두 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        // 무기 목록 가져오기
        List<ShipWeaponData> weaponsToShow;

        if (filterByType)
            // 특정 타입의 무기만 표시
            weaponsToShow = weaponDatabase.GetWeaponsByType(weaponTypeFilter);
        else
            // 모든 무기 표시
            weaponsToShow = weaponDatabase.allWeapons;

        // 모든 무기 데이터로 버튼 생성
        foreach (ShipWeaponData data in weaponsToShow)
        {
            if (data == null)
                continue;

            GameObject btnGO = Instantiate(weaponButtonPrefab, contentRoot);
            WeaponInventoryButton btn = btnGO.GetComponent<WeaponInventoryButton>();
            btn.Initialize(data, dragHandler);
        }
    }
}
