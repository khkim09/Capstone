﻿using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUIManager : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private GameObject eventPanel;

    [SerializeField] private TextMeshProUGUI eventTitleText;
    [SerializeField] private TextMeshProUGUI eventDescriptionText;
    [SerializeField] private Image eventImage;

    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private GameObject choiceButtonPrefab;

    [SerializeField] private GameObject outcomePanel;
    [SerializeField] private TextMeshProUGUI outcomeText;
    [SerializeField] private Button continueButton;

    [Header("Settings")] [SerializeField] private float textTypingSpeed = 0.05f;

    private RandomEvent currentEvent;
    public static EventUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        eventPanel.SetActive(false);
        outcomePanel.SetActive(false);

        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    public void ShowEvent(RandomEvent randomEvent)
    {
        EventManager.Instance.EventPanelController.ShowEvent(randomEvent);
        // UIManager.Instance.ShowEvent(randomEvent);
        return;
        currentEvent = randomEvent;

        // 이전 타이핑 코루틴 강제 종료
        StopAllCoroutines();

        // UI초기화
        eventTitleText.text = "";
        eventDescriptionText.text = "";
        outcomeText.text = "";

        foreach (Transform child in choicesPanel.transform)
            Destroy(child.gameObject); // 선택지 클리어

        // 이벤트 패널 초기화
        outcomePanel.SetActive(false);
        eventPanel.SetActive(true);
        choicesPanel.SetActive(true);

        // 이벤트 정보 설정
        eventTitleText.text = randomEvent.eventTitle.Localize();
        StartCoroutine(TypeText(eventDescriptionText, randomEvent.eventDescription.Localize()));

        if (randomEvent.eventImage != null)
        {
            eventImage.gameObject.SetActive(true);
            eventImage.sprite = randomEvent.eventImage;
        }
        else
        {
            eventImage.gameObject.SetActive(false);
        }

        // 선택지 버튼 생성
        SetupChoiceButtons(randomEvent);
    }

    private void SetupChoiceButtons(RandomEvent randomEvent)
    {
        // 기존 버튼 제거
        foreach (Transform child in choicesPanel.transform) Destroy(child.gameObject);

        // 새 선택지 버튼 생성
        for (int i = 0; i < randomEvent.choices.Count; i++)
        {
            EventChoice choice = randomEvent.choices[i];
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            buttonText.text = choice.choiceText.Localize();

            int choiceIndex = i; // 인덱스를 클로저 내에서 사용하기 위해 복사
            button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
    }

    private void OnChoiceSelected(int choiceIndex)
    {
        // 선택지 버튼 비활성화
        foreach (Transform child in choicesPanel.transform) child.GetComponent<Button>().interactable = false;

        // 이벤트 패널 숨김
        eventPanel.SetActive(false);

        // 선택 처리
        EventManager.Instance.ProcessChoice(currentEvent, choiceIndex);
    }

    public void ShowOutcome(string text)
    {
        choicesPanel.SetActive(false);
        outcomePanel.SetActive(true);

        StartCoroutine(TypeText(outcomeText, text));
    }

    private IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = "";

        foreach (char c in text)
        {
            textComponent.text += c;

            yield return new WaitForSeconds(textTypingSpeed);
        }
    }

    private void OnContinueButtonClicked()
    {
        outcomePanel.SetActive(false);
        EventManager.Instance.EndEvent(); // 이벤트 종료 처리

        // 게임 상태 업데이트
        GameManager.Instance.OnEventCompleted();
    }
}
