using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 외갑판 레벨 선택 UI: 버튼 3개를 동적으로 생성하고 선택을 관리.
/// </summary>
public class BlueprintHullLevelInventoryUI : MonoBehaviour
{
    [Header("드래그 핸들러")] public BlueprintWeaponDragHandler weaponDragHandler;

    [Header("UI 참조")] public Transform contentRoot; // 버튼을 담을 공간 (예: Horizontal Layout Group)
    public GameObject levelButtonPrefab; // 버튼 프리팹

    [Header("시각 효과 색상")] public Color selectedColor = Color.white;
    public Color unselectedColor = new(0.7f, 0.7f, 0.7f, 0.7f);
    public Color selectedTextColor = Color.black;
    public Color unselectedTextColor = Color.white;

    private List<Button> levelButtons = new(3);
    private int currentLevel = 0;

    private void Start()
    {
        SetHullLevel(0); // 기본 선택 레벨 0
    }

    /// <summary>
    /// 외갑판 레벨 버튼 생성
    /// </summary>
    public void LoadHullLevelButtons()
    {
        // 기존 버튼 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);
        levelButtons.Clear();

        // 3개의 레벨 버튼 생성
        for (int i = 0; i < 3; i++)
        {
            GameObject btnGO = Instantiate(levelButtonPrefab, contentRoot);
            Button button = btnGO.GetComponent<Button>();
            TMP_Text text = btnGO.GetComponentInChildren<TMP_Text>();
            if (text != null)
                text.text = $"레벨 {i + 1}";

            int levelIndex = i;
            button.onClick.AddListener(() => SetHullLevel(levelIndex));
            levelButtons.Add(button);
        }

        // 모든 버튼에 선택/미선택 시각 효과 적용
        for (int i = 0; i < levelButtons.Count; i++)
            UpdateButtonVisuals(levelButtons[i], i == currentLevel);
    }

    /// <summary>
    /// 외갑판 레벨 선택
    /// </summary>
    public void SetHullLevel(int level)
    {
        currentLevel = level;

        Debug.Log("SetHullLevel: " + level);

        if (weaponDragHandler != null)
            weaponDragHandler.currentHullLevel = level;

        for (int i = 0; i < levelButtons.Count; i++) UpdateButtonVisuals(levelButtons[i], i == level);
    }

    /// <summary>
    /// 버튼 시각 효과
    /// </summary>
    private void UpdateButtonVisuals(Button button, bool isSelected)
    {
        if (button == null) return;

        Image img = button.GetComponent<Image>();
        if (img != null)
            img.color = isSelected ? selectedColor : unselectedColor;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.color = isSelected ? selectedTextColor : unselectedTextColor;
    }

    public int GetCurrentHullLevel()
    {
        return currentLevel;
    }
}
