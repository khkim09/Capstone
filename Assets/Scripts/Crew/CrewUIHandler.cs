using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using NUnit.Framework.Constraints;
using UnityEditor.ShaderGraph;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 선원 생성, 종족 선택, 장비 구매 등 크루 관련 UI 전체를 관리하는 핸들러.
/// 화면 전환, 입력 처리, 버튼 이벤트 등 UI 흐름을 통합적으로 제어합니다.
/// </summary>
public class CrewUIHandler : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static CrewUIHandler Instance;

    /// <summary>
    /// 선원 생성 메인 화면입니다.
    /// </summary>
    [Header("UI Panels")] public GameObject mainUIScreen;

    /// <summary>
    /// 선원 생성 요청 화면입니다.
    /// </summary>
    public GameObject createUIScreen;

    /// <summary>
    /// 함선 커스터마이즈 UI 화면입니다.
    /// </summary>
    public GameObject customizeShipUIScreen;

    /// <summary>
    /// 장비 구매 UI 화면입니다.
    /// </summary>
    public GameObject addEquipmentUIScreen;

    /// <summary>
    /// 선원 최대 수용 시 표시되는 UI 화면입니다.
    /// </summary>
    public GameObject fullCrewUIScreen;

    /// <summary>
    /// 장비 UI 핸들러 참조입니다.
    /// </summary>
    [Header("Equipment UI Handler")] public EquipmentUIHandler equipmentUIHandler;


    /// <summary>
    /// 선원 이름 입력 필드입니다.
    /// </summary>
    [Header("Input Fields")] [SerializeField]
    private TMP_InputField nameInputField;

    /// <summary>
    /// 각 종족 선택 버튼 컨트롤러 배열입니다.
    /// </summary>
    public RaceButtonController[] raceButtonControllers;

    /// <summary>
    /// 현재 선택된 종족 버튼입니다.
    /// </summary>
    [SerializeField] private RaceButtonController currentSelectedButton;

    /// <summary>
    /// 현재 선택된 종족 정보입니다.
    /// </summary>
    [SerializeField] private CrewRace selectedRace = CrewRace.None;


    /// <summary>
    /// 최종 제출 버튼입니다.
    /// </summary>
    [Header("UI Buttons")] public Button submitButton;

    /// <summary>
    /// 인간형 종족 선택 버튼입니다.
    /// </summary>
    [Header("Race Buttons")] public Button humanButton;

    /// <summary>
    /// 부정형(아모르프) 종족 선택 버튼입니다.
    /// </summary>
    public Button amorphousButton;

    /// <summary>
    /// 돌격 기계형 종족 선택 버튼입니다.
    /// </summary>
    public Button mechanicTankButton;

    /// <summary>
    /// 지원 기계형 종족 선택 버튼입니다.
    /// </summary>
    public Button mechanicSupButton;

    /// <summary>
    /// 짐승형 종족 선택 버튼입니다.
    /// </summary>
    public Button beastButton;

    /// <summary>
    /// 곤충형 종족 선택 버튼입니다.
    /// </summary>
    public Button insectButton;

    /// <summary>
    /// 선원 이름
    /// </summary>
    private string inputName;

    /// <summary>
    /// 종족별 prefab
    /// </summary>
    [Header("Crew Prefabs")] [SerializeField]
    private GameObject[] crewPrefabs;

    // GridPlacer
    // public GridPlacer gridPlacer;

    /// <summary>
    /// UI 화면 이동 이력을 저장하는 스택 구조입니다.
    /// 뒤로 가기 기능에 사용됩니다.
    /// </summary>
    private Stack<GameObject> uiHistory = new(); // stack 구조

    /// <summary>
    /// 싱글턴 인스턴스를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }


    /// <summary>
    /// UI 초기 상태를 설정합니다.
    /// Submit 버튼을 비활성화하며, 버튼 이벤트 연결은 에디터에서 처리합니다.
    /// </summary>
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
    public GameObject GetCurrentActiveUI()
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
    /// 새로운 선원 생성 후 생성한 선원 반환
    /// </summary>
    /// <returns></returns>
    private CrewMember CreateCrewMember()
    {
        // 생성할 위치 (예시)
        Vector3 spawnPosition = new(-8f, 0f, 0f);

        // 선택된 종족에 맞는 프리팹 가져오기
        int raceIndex = (int)selectedRace - 1;
        if (raceIndex < 0 || raceIndex >= crewPrefabs.Length)
            Debug.LogError("선택된 종족에 맞는 프리팹을 찾을 수 없습니다.");

        GameObject crewGO = Instantiate(crewPrefabs[raceIndex], spawnPosition, Quaternion.identity);
        CrewMember newCrew = crewGO.GetComponent<CrewMember>();
        newCrew.crewName = inputName;
        newCrew.isPlayerControlled = true;
        newCrew.race = selectedRace;

        // 초기화
        newCrew.Initialize();

        return newCrew;
    }

    /// <summary>
    /// 선원 정보 입력 후 실제 생성하는 버튼 클릭 시 호출
    /// </summary>
    // 선원 추가 커밋
    public void OnSubmitButtonClicked()
    {
        inputName = nameInputField.text;

        // 선원 추가
        GameManager.Instance.GetPlayerShip().AddCrew(CreateCrewMember());

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
