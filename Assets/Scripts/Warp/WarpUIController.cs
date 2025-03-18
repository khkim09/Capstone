using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 워프 과정의 모든 UI 요소를 관리하는 컨트롤러
/// 맵 컴포넌트들의 조정자 역할 수행
/// </summary>
public class WarpUIController : MonoBehaviour, IUIController
{
    #region 패널 및 주요 UI 참조

    [Header("UI 패널")] [SerializeField] private GameObject routePlanningPanel; // 경로 계획 패널
    [SerializeField] private GameObject eventSelectionPanel; // 이벤트 선택 패널
    [SerializeField] private GameObject warpAnimationPanel; // 워프 애니메이션 패널
    [SerializeField] private GameObject eventProcessingPanel; // 이벤트 처리 패널
    [SerializeField] private GameObject arrivalPanel; // 도착 패널

    [Header("맵 컴포넌트 참조")] [SerializeField] private NodePlacementMap nodePlacementMap; // 노드 배치 맵 컴포넌트
    [SerializeField] private EventTreeMap eventTreeMap; // 이벤트 트리 맵 컴포넌트

    [Header("워프 애니메이션 요소")] [SerializeField]
    private Image warpEffectImage; // 워프 이펙트 이미지

    [SerializeField] private ParticleSystem warpParticleSystem; // 워프 파티클 시스템
    [SerializeField] private AudioSource warpAudioSource; // 워프 오디오 소스

    [Header("이벤트 UI 요소")] [SerializeField] private Text eventTitleText; // 이벤트 제목 텍스트
    [SerializeField] private Text eventDescriptionText; // 이벤트 설명 텍스트
    [SerializeField] private Button[] eventChoiceButtons; // 이벤트 선택 버튼들

    [Header("도착 UI 요소")] [SerializeField] private Text arrivalPlanetNameText; // 도착 행성 이름
    [SerializeField] private Button continueButton; // 계속 버튼

    #endregion

    #region 상태 및 이벤트

    // 상태 정의
    public enum WarpUIState
    {
        Hidden,
        RoutePlanning,
        EventSelection,
        WarpAnimation,
        EventProcessing,
        Arrival
    }

    // 현재 UI 상태
    private WarpUIState currentUIState = WarpUIState.Hidden;

    // 경로 데이터
    private List<Vector2> pathNodes = new();
    private List<bool> pathDangerInfo = new();

    // 이벤트 정의
    public event Action<WarpUIState> OnUIStateChanged;
    public event Action OnContinueButtonClicked;
    public event Action<int> OnEventChoiceSelected;
    public event Action<EventNode> OnEventNodeSelected;
    public event Action OnRoutePlanningCompleted;
    public event Action<List<Vector2>, List<bool>> OnPathDataSet;

    #endregion

    #region 초기화 및 이벤트 설정

    private void Awake()
    {
        InitializeComponents();
        RegisterEventListeners();
    }

    private void InitializeComponents()
    {
        // 컴포넌트 참조 찾기 (Inspector에서 할당되지 않은 경우)
        if (nodePlacementMap == null)
            nodePlacementMap = GetComponentInChildren<NodePlacementMap>(true);

        if (eventTreeMap == null)
            eventTreeMap = GetComponentInChildren<EventTreeMap>(true);

//if (continueButton == null)
//            continueButton = arrivalPanel?.GetComponentInChildren<Button>();

        // UI 패널 초기 상태 설정
        HideAllPanels();
    }

    private void RegisterEventListeners()
    {
        // 계속 버튼 이벤트 등록
        if (continueButton != null)
            continueButton.onClick.AddListener(() => OnContinueButtonClicked?.Invoke());

        // 이벤트 선택 버튼 이벤트 등록
        for (int i = 0; i < eventChoiceButtons.Length; i++)
        {
            int choiceIndex = i; // 클로저 문제 방지를 위한 로컬 변수
            if (eventChoiceButtons[i] != null)
                eventChoiceButtons[i].onClick.AddListener(() => OnEventChoiceSelected?.Invoke(choiceIndex));
        }

        // 맵 컴포넌트 이벤트 등록
        if (nodePlacementMap != null)
            // NodePlacementMap 경로 완료 이벤트 구독
            nodePlacementMap.OnPathCompleted += HandlePathCompleted;

        if (eventTreeMap != null)
            // EventTreeMap 노드 선택 이벤트 구독
            eventTreeMap.OnNodeSelected += HandleEventNodeSelected;
    }

    private void OnDestroy()
    {
        // 이벤트 핸들러 해제
        UnregisterEventListeners();
    }

