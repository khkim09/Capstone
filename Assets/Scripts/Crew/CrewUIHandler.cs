using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using NUnit.Framework.Constraints;
using UnityEditor.ShaderGraph;
using System.Collections;
using System.Collections.Generic;

public class CrewUIHandler : MonoBehaviour
{
    public static CrewUIHandler Instance; // singleton 참조

    [Header("UI Panels")]
    public GameObject mainUIScreen; // 선원 생성하기 메인 화면
    public GameObject createUIScreen; // 선원 생성 요청 화면
    public GameObject customizeShipUIScreen; // 함선 제작 화면
    public GameObject addEquipmentUIScreen; // 장비 구매 화면
    public GameObject fullCrewUIScreen; // 선원 full 화면

    [Header("Equipment UI Handler")]
    public EquipmentUIHandler equipmentUIHandler;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInputField; // input 선원 이름
    public RaceButtonController[] raceButtonControllers; // inspector 할당
    [SerializeField] private RaceButtonController currentSelectedButton;
    [SerializeField] private CrewRace selectedRace = CrewRace.None; // 선원 종류

    [Header("UI Buttons")]
    public Button submitButton;

    [Header("Race Buttons")]
    public Button humanButton;
    public Button amorphousButton;
    public Button mechanicTankButton;
    public Button mechanicSupButton;
    public Button beastButton;
    public Button insectButton;

    // GridPlacer
    public GridPlacer gridPlacer;

    private Stack<GameObject> uiHistory = new Stack<GameObject>(); // stack 구조

    void Awake()
    {
        Instance = this;
    }

    // click 함수 연결 (editor에서 연결이 일반적이지만 어떻게 할 건지 논의 후 수정)
    void Start()
    {
        // 초기 submit 버튼 비활성화
        submitButton.interactable = false;
    }

    void Update()
    {
        // 종족, 이름 모두 입력 시 submit 버튼 활성화화
        if (selectedRace != CrewRace.None && nameInputField.text != "")
            submitButton.interactable = true;
        else
            submitButton.interactable = false;
    }

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

    // 뒤로 가기
    public void OnBackButtonClicked()
    {
        if (uiHistory.Count > 0)
        {
            GameObject currentUI = GetCurrentActiveUI();
            if (currentUI)
            {
                if (currentUI == customizeShipUIScreen)
                    ResetCustomizeShipUI();
                else if (currentUI == addEquipmentUIScreen)
                    ResetEquipmentUI();
                currentUI.SetActive(false);
            }

            GameObject previousUI = uiHistory.Pop();
            previousUI.SetActive(true);
        }
    }

    // race 선택 button click
    public void OnRaceButtonClicked(RaceButtonController clickedButton)
    {
        // 이미 선택된 버튼 (선택 해제)
        if (currentSelectedButton != null && currentSelectedButton != clickedButton)
            currentSelectedButton.SetHighlighted(false);

        currentSelectedButton = clickedButton;
        currentSelectedButton.SetHighlighted(true);
        selectedRace = clickedButton.raceType;
    }

    // 함선 세팅 UI 초기화
    private void ResetCustomizeShipUI()
    {
        if (gridPlacer)
            gridPlacer.ClearGrid();
    }

    // customize button click
    public void OnCustomizeButtonClicked()
    {
        ShowUI(customizeShipUIScreen);
    }

    public void OnCancelButtonClicked()
    {
        if (gridPlacer)
            gridPlacer.ClearGrid();

        customizeShipUIScreen.SetActive(false);
        mainUIScreen.SetActive(true);
    }

    // 선원 생성 UI 초기화
    private void ResetCrewCreateUI()
    {
        currentSelectedButton.SetHighlighted(false);
        currentSelectedButton = null;
        selectedRace = CrewRace.None;
        nameInputField.text = "";
    }

    // 수용 가능 선원 꽉 참 UI 생성
    private void ActiveFullCrewUI()
    {
        fullCrewUIScreen.SetActive(true);
    }

    private void InActiveFullCrewUI()
    {
        fullCrewUIScreen.SetActive(false);
    }

    // 선원 추가
    public void OnCreateButtonClicked()
    {
        if (currentSelectedButton || nameInputField.text != "")
            ResetCrewCreateUI();

        // 선원 꽉 참
        if (CrewManager.Instance.currentCrewCount >= CrewManager.Instance.maxCrewCount)
        {
            ActiveFullCrewUI();
            Invoke("InActiveFullCrewUI", 2f);
            return;
        }

        ShowUI(createUIScreen);
    }

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

    private void ResetEquipmentUI()
    {
        if (equipmentUIHandler.itemBuyPanel.activeSelf)
            equipmentUIHandler.itemBuyPanel.SetActive(false);
    }

    // add equipment button click
    public void OnAddEquipmentButtonClicked()
    {
        ShowUI(addEquipmentUIScreen);
    }
}
