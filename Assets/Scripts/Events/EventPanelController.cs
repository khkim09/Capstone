using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventPanelController : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private TextMeshProUGUI eventTitleText;

    [SerializeField] private TextMeshProUGUI eventDescriptionText;

    [SerializeField] private GameObject choiceButtonPrefab;

    // 계속하기 버튼 추가
    [SerializeField] private Button continueButton;

    [SerializeField] private TextMeshProUGUI outcomeText;

    [SerializeField] private GameObject choiceContainer;

    [Header("Settings")] [SerializeField] private float textTypingSpeed = 0.05f;

    private RandomEvent currentEvent;

    private void Start()
    {
        EventManager.Instance.EventPanelController = this;

        // 계속하기 버튼에 이벤트 연결
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(CloseEventPanel);
            continueButton.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 이벤트 표시 - UIManager에서 호출
    /// </summary>
    public void ShowEvent(RandomEvent randomEvent)
    {
        currentEvent = randomEvent;
        gameObject.SetActive(true);
        // 이전 타이핑 코루틴 강제 종료
        StopAllCoroutines();

        // UI 초기화
        if (eventTitleText != null)
            eventTitleText.text = "";
        if (eventDescriptionText != null)
            eventDescriptionText.text = "";
        if (outcomeText != null)
            outcomeText.text = "";

        // 계속하기 버튼 숨기기
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        // 선택지 클리어
        ClearChoices();


        // 이벤트 정보 설정
        if (eventTitleText != null)
            eventTitleText.text = randomEvent.eventTitle.Localize();
        if (eventDescriptionText != null)
        {
            if (randomEvent.eventType == EventType.Planet)
            {
                string planetName = GameManager.Instance.PlanetDataList[randomEvent.planetId].planetName;
                StartCoroutine(TypeText(eventDescriptionText, randomEvent.eventDescription.Localize(planetName)));
            }
            else
            {
                StartCoroutine(TypeText(eventDescriptionText, randomEvent.eventDescription.Localize()));
            }
        }

        // 선택지 버튼 생성
        SetupChoiceButtons(randomEvent);
    }

    /// <summary>
    /// 선택지 버튼 설정
    /// </summary>
    private void SetupChoiceButtons(RandomEvent randomEvent)
    {
        // 선택지 패널 확인
        if (choiceButtonPrefab == null)
            return;

        // 기존 버튼 제거
        ClearChoices();

        // 새 선택지 버튼 생성
        for (int i = 0; i < randomEvent.choices.Count; i++)
        {
            EventChoice choice = randomEvent.choices[i];
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceContainer.transform);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
                buttonText.text = choice.choiceText.Localize();

            int choiceIndex = i; // 인덱스를 클로저 내에서 사용하기 위해 복사
            button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
    }

    /// <summary>
    /// 선택지 클리어
    /// </summary>
    private void ClearChoices()
    {
        if (choiceContainer == null)
            return;

        foreach (Transform child in choiceContainer.transform)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// 선택지 선택 처리
    /// </summary>
    private void OnChoiceSelected(int choiceIndex)
    {
        StopAllCoroutines();

        eventDescriptionText.text = "";

        // 선택지 버튼 비활성화
        if (choiceContainer != null)
            foreach (Transform child in choiceContainer.transform)
                if (child.GetComponent<Button>() != null)
                    child.GetComponent<Button>().interactable = false;

        // EventManager로 선택 전달
        EventManager.Instance.ProcessChoice(currentEvent, choiceIndex);
    }

    /// <summary>
    /// 결과 표시
    /// </summary>
    public void ShowOutcome(string text)
    {
        if (choiceContainer != null)
            choiceContainer.SetActive(false);

        outcomeText.gameObject.SetActive(true);

        if (outcomeText != null)
            StartCoroutine(TypeOutcomeText(outcomeText, text));
    }

    /// <summary>
    /// 텍스트 타이핑 효과
    /// </summary>
    private IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = "";

        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textTypingSpeed);
        }
    }

    /// <summary>
    /// 결과 텍스트 타이핑 효과 - 완료 후 계속하기 버튼 표시
    /// </summary>
    private IEnumerator TypeOutcomeText(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = "";

        continueButton.gameObject.SetActive(true);

        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textTypingSpeed);
        }
    }

    /// <summary>
    /// 이벤트 패널 닫기
    /// </summary>
    public void CloseEventPanel()
    {
        StopAllCoroutines();

        outcomeText.text = "";
        outcomeText.gameObject.SetActive(false);
        choiceContainer.SetActive(true);
        gameObject.SetActive(false);
        EventManager.Instance.EndEvent(); // 이벤트 종료 처리

        // 필요한 경우 여기서 다른 게임 시스템을 다시 활성화

        // 예: GameManager.Instance.ResumeGame();
    }
}
