using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Ship 템플릿을 저장하고 관리하는 클래스.
/// ShipSerialization 유틸리티를 활용하여 블루프린트를 Ship으로 변환 후 저장합니다.
/// </summary>
public class ShipTemplateSaver : MonoBehaviour
{
    /// <summary>
    /// 탬플릿 이름을 입력받는 필드
    /// </summary>
    [Header("UI - 저장")] public TMP_InputField templateNameInput;

    /// <summary>
    /// 저장 결과 피드백을 표시할 텍스트
    /// </summary>
    public TMP_Text feedbackText;

    /// <summary>
    /// 저장 버튼
    /// </summary>
    public Button saveButton;

    /// <summary>
    /// 블루프린트 함선 참조
    /// </summary>
    [Header("Ship References")] public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 플레이어 함선 참조 (템플릿 로드 시 사용)
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 그리드 플레이서 참조
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 템플릿 목록 패널
    /// </summary>
    [Header("UI - 목록")] public GameObject templateListPanel;

    /// <summary>
    /// 템플릿 목록 콘텐츠 영역
    /// </summary>
    public Transform templateListContent;

    /// <summary>
    /// 템플릿 아이템 프리팹
    /// </summary>
    public GameObject templateItemPrefab;

    /// <summary>
    /// 목록 버튼
    /// </summary>
    public Button listButton;

    /// <summary>
    /// 닫기 버튼
    /// </summary>
    public Button closeListButton;

    /// <summary>
    /// 새로고침 버튼
    /// </summary>
    public Button refreshButton;

    /// <summary>
    /// 템플릿 저장 경로 (기본값)
    /// </summary>
    private readonly string templateSavePath = "ShipTemplates";

    /// <summary>
    /// 활성화된 템플릿 목록 아이템
    /// </summary>
    private List<GameObject> activeTemplateItems = new();

    /// <summary>
    /// 초기화
    /// </summary>
    private void Start()
    {
        // 버튼 이벤트 연결
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveShipTemplate);

        if (listButton != null)
            listButton.onClick.AddListener(ShowTemplateList);

