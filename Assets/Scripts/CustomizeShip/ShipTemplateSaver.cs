using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

/// <summary>
/// 함선 템플릿을 저장하는 기능을 제공하는 클래스.
/// 현재 배치된 Blueprint Room을 유효성 검사 없이 템플릿으로 저장합니다.
/// </summary>
public class ShipTemplateSaver : MonoBehaviour
{
    /// <summary>
    /// 탬플릿 이름을 입력받는 필드
    /// </summary>
    [Header("UI")]
    public TMP_InputField templateNameInput;

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
    [Header("References")]
    public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 그리드 플레이서 참조
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 템플릿 저장 데이터 클래스
    /// </summary>
    [Serializable]
    public class ShipTemplateData
    {
        public string templateName;
        public List<BlueprintRoomSaveData> rooms = new List<BlueprintRoomSaveData>();
    }

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveShipTemplate);

        // 초기 버튼 상태 설정
        UpdateSaveButtonState();

        // 입력 필드 변경 이벤트 리스너 추가
        if (templateNameInput != null)
            templateNameInput.onValueChanged.AddListener(OnTemplateNameChanged);
    }

    /// <summary>
    /// 템플릿 이름 변경 시 저장 버튼 상태 업데이트
    /// </summary>
    /// <param name="newText">새 템플릿 이름</param>
    private void OnTemplateNameChanged(string newText)
    {
        UpdateSaveButtonState();
    }

    /// <summary>
    /// 저장 버튼 상태 업데이트 (템플릿 이름과 블루프린트 유무에 따라)
    /// </summary>
    private void UpdateSaveButtonState()
    {
        if (saveButton == null) return;

        bool hasValidName = !string.IsNullOrEmpty(templateNameInput?.text);
        bool hasBlueprints = targetBlueprintShip != null && targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>().Length > 0;

        saveButton.interactable = hasValidName && hasBlueprints;
    }

    /// <summary>
    /// Update 메서드에서 상태 체크
    /// </summary>
    private void Update()
    {
        UpdateSaveButtonState();
    }

    /// <summary>
    /// 현재 배치된 블루프린트 룸을 템플릿으로 저장
    /// 유효성 검사 없이 Easy Save를 통해 저장합니다.
    /// </summary>
    public void SaveShipTemplate()
    {
        if (targetBlueprintShip == null)
        {
            ShowFeedback("타겟 블루프린트 함선이 없습니다.");
            return;
        }

        if (string.IsNullOrEmpty(templateNameInput.text))
        {
            ShowFeedback("템플릿 이름을 입력해주세요.");
            return;
        }

        BlueprintRoom[] bpRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();

        if (bpRooms.Length == 0)
        {
            ShowFeedback("저장할 블루프린트 방이 없습니다.");
            return;
        }

        // 템플릿 데이터 생성
        ShipTemplateData templateData = new ShipTemplateData
        {
            templateName = templateNameInput.text
        };

        // 블루프린트 방 데이터 저장
        foreach (BlueprintRoom bpRoom in bpRooms)
        {
            BlueprintRoomSaveData roomData = new BlueprintRoomSaveData
            {
                bpRoomData = bpRoom.bpRoomData,
                bpLevelIndex = bpRoom.bpLevelIndex,
                bpPosition = bpRoom.bpPosition,
                bpRotation = bpRoom.bpRotation
            };

            templateData.rooms.Add(roomData);
        }

        // Easy Save를 사용하여 템플릿 저장
        try
        {
            string templateKey = "ShipTemplate_" + templateNameInput.text;
            ES3.Save(templateKey, templateData);
            ShowFeedback($"템플릿 '{templateNameInput.text}'이(가) 성공적으로 저장되었습니다.");

            // 저장 성공 후 입력 필드 초기화 (선택 사항)
            templateNameInput.text = "";
        }
        catch (Exception ex)
        {
            ShowFeedback($"템플릿 저장 실패: {ex.Message}");
            Debug.LogError($"Failed to save template: {ex}");
        }
    }

    /// <summary>
    /// 저장한 템플릿 불러오기
    /// </summary>
    /// <param name="templateName">불러올 템플릿 이름</param>
    public void LoadShipTemplate(string templateName)
    {
        string templateKey = "ShipTemplate_" + templateName;

        if (ES3.KeyExists(templateKey))
        {
            try
            {
                // 기존 블루프린트 제거
                if (targetBlueprintShip != null)
                    targetBlueprintShip.ClearRooms();

                if (gridPlacer != null)
                    gridPlacer.occupiedGridTiles = new HashSet<Vector2Int>();

                // 템플릿 데이터 불러오기
                ShipTemplateData templateData = ES3.Load<ShipTemplateData>(templateKey);

                // 블루프린트 방 배치
                foreach (BlueprintRoomSaveData roomData in templateData.rooms)
                {
                    gridPlacer.PlaceRoom(
                        roomData.bpRoomData,
                        roomData.bpLevelIndex,
                        roomData.bpPosition,
                        roomData.bpRotation
                    );
                }

                ShowFeedback($"템플릿 '{templateName}'을(를) 성공적으로 불러왔습니다.");
            }
            catch (Exception ex)
            {
                ShowFeedback($"템플릿 불러오기 실패: {ex.Message}");
                Debug.LogError($"Failed to load template: {ex}");
            }
        }
        else
        {
            ShowFeedback($"템플릿 '{templateName}'을(를) 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 저장된 모든 템플릿 이름 목록 가져오기
    /// </summary>
    /// <returns>템플릿 이름 목록</returns>
    public List<string> GetAllTemplateNames()
    {
        List<string> templateNames = new List<string>();

        // 모든 키 가져오기
        string[] allKeys = ES3.GetKeys();

        // 템플릿 키 필터링
        foreach (string key in allKeys)
        {
            if (key.StartsWith("ShipTemplate_"))
            {
                string templateName = key.Substring("ShipTemplate_".Length);
                templateNames.Add(templateName);
            }
        }

        return templateNames;
    }

    /// <summary>
    /// 저장된 템플릿 삭제하기
    /// </summary>
    /// <param name="templateName">삭제할 템플릿 이름</param>
    /// <returns>삭제 성공 여부</returns>
    public bool DeleteTemplate(string templateName)
    {
        string templateKey = "ShipTemplate_" + templateName;

        if (ES3.KeyExists(templateKey))
        {
            ES3.DeleteKey(templateKey);
            ShowFeedback($"템플릿 '{templateName}'이(가) 삭제되었습니다.");
            return true;
        }
        else
        {
            ShowFeedback($"템플릿 '{templateName}'을(를) 찾을 수 없습니다.");
            return false;
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

            // 일정 시간 후 메시지 지우기 (선택 사항)
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
        if (feedbackText != null)
            feedbackText.text = "";
    }
}
