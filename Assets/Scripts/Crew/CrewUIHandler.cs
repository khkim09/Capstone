using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using NUnit.Framework.Constraints;
using UnityEditor.ShaderGraph;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 선원과 관련된 UI 모두 관리하는 Handler
/// </summary>
public class CrewUIHandler : MonoBehaviour
{
    public static CrewUIHandler Instance; // singleton 참조

    /// <summary>
    /// UI Panels
    //  - mainUIScreen : 선원 생성하기 메인 화면
    /// - createUIScreen : 선원 생성 요청 화면
    /// - customizeShipUIScreen : 함선 제작 화면
    /// - addEquipmentUIScreen : 장비 구매 화면
    /// - fullCrewUIScreen : 선원 full 화면
    /// </summary>
    [Header("UI Panels")]
    public GameObject mainUIScreen; // 선원 생성하기 메인 화면
    public GameObject createUIScreen; // 선원 생성 요청 화면
    public GameObject customizeShipUIScreen; // 함선 제작 화면
    public GameObject addEquipmentUIScreen; // 장비 구매 화면
    public GameObject fullCrewUIScreen; // 선원 full 화면

    [Header("Equipment UI Handler")] public EquipmentUIHandler equipmentUIHandler;

    [Header("Input Fields")]
    [SerializeField]
    private TMP_InputField nameInputField; // input 선원 이름

    public RaceButtonController[] raceButtonControllers; // inspector 할당
    [SerializeField] private RaceButtonController currentSelectedButton;
    [SerializeField] private CrewRace selectedRace = CrewRace.None; // 선원 종류

    [Header("UI Buttons")] public Button submitButton;

    [Header("Race Buttons")] public Button humanButton;
    public Button amorphousButton;
    public Button mechanicTankButton;
    public Button mechanicSupButton;
    public Button beastButton;
    public Button insectButton;

    // GridPlacer
    // public GridPlacer gridPlacer;

    private Stack<GameObject> uiHistory = new(); // stack 구조

    private void Awake()
    {
        Instance = this;
    }


    // click 함수 연결 (editor에서 연결이 일반적이지만 어떻게 할 건지 논의 후 수정)
    private void Start()
    {
        // 초기 submit 버튼 비활성화
        submitButton.interactable = false;
    }

    /// <summary>
    /// 종족, 이름 모두 입력 시 submit 버튼 활성화
    /// </summary>
    private void Update()
    {
        // 종족, 이름 모두 입력 시 submit 버튼 활성화화
        if (selectedRace != CrewRace.None && nameInputField.text != "")
            submitButton.interactable = true;
        else
            submitButton.interactable = false;
    }

    /// <summary>
    /// 현재 활성화 되어 있는 UI 반환
    /// </summary>
    /// <returns></returns>
    private GameObject GetCurrentActiveUI()
    {
        if (mainUIScreen.activeSelf)
            return mainUIScreen;
        if (createUIScreen.activeSelf)
            return createUIScreen;
        if (customizeShipUIScreen.activeSelf)
            return customizeShipUIScreen;
        if (addEquipmentUIScreen.activeSelf)
            return addEquipmentUIScreen;

        return null;
    }

    /// <summary>
    /// 인자로 받은 UI 활성화
    /// </summary>
    /// <param name="targetUI"></param>
    public void ShowUI(GameObject targetUI)
    {
        // UI history 저장
        GameObject currentUI = GetCurrentActiveUI();
        if (currentUI && currentUI != targetUI)
        {
            uiHistory.Push(currentUI);
            currentUI.SetActive(false);
        }

        targetUI.SetActive(true);

        if (targetUI != mainUIScreen && MenuUIManager.Instance != null)
            MenuUIManager.Instance.ForceCloseMenu();
    }

    /// <summary>
    /// UIHistory 개념으로 인터넷 뒤로 가기 버튼과 동일한 기능
    /// </summary>
    public void OnBackButtonClicked()
    {
        if (uiHistory.Count > 0)
        {
            GameObject currentUI = GetCurrentActiveUI();
            if (currentUI)
            {
                if (currentUI == addEquipmentUIScreen)
                    ResetEquipmentUI();
                currentUI.SetActive(false);
            }

            GameObject previousUI = uiHistory.Pop();
            previousUI.SetActive(true);
        }
    }

    /// <summary>
    /// 종족 선택하는 버튼 클릭 시 호출
    /// </summary>
    /// <param name="clickedButton"></param>
    public void OnRaceButtonClicked(RaceButtonController clickedButton)
    {
        // 이미 선택된 버튼 (선택 해제)
        if (currentSelectedButton != null && currentSelectedButton != clickedButton)
            currentSelectedButton.SetHighlighted(false);

        currentSelectedButton = clickedButton;
        currentSelectedButton.SetHighlighted(true);
        selectedRace = clickedButton.raceType;
    }

    /// <summary>
    /// 함선 커스터마이즈를 위한 버튼 클릭 시 호출
    /// </summary>
    public void OnCustomizeButtonClicked()
    {
        ShowUI(customizeShipUIScreen);
    }

    /// <summary>
    /// 함선 커스터마이즈 취소 버튼 클릭 시 호출
    /// </summary>
    public void OnCancelButtonClicked()
    {
        customizeShipUIScreen.SetActive(false);
        mainUIScreen.SetActive(true);
    }

    /// <summary>
    /// 선원 생성 시 UI 초기화
    /// </summary>
    private void ResetCrewCreateUI()
    {
        currentSelectedButton.SetHighlighted(false);
        currentSelectedButton = null;
        selectedRace = CrewRace.None;
        nameInputField.text = "";
    }

    /// <summary>
    /// 수용 가능 선원 꽉 참 UI 생성
    /// </summary>
    private void ActiveFullCrewUI()
    {
        fullCrewUIScreen.SetActive(true);
    }

    /// <summary>
    /// 수용 가능 선원 꽉 참 UI 해제
    /// </summary>
    private void InActiveFullCrewUI()
    {
        fullCrewUIScreen.SetActive(false);
    }

    /// <summary>
    /// 선원 추가를 위한 UI로 이동 버튼 클릭 시 호출
    /// </summary>
    public void OnCreateButtonClicked()
    {
        if (currentSelectedButton || nameInputField.text != "")
            ResetCrewCreateUI();

        // 선원 꽉 참
        if (GameManager.Instance.GetPlayerShip().GetCrewCount() >=
            GameManager.Instance.GetPlayerShip().GetMaxCrew())
        {
            ActiveFullCrewUI();
            Invoke("InActiveFullCrewUI", 2f);
            return;
        }

        ShowUI(createUIScreen);
    }

    /// <summary>
    /// 선원 정보 입력 후 실제 생성하는 버튼 클릭 시 호출
    /// </summary>
    // 선원 추가 커밋
    public void OnSubmitButtonClicked()
    {
        string inputName = nameInputField.text;

        // 선원 추가
        CrewManager.Instance.AddCrewMember(inputName, selectedRace);

        // 초기화
        ResetCrewCreateUI();

        createUIScreen.SetActive(false);
        mainUIScreen.SetActive(true);
    }

    /// <summary>
    /// 장비 구매를 물어보는 UI 해제
    /// </summary>
    private void ResetEquipmentUI()
    {
        if (equipmentUIHandler.itemBuyPanel.activeSelf)
            equipmentUIHandler.itemBuyPanel.SetActive(false);
    }

    /// <summary>
    /// 장비 구매 UI로의 이동 버튼 클릭 시 호출
    /// </summary>
    public void OnAddEquipmentButtonClicked()
    {
        ShowUI(addEquipmentUIScreen);
    }
}
