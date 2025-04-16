using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    public Color defaultBG = new Color(0.4509804f, 0.4509804f, 0.4509804f, 0.6392157f);
    public Color defaultText = Color.white;

    /// <summary>
    /// 카테고리 버튼의 상태 갱신
    /// </summary>
    /// <param name="selectedCategory">선택된 카테고리</param>
    public void SetButtonState(RoomCategory selectedCategory)
    {
        foreach (Button btn in categoryButtons)
        {
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            Image img = btn.GetComponent<Image>();

            // 버튼 이름으로 카테고리 매칭
            RoomCategory category = (RoomCategory)System.Enum.Parse(typeof(RoomCategory), btn.name);

            bool isSelected = category == selectedCategory;

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
}
