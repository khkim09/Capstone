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

    public List<Button> categoryButtons;

    public Color selectedBG = Color.white;
    public Color selectedText = Color.black;

    public Color defaultBG = new Color(0.4509804f, 0.4509804f, 0.4509804f, 0.6392157f);
    public Color defaultText = Color.white;

    public void SetButtonState(RoomCategory selectedCategory)
    {
        foreach (var btn in categoryButtons)
        {
            var btnText = btn.GetComponentInChildren<TMP_Text>();
            var img = btn.GetComponent<Image>();

            // 버튼 이름으로 카테고리 매칭
            RoomCategory category = (RoomCategory)System.Enum.Parse(typeof(RoomCategory), btn.name);

            bool isSelected = category == selectedCategory;

            img.color = isSelected ? selectedBG : defaultBG;
            if (btnText != null)
                btnText.color = isSelected ? selectedText : defaultText;
        }
    }

    public void ShowEssential()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Essential);
        SetButtonState(RoomCategory.Essential);
    }

    public void ShowAuxiliary()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Auxiliary);
        SetButtonState(RoomCategory.Auxiliary);
    }

    public void ShowLiving()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Living);
        SetButtonState(RoomCategory.Living);
    }

    public void ShowStorage()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Storage);
        SetButtonState(RoomCategory.Storage);
    }

    public void ShowEtc()
    {
        bpInventoryUI.FilterByCategory(RoomCategory.Etc);
        SetButtonState(RoomCategory.Etc);
    }
}