    private void UnregisterEventListeners()
    {
        if (nodePlacementMap != null)
            nodePlacementMap.OnPathCompleted -= HandlePathCompleted;

        if (eventTreeMap != null)
            eventTreeMap.OnNodeSelected -= HandleEventNodeSelected;

        // 버튼 이벤트 해제
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();

        for (int i = 0; i < eventChoiceButtons.Length; i++)
            if (eventChoiceButtons[i] != null)
                eventChoiceButtons[i].onClick.RemoveAllListeners();
    }

    #endregion

    #region IUIController 인터페이스 구현

    public void Initialize()
    {
        // 초기화 로직
        HideAllPanels();

        // 맵 컴포넌트 초기화
        if (nodePlacementMap != null)
            nodePlacementMap.Initialize();

        if (eventTreeMap != null)
            eventTreeMap.Initialize();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetUIState(WarpUIState.RoutePlanning); // 기본적으로 경로 계획 상태로 시작
    }

    public void Hide()
    {
        SetUIState(WarpUIState.Hidden);
        gameObject.SetActive(false);
    }

    public void Update()
    {
        // 상태에 따른 업데이트 로직
        switch (currentUIState)
        {
            case WarpUIState.WarpAnimation:
                UpdateWarpAnimation();
                break;

            case WarpUIState.Arrival:
                UpdateArrivalUI();
                break;
        }
    }

    #endregion

    #region UI 상태 관리

    /// <summary>
    /// UI 상태 설정하는 메소드
    /// </summary>
    public void SetUIState(WarpUIState state)
    {
        if (currentUIState == state) return;

        // 이전 상태 정리
        CleanupCurrentState();

        // 새 상태 설정
        currentUIState = state;

        // 패널 활성화/비활성화
        HideAllPanels();

        switch (state)
        {
            case WarpUIState.RoutePlanning:
                routePlanningPanel?.SetActive(true);
                if (nodePlacementMap != null)
                    nodePlacementMap.gameObject.SetActive(true);
                break;

            case WarpUIState.EventSelection:
                eventSelectionPanel?.SetActive(true);
                if (eventTreeMap != null)
                    eventTreeMap.gameObject.SetActive(true);
                break;

            case WarpUIState.WarpAnimation:
                warpAnimationPanel?.SetActive(true);
                StartWarpAnimation();
                break;

            case WarpUIState.EventProcessing:
                eventProcessingPanel?.SetActive(true);
                break;

            case WarpUIState.Arrival:
                arrivalPanel?.SetActive(true);
                StartArrivalSequence();
                break;
        }

        // 이벤트 발생
        OnUIStateChanged?.Invoke(state);
    }

    private void CleanupCurrentState()
    {
        switch (currentUIState)
        {
            case WarpUIState.RoutePlanning:
                if (nodePlacementMap != null)
                    nodePlacementMap.gameObject.SetActive(false);
                break;

            case WarpUIState.EventSelection:
                if (eventTreeMap != null)
                    eventTreeMap.gameObject.SetActive(false);
                break;

            case WarpUIState.WarpAnimation:
                StopWarpAnimation();
                break;

            case WarpUIState.Arrival:
                StopArrivalSequence();
                break;
        }
    }

    private void HideAllPanels()
    {
        //routePlanningPanel?.SetActive(false);
        //eventSelectionPanel?.SetActive(false);
        // warpAnimationPanel?.SetActive(false);
        //eventProcessingPanel?.SetActive(false);
        //arrivalPanel?.SetActive(false);

        // 맵 컴포넌트 비활성화
        if (nodePlacementMap != null)
            nodePlacementMap.gameObject.SetActive(false);

        if (eventTreeMap != null)
            eventTreeMap.gameObject.SetActive(false);
    }

    #endregion

    #region 워프 애니메이션 관련 메소드

    private void StartWarpAnimation()
    {
        if (warpEffectImage != null)
        {
            // 애니메이션 초기화
            warpEffectImage.fillAmount = 0f;
            warpEffectImage.gameObject.SetActive(true);
        }

        if (warpParticleSystem != null)
        {
            warpParticleSystem.Clear();
            warpParticleSystem.Play();
        }

        if (warpAudioSource != null)
            warpAudioSource.Play();
    }

    private void UpdateWarpAnimation()
    {
        if (warpEffectImage != null)
            // 워프 이미지 애니메이션 업데이트
            warpEffectImage.fillAmount = Mathf.MoveTowards(warpEffectImage.fillAmount, 1f, Time.deltaTime * 0.5f);
    }

    private void StopWarpAnimation()
    {
        if (warpParticleSystem != null)
            warpParticleSystem.Stop();

        if (warpAudioSource != null && warpAudioSource.isPlaying)
            warpAudioSource.Stop();
    }