        if (closeListButton != null)
            closeListButton.onClick.AddListener(HideTemplateList);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshTemplateList);

        // 초기 상태 설정
        UpdateSaveButtonState();

        // 입력 필드 이벤트 연결
        if (templateNameInput != null)
            templateNameInput.onValueChanged.AddListener(OnTemplateNameChanged);

        // 템플릿 목록 패널 초기 숨김
        if (templateListPanel != null)
            templateListPanel.SetActive(false);

        // 템플릿 저장 디렉토리 확인 및 생성
        EnsureTemplateSaveDirectory();

        playerShip.Initialize();
    }

    /// <summary>
    /// 템플릿 저장 디렉토리 확인 및 생성
    /// </summary>
    private void EnsureTemplateSaveDirectory()
    {
        string directory = Path.Combine(Application.persistentDataPath, templateSavePath);
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
    }

    /// <summary>
    /// 템플릿 이름 변경 이벤트
    /// </summary>
    private void OnTemplateNameChanged(string newName)
    {
        UpdateSaveButtonState();
    }

    /// <summary>
    /// 저장 버튼 상태 업데이트
    /// </summary>
    private void UpdateSaveButtonState()
    {
        if (saveButton == null) return;

        bool hasValidName = !string.IsNullOrEmpty(templateNameInput?.text);
        bool hasBlueprints = targetBlueprintShip != null &&
                             targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>().Length > 0;

        saveButton.interactable = hasValidName && hasBlueprints;
    }

    /// <summary>
    /// 업데이트 호출마다 상태 검사
    /// </summary>
    private void Update()
    {
        UpdateSaveButtonState();
    }

    /// <summary>
    /// 함선 템플릿 저장
    /// </summary>
    public void SaveShipTemplate()
    {
        if (targetBlueprintShip == null)
        {
            ShowFeedback("There is no blueprint ship to save.");
            return;
        }

        if (string.IsNullOrEmpty(templateNameInput.text))
        {
            ShowFeedback("Please enter a template name.");
            return;
        }

        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();

        if (bpRooms.Length == 0)
        {
            ShowFeedback("There is no blueprint room in the blueprint ship.");
            return;
        }

        try
        {
            // Blueprint를 Ship으로 변환
            playerShip.ReplaceShipFromBlueprint(targetBlueprintShip);

            playerShip.UpdateOuterHullVisuals();

            // ShipSerialization을 사용하여 저장
            string templateFileName = GetTemplateFilePath(templateNameInput.text);
            ShipSerialization.SaveShip(playerShip, templateFileName);

            ShowFeedback($"Template '{templateNameInput.text}' is saved.");
            templateNameInput.text = "";
        }
        catch (Exception ex)
        {
            ShowFeedback($"Save failed: {ex.Message}");
            Debug.LogError($"템플릿 저장 실패: {ex}");
        }
    }

    /// <summary>
    /// 템플릿 파일 경로 생성
    /// </summary>
    /// <param name="templateName">템플릿 이름</param>
    /// <returns>전체 파일 경로</returns>
    private string GetTemplateFilePath(string templateName)
    {
        return Path.Combine(Application.persistentDataPath, templateSavePath, $"template_{templateName}.es3");
    }

    /// <summary>
    /// 템플릿 목록 표시
    /// </summary>
    public void ShowTemplateList()
    {
        if (templateListPanel != null)
        {
            templateListPanel.SetActive(true);
            RefreshTemplateList();
        }
    }

    /// <summary>
    /// 템플릿 목록 숨기기
    /// </summary>
    public void HideTemplateList()
    {
        if (templateListPanel != null) templateListPanel.SetActive(false);
    }

    /// <summary>
    /// 템플릿 목록 새로고침
    /// </summary>
    public void RefreshTemplateList()
    {
        // 기존 목록 아이템 제거
        ClearTemplateList();

        try
        {
            // 템플릿 파일 목록 가져오기
            string templateDir = Path.Combine(Application.persistentDataPath, templateSavePath);
            if (!Directory.Exists(templateDir))
            {
                ShowFeedback("템플릿 디렉토리가 없습니다.");
                return;
            }

            string[] templateFiles = Directory.GetFiles(templateDir, "template_*.es3");

            if (templateFiles.Length == 0)
            {
                ShowFeedback("저장된 템플릿이 없습니다.");
                return;
            }

            // 템플릿 목록 생성
            foreach (string templateFile in templateFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(templateFile);
                string templateName = fileName.Substring("template_".Length);

                AddTemplateItem(templateName);
            }

            ShowFeedback($"{templateFiles.Length}개의 템플릿이 있습니다.");
        }
        catch (Exception ex)
        {
            ShowFeedback($"템플릿 목록 불러오기 실패: {ex.Message}");
            Debug.LogError($"템플릿 목록 실패: {ex}");
        }
    }

    /// <summary>
    /// 기존 템플릿 목록 아이템 제거
    /// </summary>
    private void ClearTemplateList()
    {
        foreach (GameObject item in activeTemplateItems) Destroy(item);

        activeTemplateItems.Clear();
    }

    /// <summary>
    /// 템플릿 목록 아이템 추가
    /// </summary>
    /// <param name="templateName">템플릿 이름</param>
    private void AddTemplateItem(string templateName)
    {
        if (templateItemPrefab != null && templateListContent != null)
        {
            GameObject item = Instantiate(templateItemPrefab, templateListContent);
            activeTemplateItems.Add(item);

            // 이름 설정
            TMP_Text nameText = item.GetComponentInChildren<TMP_Text>();
            if (nameText != null) nameText.text = templateName;

            // 버튼 이벤트 설정
            Button[] buttons = item.GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
                if (button.name.Contains("Load") || button.name.Contains("로드"))
                {
                    string name = templateName; // 클로저 문제 방지
                    button.onClick.AddListener(() => LoadTemplate(name));
                }
                else if (button.name.Contains("Delete") || button.name.Contains("삭제"))
                {
                    string name = templateName; // 클로저 문제 방지
                    button.onClick.AddListener(() => DeleteTemplate(name));
                }
        }
    }

    /// <summary>
    /// 템플릿 로드하여 함선에 적용
    /// </summary>
    /// <param name="templateName">템플릿 이름</param>
    public void LoadTemplate(string templateName)
    {
        if (playerShip == null)
        {
            ShowFeedback("적용할 함선이 없습니다.");
            return;
        }

        try
        {
            string templateFilePath = GetTemplateFilePath(templateName);

            if (!File.Exists(templateFilePath))
            {
                ShowFeedback($"템플릿 '{templateName}'을(를) 찾을 수 없습니다.");
                return;
            }

            // ShipSerialization을 사용하여 함선 복원
            if (ShipSerialization.RestoreShip(templateFilePath, playerShip))
            {
                ShowFeedback($"템플릿 '{templateName}'이(가) 함선에 적용되었습니다.");
                HideTemplateList(); // 적용 후 목록 닫기
            }
            else
            {
                ShowFeedback("템플릿 적용 실패");
            }
        }
        catch (Exception ex)
        {
            ShowFeedback($"템플릿 적용 실패: {ex.Message}");
            Debug.LogError($"템플릿 로드 실패: {ex}");
        }
    }

    /// <summary>
    /// 템플릿 삭제
    /// </summary>
    /// <param name="templateName">삭제할 템플릿 이름</param>
    public void DeleteTemplate(string templateName)
    {
        try
        {
            string templateFilePath = GetTemplateFilePath(templateName);

            if (File.Exists(templateFilePath))
            {
                File.Delete(templateFilePath);
                ShowFeedback($"템플릿 '{templateName}'이(가) 삭제되었습니다.");
                RefreshTemplateList(); // 목록 갱신
            }
            else
            {
                ShowFeedback($"템플릿 '{templateName}'을(를) 찾을 수 없습니다.");
            }
        }
        catch (Exception ex)
        {
            ShowFeedback($"템플릿 삭제 실패: {ex.Message}");
            Debug.LogError($"템플릿 삭제 실패: {ex}");
        }
    }

    /// <summary>
    /// 피드백 메시지 표시
    /// </summary>
    /// <param name="message">표시할 메시지</param>
    private void ShowFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;

            // 일정 시간 후 메시지 지우기
            CancelInvoke("ClearFeedback");
            Invoke("ClearFeedback", 3f);
        }

        Debug.Log(message);
    }

    /// <summary>
    /// 피드백 메시지 지우기
    /// </summary>
    private void ClearFeedback()
    {
        if (feedbackText != null) feedbackText.text = "";
    }
}
