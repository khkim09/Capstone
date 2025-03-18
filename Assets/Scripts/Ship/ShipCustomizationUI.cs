using UnityEngine;
using UnityEngine.UI;

public class ShipCustomizationUI : MonoBehaviour
{
    [Header("References")]
    public ShipCustomizationManager customizationManager; // 모듈 배치 관리자
    public ShipLayoutValidator layoutValidator;           // 레이아웃 검증 관리자

    [Header("UI Elements")]
    public Text currencyText;           // 현재 재화를 표시할 텍스트
    public Button corridorButton;       // 복도 선택 버튼
    public Button doorButton;           // 문 선택 버튼
    public Button placeModuleButton;    // 모듈 배치 실행 버튼
    public Button clearModulesButton;   // 모든 모듈 삭제 버튼
    public Button validateLayoutButton; // 레이아웃 검증 버튼
    public Text feedbackText;           // 사용자에게 피드백을 줄 텍스트

    // 현재 선택된 모듈 프리팹 (복도, 문 등)
    private GameObject selectedModulePrefab = null;

    private void Start()
    {
        // 버튼 클릭 이벤트 설정
        if (corridorButton)
            corridorButton.onClick.AddListener(() => SelectModule("Corridor"));
        if (doorButton)
            doorButton.onClick.AddListener(() => SelectModule("Door"));
        if (placeModuleButton)
            placeModuleButton.onClick.AddListener(PlaceModuleAtMousePosition);
        if (clearModulesButton)
            clearModulesButton.onClick.AddListener(ClearModules);
        if (validateLayoutButton)
            validateLayoutButton.onClick.AddListener(ValidateLayout);

        UpdateCurrencyDisplay();
        feedbackText.text = "모듈을 선택하세요.";
    }

    /// <summary>
    /// 현재 재화 텍스트를 업데이트합니다.
    /// </summary>
    private void UpdateCurrencyDisplay()
    {
        if (currencyText)
            currencyText.text = "재화: " + customizationManager.currency.ToString();
    }

    /// <summary>
    /// 모듈 선택 버튼 클릭 시 호출됩니다.
    /// 선택된 모듈에 따라 해당 프리팹을 저장합니다.
    /// </summary>
    /// <param name="moduleType">모듈 타입 ("Corridor" 또는 "Door")</param>
    private void SelectModule(string moduleType)
    {
        switch (moduleType)
        {
            case "Corridor":
                selectedModulePrefab = customizationManager.corridorPrefab;
                feedbackText.text = "복도 선택됨. '모듈 배치' 버튼을 눌러 배치하세요.";
                break;
            case "Door":
                selectedModulePrefab = customizationManager.doorPrefab;
                feedbackText.text = "문 선택됨. '모듈 배치' 버튼을 눌러 배치하세요.";
                break;
            default:
                selectedModulePrefab = null;
                feedbackText.text = "모듈이 선택되지 않았습니다.";
                break;
        }
    }

    /// <summary>
    /// '모듈 배치' 버튼 클릭 시 현재 마우스 위치에 선택된 모듈을 배치합니다.
    /// </summary>
    private void PlaceModuleAtMousePosition()
    {
        if (selectedModulePrefab == null)
        {
            feedbackText.text = "먼저 모듈을 선택하세요.";
            return;
        }

        // 현재 마우스 위치를 월드 좌표로 변환
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        bool success = customizationManager.PlaceModule(selectedModulePrefab, worldPos);
        if (success)
        {
            feedbackText.text = $"{selectedModulePrefab.name} 배치 성공.";
            UpdateCurrencyDisplay();
        }
        else
        {
            feedbackText.text = $"{selectedModulePrefab.name} 배치 실패. 그리드 범위 또는 재화를 확인하세요.";
        }
    }

    /// <summary>
    /// '모듈 삭제' 버튼 클릭 시 모든 모듈을 삭제합니다.
    /// </summary>
    private void ClearModules()
    {
        customizationManager.ClearAllModules();
        UpdateCurrencyDisplay();
        feedbackText.text = "모든 모듈 삭제됨.";
    }

    /// <summary>
    /// '레이아웃 검증' 버튼 클릭 시 현재 배치된 레이아웃의 유효성을 검사합니다.
    /// </summary>
    private void ValidateLayout()
    {
        bool valid = layoutValidator.ValidateLayout();
        feedbackText.text = valid ? "레이아웃 유효합니다." : "레이아웃 오류: 방 연결을 확인하세요.";
    }
}