    #endregion

    #region 도착 시퀀스 관련 메소드

    private void StartArrivalSequence()
    {
        // 도착 효과음 재생 등의 로직
    }

    private void UpdateArrivalUI()
    {
        // 도착 UI 업데이트 로직
    }

    private void StopArrivalSequence()
    {
        // 도착 시퀀스 정리 로직
    }

    /// <summary>
    /// 도착 행성 정보 설정 메소드
    /// </summary>
    public void SetArrivalPlanetInfo(string planetName)
    {
        if (arrivalPlanetNameText != null)
        {
            arrivalPlanetNameText.text = planetName;
            Debug.Log($"Arrival planet set to: {planetName}");
        }
        else
        {
            Debug.LogWarning("Cannot set arrival planet name: arrivalPlanetNameText is null");
        }
    }

    #endregion

    #region 이벤트 UI 관련 메소드

    /// <summary>
    /// 이벤트 정보를 UI에 표시하는 메소드
    /// </summary>
    public void SetEventInfo(string title, string description, string[] choices)
    {
        if (eventTitleText != null)
            eventTitleText.text = title;

        if (eventDescriptionText != null)
            eventDescriptionText.text = description;

        // 선택지 버튼 설정
        for (int i = 0; i < eventChoiceButtons.Length; i++)
            if (i < choices.Length)
            {
                eventChoiceButtons[i].gameObject.SetActive(true);
                Text buttonText = eventChoiceButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                    buttonText.text = choices[i];
            }
            else
            {
                eventChoiceButtons[i].gameObject.SetActive(false);
            }
    }

    #endregion

    #region 맵 초기화 및 데이터 관리

    /// <summary>
    /// 경로 계획 UI 초기화 메소드
    /// </summary>
    public void InitializeRoutePlanningUI(int numberOfPlanets, float maxPlacementDistance)
    {
        if (nodePlacementMap == null)
        {
            Debug.LogError("NodePlacementMap component is missing!");
            return;
        }

        // 노드 배치 맵 초기화를 위임
        nodePlacementMap.InitializeMap(numberOfPlanets, maxPlacementDistance);
    }

    /// <summary>
    /// 이벤트 선택 UI 초기화 메소드
    /// </summary>
    public void InitializeEventSelectionUI(List<Vector2> pathNodes, List<bool> dangerInfo)
    {
        if (eventTreeMap == null)
        {
            Debug.LogError("EventTreeMap component is missing!");
            return;
        }

        // 계층 구조 확인
        Transform parent = eventTreeMap.transform.parent;
        string parentPath = "";
        while (parent != null)
        {
            parentPath = parent.name + "/" + parentPath;
            parent = parent.parent;
        }

        Debug.Log($"EventTreeMap 계층 구조: {parentPath}{eventTreeMap.name}");


        // 경로 데이터 저장
        SetPathData(pathNodes, dangerInfo);

        // 이벤트 트리 맵 초기화를 위임
        eventTreeMap.GenerateTreeFromPath(pathNodes, dangerInfo);
    }

    /// <summary>
    /// 경로 데이터 설정 메서드
    /// </summary>
    public void SetPathData(List<Vector2> nodes, List<bool> dangerInfo)
    {
        pathNodes = new List<Vector2>(nodes);
        pathDangerInfo = new List<bool>(dangerInfo);

        // 데이터 설정 후 이벤트 발생
        OnPathDataSet?.Invoke(pathNodes, pathDangerInfo);
    }

    /// <summary>
    /// 현재 경로 데이터 가져오기
    /// </summary>
    public List<Vector2> GetPathNodes()
    {
        return new List<Vector2>(pathNodes);
    }

    /// <summary>
    /// 현재 위험 정보 데이터 가져오기
    /// </summary>
    public List<bool> GetPathDangerInfo()
    {
        return new List<bool>(pathDangerInfo);
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// NodePlacementMap에서 경로 완료 이벤트 처리
    /// </summary>
    private void HandlePathCompleted(List<Vector2> completedPath, List<bool> dangers)
    {
        Debug.Log("Path planning completed");

        // 경로 데이터 저장
        SetPathData(completedPath, dangers);

        // 이벤트 발생
        OnRoutePlanningCompleted?.Invoke();
    }

    /// <summary>
    /// EventTreeMap에서 노드 선택 이벤트 처리
    /// </summary>
    private void HandleEventNodeSelected(EventNode selectedNode)
    {
        Debug.Log($"Event node selected: {selectedNode.name}");

        // 이벤트 발생
        OnEventNodeSelected?.Invoke(selectedNode);
    }

    #endregion
}
