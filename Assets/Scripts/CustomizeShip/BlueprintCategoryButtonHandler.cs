using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 카테고리 별 분류 가능하도록 카테고리 버튼에 함수 연결
/// </summary>
public class BlueprintCategoryButtonHandler : MonoBehaviour
{
    /// <summary>
    /// 설계도 UI 오브젝트
    /// </summary>
    public BlueprintInventoryUI bpInventoryUI;

    /// <summary>
    /// 무기 인벤토리 UI
    /// </summary>
    public BlueprintWeaponInventoryUI bpWeaponInventoryUI;

    public BlueprintHullLevelInventoryUI hullInventoryUI;

    /// <summary>
    /// 카테고리 버튼들
    /// </summary>
    public List<Button> categoryButtons;

    /// <summary>
    /// 선택 된 카테고리의 배경, 글자 색 조정
    /// </summary>
    public Color selectedBG = Color.white;

    public Color selectedText = Color.black;

    /// <summary>
    /// 기본 카테고리 배경, 글자 색 조정
    /// </summary>
    public Color defaultBG = new(0.4509804f, 0.4509804f, 0.4509804f, 0.6392157f);

    public Color defaultText = Color.white;

    /// <summary>
    /// 내용 표시 영역 (방/무기 UI가 표시될 공간)
    /// </summary>
    public Transform contentRoot;

    /// <summary>
    /// 방 인벤토리 UI 게임 오브젝트
    /// </summary>
    public GameObject roomInventoryUIObject;

    /// <summary>
    /// 무기 인벤토리 UI 게임 오브젝트
    /// </summary>
    public GameObject weaponInventoryUIObject;

    /// <summary>
    /// 외갑판 레벨 선택 패널
    /// </summary>
    public GameObject hullInventoryUIObject;

    /// <summary>
    /// 카테고리 버튼의 상태 갱신
    /// </summary>
    /// <param name="selectedCategory">선택된 카테고리</param>
    public void SetButtonState(RoomCategory selectedCategory)
    {
        // 무기 UI와 외갑판 레벨 패널 비활성화, 방 UI 활성화
        if (weaponInventoryUIObject != null)
            weaponInventoryUIObject.SetActive(false);

        if (hullInventoryUIObject != null)
            hullInventoryUIObject.SetActive(false);

        if (roomInventoryUIObject != null)
            roomInventoryUIObject.SetActive(true);

        foreach (Button btn in categoryButtons)
        {
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            Image img = btn.GetComponent<Image>();

            // 버튼 이름으로 카테고리 매칭
            if (btn.name == "Weapon")
            {
                // 무기 버튼은 방 카테고리가 선택되었을 때 항상 비선택 상태
                img.color = defaultBG;
                if (btnText != null)
                    btnText.color = defaultText;
                continue;
            }

            RoomCategory category = (RoomCategory)System.Enum.Parse(typeof(RoomCategory), btn.name);

            bool isSelected = category == selectedCategory;

            img.color = isSelected ? selectedBG : defaultBG;
            if (btnText != null)
                btnText.color = isSelected ? selectedText : defaultText;
        }
    }

    /// <summary>
    /// 무기 카테고리 선택 시 버튼 상태 갱신
    /// </summary>
    public void SetWeaponButtonState()
    {
        // 방 UI 비활성화, 무기 UI와 외갑판 레벨 패널 활성화
        if (roomInventoryUIObject != null)
            roomInventoryUIObject.SetActive(false);

        if (hullInventoryUIObject != null)
            hullInventoryUIObject.SetActive(false);

        if (weaponInventoryUIObject != null)
            weaponInventoryUIObject.SetActive(true);

        foreach (Button btn in categoryButtons)
        {
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            Image img = btn.GetComponent<Image>();

            // 무기 버튼만 선택 상태로, 나머지는 비선택 상태로
            bool isSelected = btn.name == "Weapon";

            img.color = isSelected ? selectedBG : defaultBG;
            if (btnText != null)
                btnText.color = isSelected ? selectedText : defaultText;
        }
    }

    /// <summary>
    /// 외갑판 레벨 카테고리 선택 시 버튼 상태 갱신
    /// </summary>
    public void SetHullButtonState()
    {
        // 방 UI와 무기 UI 비활성화, 외갑판 레벨 UI 활성화
        if (roomInventoryUIObject != null)
            roomInventoryUIObject.SetActive(false);

        if (weaponInventoryUIObject != null)
            weaponInventoryUIObject.SetActive(false);

        if (hullInventoryUIObject != null)
            hullInventoryUIObject.SetActive(true);

        foreach (Button btn in categoryButtons)
        {
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            Image img = btn.GetComponent<Image>();

            // 외갑판 버튼만 선택 상태로, 나머지는 비선택 상태로
            bool isSelected = btn.name == "Hull";

            img.color = isSelected ? selectedBG : defaultBG;
            if (btnText != null)
                btnText.color = isSelected ? selectedText : defaultText;
        }
    }

    /// <summary>
    /// 필수 시설 - 인벤토리에 호출
    /// </summary>
    public void ShowEssential()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Essential);
        SetButtonState(RoomCategory.Essential);
    }

    /// <summary>
    /// 보조 시설 - 인벤토리에 호출
    /// </summary>
    public void ShowAuxiliary()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Auxiliary);
        SetButtonState(RoomCategory.Auxiliary);
    }

    /// <summary>
    /// 생활활 시설 - 인벤토리에 호출
    /// </summary>
    public void ShowLiving()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Living);
        SetButtonState(RoomCategory.Living);
    }

    /// <summary>
    /// 저장고 - 인벤토리에 호출
    /// </summary>
    public void ShowStorage()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Storage);
        SetButtonState(RoomCategory.Storage);
    }

    /// <summary>
    /// 복도 - 인벤토리에 호출
    /// </summary>
    public void ShowEtc()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Etc);
        SetButtonState(RoomCategory.Etc);
    }

    /// <summary>
    /// 무기 - 인벤토리에 호출
    /// </summary>
    public void ShowWeapons()
    {
        // 데이터베이스 초기화 확인
        if (bpWeaponInventoryUI.weaponDatabase != null)
            bpWeaponInventoryUI.weaponDatabase.Initialize();

        // 무기 목록 로드
        bpWeaponInventoryUI.LoadWeapons();
        SetWeaponButtonState();
    }


    /// <summary>
    /// 외갑판 카테고리 - 인벤토리에 호출
    /// </summary>
    public void ShowHull()
    {
        hullInventoryUI.LoadHullLevelButtons();
        SetHullButtonState();
    }
}
