using UnityEngine;
using TMPro;

public class UIHandler : MonoBehaviour
{
    // name, race field
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField raceInputField;

    // 버튼이 클릭되었을 때 실행할 함수
    public void OnSubmitButtonClicked()
    {
        // 1) InputField에서 입력값 가져오기
        string inputName = nameInputField.text;
        string inputRace = raceInputField.text;

        // 2) Cube 생성
        GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // 3) 생성된 Cube의 이름 설정
        newCube.name = $"Name: {inputName}, Race: {inputRace}";

        // (선택) 위치, 크기 등 원하는 대로 설정 가능
        newCube.transform.position = Vector3.zero;

        // 확인용 로그
        Debug.Log($"New Cube created with name: {newCube.name}");
    }
}
