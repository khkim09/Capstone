using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using NUnit.Framework.Constraints;

public class CrewUIHandler : MonoBehaviour
{
    public static CrewUIHandler Instance; // singleton 참조

    [Header("UI Panels")]
    public GameObject mainUIScreen; // 선원 생성하기 메인 화면
    public GameObject createUIScreen; // 선원 생성 요청 화면
    public GameObject customizeShipUIScreen; // 함선 제작 화면

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInputField; // input 선원 이름
    public RaceButtonController[] raceButtonControllers; // inspector 할당
    [SerializeField] private RaceButtonController currentSelectedButton;
    [SerializeField] private CrewRace selectedRace = CrewRace.None; // 선원 종류
    [SerializeField] private int i = 0;

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

    // race 선택 button click
    public void OnRaceButtonClicked(RaceButtonController clickedButton)
    {
        // 이미 선택된 버튼 (선택 해제)
        if (currentSelectedButton != null && currentSelectedButton != clickedButton)
            currentSelectedButton.SetHighlighted(false);

        currentSelectedButton = clickedButton;
        currentSelectedButton.SetHighlighted(true);
        selectedRace = clickedButton.raceType;
        Debug.Log($"선택된 종족 : {selectedRace}");
    }

    // customize button click
    public void OnCustomizeButtonClicked()
    {
        mainUIScreen.SetActive(false);
        customizeShipUIScreen.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        if (gridPlacer)
            gridPlacer.ClearGrid();

        customizeShipUIScreen.SetActive(false);
        mainUIScreen.SetActive(true);
    }

    // create button click
    public void OnCreateButtonClicked()
    {
        currentSelectedButton = null;
        selectedRace = CrewRace.None;
        nameInputField.text = "";

        mainUIScreen.SetActive(false);
        createUIScreen.SetActive(true);
    }

    // submit button click
    public void OnSubmitButtonClicked()
    {
        /* cube 대신 img로 선원 생성 -> 선택한 선원 별 다르게 (kimchi run 참고) */

        // 1) InputField에서 입력값 가져오기
        string inputName = nameInputField.text;

        // 2) Cube 생성 및 위치 초기화
        GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 startPos = new Vector3(-8.0f, 0.0f, 0.0f);
        newCube.name = $"Name: {inputName}, Race: {selectedRace}";
        newCube.transform.position = new Vector3(i++ * 1.5f, 0.0f, 0.0f) + startPos;

        // 3) component 추가
        CrewMember crewComp = newCube.AddComponent<CrewMember>();
        CrewManager.Instance.AddCrewMember(inputName, selectedRace, crewComp);

        // 확인용 로그
        Debug.Log($"새로운 선원 : {crewComp.crewName} {crewComp.race}");

        // 초기화
        createUIScreen.SetActive(false);
        mainUIScreen.SetActive(true);
    }
}
