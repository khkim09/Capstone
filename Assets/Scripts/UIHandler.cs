using UnityEngine;
using TMPro;
using System;

public class UIHandler : MonoBehaviour
{
    [Header("UI")]
    public GameObject createUIScreen;
    public GameObject testUIScreen;
    public GameObject CustomizeUI;

    // name, race field
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField raceInputField;
    [SerializeField] private int i = 0;

    // GridPlacer
    public GridPlacer gridPlacer;

    // customize button click
    public void OnCustomizeButtonClicked()
    {
        createUIScreen.SetActive(false);
        CustomizeUI.SetActive(true);
    }

    public void OnBackButtonClicked()
    {
        if (gridPlacer)
            gridPlacer.ClearGrid();

        CustomizeUI.SetActive(false);
        createUIScreen.SetActive(true);
    }

    // create button click
    public void OnCreateButtonClicked()
    {
        nameInputField.text = "";
        raceInputField.text = "";

        createUIScreen.SetActive(false);
        testUIScreen.SetActive(true);
    }

    // submit button click
    public void OnSubmitButtonClicked()
    {
        // 1) InputField에서 입력값 가져오기
        string inputName = nameInputField.text;
        string inputRace = raceInputField.text;
        CrewRace crewRace;

        // 올바른 입력 값 체크
        if (!Enum.TryParse<CrewRace>(inputRace, true, out crewRace) || !Enum.IsDefined(typeof(CrewRace), crewRace))
        {
            Debug.LogWarning("종족 입력 값 오류, Human으로 대체");
            crewRace = CrewRace.Human;
        }

        // 2) Cube 생성
        GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // 3) 생성된 Cube의 이름 설정
        Vector3 startPos = new Vector3(-8.0f, 0.0f, 0.0f);
        newCube.name = $"Name: {inputName}, Race: {inputRace}";
        newCube.transform.position = new Vector3(i++ * 1.5f, 0.0f, 0.0f) + startPos;

        // 4) component 추가
        CrewMember crewComp = newCube.AddComponent<CrewMember>();
        CrewManager.Instance.AddCrewMember(inputName, crewRace, crewComp);

        // 확인용 로그
        Debug.Log($"New Cube created with name: {newCube.name}");

        // 초기화
        testUIScreen.SetActive(false);
        createUIScreen.SetActive(true);
    }
}
