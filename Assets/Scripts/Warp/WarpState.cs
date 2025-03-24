using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 워프 과정의 상태를 관리하는 클래스
/// 맵 시스템을 직접 제어하고 워프 프로세스의 전체 흐름을 관리함
/// </summary>
public class WarpState : IGameState
{
    // 워프 과정의 단계를 정의하는 열거형
    public enum WarpPhase
    {
        Initializing, // 초기화 단계
        PlanningRoute, // 경로 계획 단계
        SelectingEvents, // 이벤트 선택 단계
        ExecutingWarp, // 워프 애니메이션 실행 단계
        ProcessingEvent, // 이벤트 처리 단계
        Arriving // 도착 처리 단계
    }

    // 현재 단계
    private WarpPhase currentPhase;

    // 워프 UI 컨트롤러 참조
    private WarpUIController uiController;
    private NodePlacementMap nodePlacementMap;
    private EventTreeMap eventTreeMap;

    // 워프 과정 데이터
    private List<Vector2> selectedPath = new();
    private List<bool> pathDangerInfo = new();
    private EventNode selectedEventNode;

    // 각 단계별 상태 변수
    private bool routePlanningComplete = false;
    private bool pathNodeSelected = false;
    private bool eventNodeSelected = false;
    private bool eventChoiceSelected = false;
    private int selectedChoiceIndex = -1;

    // 워프 애니메이션 변수
    private float warpAnimationTime = 0f;
    private float warpAnimationDuration = 2.0f;

    // 도착 애니메이션 변수
    private float arrivalTime = 0f;
    private float arrivalDuration = 1.5f;

    // 맵 설정 값
    private int numberOfPlanets = 20;
    private float maxPlacementDistance = 100f;

    // 상태 진입 시 호출
    public void Enter()
    {
        // UI 컨트롤러 및 맵 컴포넌트 참조 얻기
        InitializeComponents();

        // 워프 과정 초기화
        currentPhase = WarpPhase.Initializing;
        selectedPath.Clear();
        pathDangerInfo.Clear();
        selectedEventNode = null;

        // 상태 변수 초기화
        routePlanningComplete = false;
        eventNodeSelected = false;
        eventChoiceSelected = false;
        selectedChoiceIndex = -1;
        warpAnimationTime = 0f;
        arrivalTime = 0f;
    }

    // 필요한 컴포넌트 초기화 및 이벤트 구독
    private void InitializeComponents()
    {
        // UI 컨트롤러 참조 얻기
        if (UIManager.Instance != null)
        {
            uiController = UIManager.Instance.GetOrCreateWarpUIController();

            // 맵 컴포넌트 참조 얻기
            nodePlacementMap = uiController.transform.GetComponentInChildren<NodePlacementMap>(true);
            eventTreeMap = uiController.transform.GetComponentInChildren<EventTreeMap>(true);

            if (nodePlacementMap == null || eventTreeMap == null)
            {
                Debug.LogError("Map components not found in WarpUIController!");
                // GameStateManager.Instance.ChangeState(new MainMenuState());
                return;
            }
        }
        else
        {
            Debug.LogError("UIManager not found!");
            // GameStateManager.Instance.ChangeState(new MainMenuState());
            return;
        }

        // UI 컨트롤러 이벤트 구독
        uiController.OnUIStateChanged += HandleUIStateChanged;
        uiController.OnContinueButtonClicked += HandleContinueButtonClicked;
        uiController.OnEventChoiceSelected += HandleEventChoiceSelected;

        // 맵 컴포넌트 이벤트 직접 구독
        nodePlacementMap.OnPathCompleted += HandlePathCompleted;
        eventTreeMap.OnNodeSelected += HandleEventNodeSelected;

        // UI 컨트롤러 초기화 및 표시
        uiController.Initialize();
        uiController.Show();
    }

    // 상태 업데이트 시 호출
    public void Update()
    {
        // 현재 단계에 따른 처리
        switch (currentPhase)
        {
            case WarpPhase.Initializing:
                // 초기화 로직 직접 실행
                TransitionToRoutePlanning();
                break;

            case WarpPhase.PlanningRoute:
                // 경로 계획 상태 능동적으로 확인
                UpdateRoutePlanningState();

                // 경로 계획이 완료되었는지 직접 확인
                if (IsRoutePlanningComplete()) TransitionToEventSelection();
                break;

            case WarpPhase.SelectingEvents:
                // 이벤트 선택 상태 능동적으로 확인
                UpdateEventSelectionState();

                // 이벤트 노드가 선택되었는지 직접 확인
                if (IsEventNodeSelected())
                {
                    bool success = GameManager.Instance.GetPlayerShip().Warp();

                    if (success)
                    {
                        Debug.Log("워프 성공!");

                        TransitionToWarpExecution();
                    }
                    else
                    {
                        Debug.Log("워프 실패...");
                    }
                }

                break;

            case WarpPhase.ExecutingWarp:
                // 워프 애니메이션 진행
                UpdateWarpAnimation();

                // 워프 애니메이션이 완료되었는지 확인
                if (IsWarpAnimationComplete()) TransitionToEventProcessing();
                break;

            case WarpPhase.ProcessingEvent:
                // 이벤트 처리 상태 업데이트
                UpdateEventProcessingState();

                // 이벤트 선택이 완료되었는지 확인
                if (IsEventChoiceSelected()) TransitionToArrival();
                break;

            case WarpPhase.Arriving:
                // 도착 처리 업데이트
                UpdateArrivalState();

                // 도착 단계가 완료되었는지 확인
                if (IsArrivalComplete()) CompleteWarpProcess();
                break;
        }
    }

    // 상태 종료 시 호출
    public void Exit()
    {
        Debug.Log("Exiting WarpState");

        // 이벤트 구독 해제
        UnsubscribeFromEvents();

        // UI 및 맵 정리
        CleanupComponents();

        // 리소스 정리
        selectedPath.Clear();
        pathDangerInfo.Clear();
        selectedEventNode = null;
    }

    // 이벤트 구독 해제
    private void UnsubscribeFromEvents()
    {
        if (uiController != null)
        {
            // UI 컨트롤러 이벤트 구독 해제
            uiController.OnUIStateChanged -= HandleUIStateChanged;
            uiController.OnContinueButtonClicked -= HandleContinueButtonClicked;
            uiController.OnEventChoiceSelected -= HandleEventChoiceSelected;
        }

        if (nodePlacementMap != null) nodePlacementMap.OnPathCompleted -= HandlePathCompleted;

        if (eventTreeMap != null) eventTreeMap.OnNodeSelected -= HandleEventNodeSelected;
    }

    // UI 및 맵 컴포넌트 정리
    private void CleanupComponents()
    {
        if (nodePlacementMap != null) nodePlacementMap.gameObject.SetActive(false);

        if (eventTreeMap != null) eventTreeMap.gameObject.SetActive(false);

        if (uiController != null) uiController.Hide();
    }

    #region 상태 전환 메서드

    // 경로 계획 단계로 전환
    private void TransitionToRoutePlanning()
    {
        currentPhase = WarpPhase.PlanningRoute;

        // 경로 계획 초기화 - 직접 맵 제어
        InitializeRoutePlanning();

        // 상태 변수 초기화
        routePlanningComplete = false;
        pathNodeSelected = false;
    }

    // 이벤트 선택 단계로 전환
    private void TransitionToEventSelection()
    {
        currentPhase = WarpPhase.SelectingEvents;

        // 이벤트 선택 초기화 - 직접 맵 제어
        InitializeEventSelection();

        // 상태 변수 초기화
        eventNodeSelected = false;
    }

    // 워프 실행 단계로 전환
    private void TransitionToWarpExecution()
    {
        currentPhase = WarpPhase.ExecutingWarp;

        // 워프 애니메이션 UI 표시 - 맵 비활성화
        InitializeWarpAnimation();

        // 워프 애니메이션 변수 초기화
        warpAnimationTime = 0f;
        warpAnimationDuration = 2.0f;
    }

    // 이벤트 처리 단계로 전환
    private void TransitionToEventProcessing()
    {
        currentPhase = WarpPhase.ProcessingEvent;

        // 이벤트 처리 UI 표시
        InitializeEventProcessing();

        // 상태 변수 초기화
        eventChoiceSelected = false;
    }

    // 도착 단계로 전환
    private void TransitionToArrival()
    {
        currentPhase = WarpPhase.Arriving;

        // 도착 UI 표시
        InitializeArrival();

        // 도착 변수 초기화
        arrivalTime = 0f;
        arrivalDuration = 1.5f;
    }

    #endregion

    #region 상태 확인 메서드

    // 경로 계획이 완료되었는지 확인
    private bool IsRoutePlanningComplete()
    {
        // 이미 완료 플래그가 설정되었으면 true 반환
        if (routePlanningComplete)
            return true;

        // NodePlacementMap에서 경로가 완성되었는지 직접 확인
        if (selectedPath.Count >= 2)
        {
            routePlanningComplete = true;
            return true;
        }

        return false;
    }

    // 이벤트 노드가 선택되었는지 확인
    private bool IsEventNodeSelected()
    {
        // 이미 선택 플래그가 설정되었으면 true 반환
        if (eventNodeSelected)
            return true;

        // selectedEventNode 변수가 설정되었는지 확인
        if (selectedEventNode != null)
        {
            eventNodeSelected = true;
            return true;
        }

        return false;
    }

    // 워프 애니메이션이 완료되었는지 확인
    private bool IsWarpAnimationComplete()
    {
        return warpAnimationTime >= warpAnimationDuration;
    }

    // 이벤트 선택이 완료되었는지 확인
    private bool IsEventChoiceSelected()
    {
        return eventChoiceSelected;
    }

    // 도착 단계가 완료되었는지 확인
    private bool IsArrivalComplete()
    {
        return arrivalTime >= arrivalDuration;
    }

    #endregion

    #region 상태 업데이트 메서드

    // 경로 계획 상태 업데이트
    private void UpdateRoutePlanningState()
    {
        // 필요한 경우 경로 계획 관련 UI나 안내 업데이트
        // NodePlacementMap은 자체적으로 사용자 입력을 처리함
    }

    // 이벤트 선택 상태 업데이트
    private void UpdateEventSelectionState()
    {
        // 필요한 경우 이벤트 선택 관련 UI나 안내 업데이트
        // EventTreeMap은 자체적으로 사용자 입력을 처리함
    }

    // 워프 애니메이션 업데이트
    private void UpdateWarpAnimation()
    {
        // 워프 애니메이션 타이머 업데이트
        warpAnimationTime += Time.deltaTime;

        // UI 애니메이션 업데이트 (필요한 경우)
        float progress = Mathf.Clamp01(warpAnimationTime / warpAnimationDuration);
        // 예: uiController.UpdateWarpAnimationProgress(progress);
    }

    // 이벤트 처리 상태 업데이트
    private void UpdateEventProcessingState()
    {
        // 필요한 경우 이벤트 처리 관련 UI나 안내 업데이트
    }

    // 도착 상태 업데이트
    private void UpdateArrivalState()
    {
        // 도착 애니메이션 타이머 업데이트
        arrivalTime += Time.deltaTime;

        // UI 애니메이션 업데이트 (필요한 경우)
        float progress = Mathf.Clamp01(arrivalTime / arrivalDuration);
        // 예: uiController.UpdateArrivalProgress(progress);
    }

    #endregion

    #region 단계별 초기화 메서드

    // 경로 계획 단계 초기화
    private void InitializeRoutePlanning()
    {
        // UI 상태 설정
        uiController.SetUIState(WarpUIController.WarpUIState.RoutePlanning);

        // EventTreeMap 비활성화
        if (eventTreeMap != null) eventTreeMap.gameObject.SetActive(false);

        // NodePlacementMap 초기화 및 활성화
        if (nodePlacementMap != null)
        {
            nodePlacementMap.gameObject.SetActive(true);
            nodePlacementMap.Initialize();
            nodePlacementMap.InitializeMap(numberOfPlanets, maxPlacementDistance);
        }
        else
        {
            Debug.LogError("NodePlacementMap is null in InitializeRoutePlanning!");
        }
    }

    // 이벤트 선택 단계 초기화
    private void InitializeEventSelection()
    {
        // UI 상태 설정
        uiController.SetUIState(WarpUIController.WarpUIState.EventSelection);

        // NodePlacementMap 비활성화
        if (nodePlacementMap != null) nodePlacementMap.gameObject.SetActive(false);

        // EventTreeMap 초기화 및 활성화
        if (eventTreeMap != null)
        {
            eventTreeMap.gameObject.SetActive(true);
            eventTreeMap.Initialize();
            eventTreeMap.GenerateTreeFromPath(selectedPath, pathDangerInfo);
        }
        else
        {
            Debug.LogError("EventTreeMap is null in InitializeEventSelection!");
        }
    }

    // 워프 애니메이션 초기화
    private void InitializeWarpAnimation()
    {
        // 모든 맵 비활성화
        if (nodePlacementMap != null) nodePlacementMap.gameObject.SetActive(false);

        if (eventTreeMap != null) eventTreeMap.gameObject.SetActive(false);

        // 워프 애니메이션 UI 표시
        uiController.SetUIState(WarpUIController.WarpUIState.WarpAnimation);
    }

    // 이벤트 처리 초기화
    private void InitializeEventProcessing()
    {
        // 이벤트 처리 UI 표시
        uiController.SetUIState(WarpUIController.WarpUIState.EventProcessing);

        // 선택된 이벤트 노드에 따른 이벤트 처리 - 예시 데이터
        // 실제로는 selectedEventNode의 데이터를 기반으로 동적 생성
        string title = "우주 방랑자 조우";
        string description = "당신은 우주를 여행하는 방랑자를 만났습니다. 그는 무언가 교환하고 싶어하는 것 같습니다.";
        string[] choices = new string[] { "그와 거래한다", "조심스럽게 거절한다", "곧바로 떠난다" };

        uiController.SetEventInfo(title, description, choices);
    }

    // 도착 단계 초기화
    private void InitializeArrival()
    {
        // 도착 UI 표시
        uiController.SetUIState(WarpUIController.WarpUIState.Arrival);

        // 도착한 행성 정보 설정 - 실제로는 선택된 경로의 마지막 노드 정보 사용
        string planetName = "행성-42B";
        if (selectedPath.Count > 0)
        {
            // 여기서는 마지막 위치 기반으로 이름 생성 (실제 구현에서는 행성 데이터 사용)
            Vector2 arrivalPosition = selectedPath[selectedPath.Count - 1];
            planetName = $"행성-{Mathf.RoundToInt(arrivalPosition.x)}{Mathf.RoundToInt(arrivalPosition.y)}";
        }

        uiController.SetArrivalPlanetInfo(planetName);
    }

    #endregion

    // 워프 과정 완료 처리
    private void CompleteWarpProcess()
    {
        // 도착한 행성에서의 상태로 전환
        // GameStateManager.Instance.ChangeState(new PlanetExplorationState()); // 예시

        // 테스트용: 다시 워프 상태로 돌아가기
        GameStateManager.Instance.ChangeState(new WarpState());
    }

    #region 이벤트 핸들러

    // UI 상태 변경 이벤트 핸들러
    private void HandleUIStateChanged(WarpUIController.WarpUIState newState)
    {
        // UI 상태에 따른 워프 단계 동기화 필요시 여기서 처리
    }

    // 계속 버튼 클릭 이벤트 핸들러
    private void HandleContinueButtonClicked()
    {
        // 현재 단계가 도착 단계라면 도착 처리 완료로 마킹
        if (currentPhase == WarpPhase.Arriving) arrivalTime = arrivalDuration; // 강제로 도착 시간 완료 처리
    }

    // 이벤트 선택지 선택 이벤트 핸들러
    private void HandleEventChoiceSelected(int choiceIndex)
    {
        // 이벤트 선택 처리 (실제로는 여기서 게임 상태에 영향을 주는 로직 구현)
        selectedChoiceIndex = choiceIndex;
        eventChoiceSelected = true;

        // 여기서는 상태 전환을 직접 호출하지 않고 Update에서 판단
    }

    // 이벤트 노드 선택 이벤트 핸들러
    private void HandleEventNodeSelected(EventNode node)
    {
        // 선택된 이벤트 노드 저장
        selectedEventNode = node;
        eventNodeSelected = true;

        // 여기서는 상태 전환을 직접 호출하지 않고 Update에서 판단
    }

    // 경로 계획 완료 이벤트 핸들러
    private void HandlePathCompleted(List<Vector2> pathNodes, List<bool> dangerInfo)
    {
        // 경로 데이터 저장
        selectedPath = new List<Vector2>(pathNodes);
        pathDangerInfo = new List<bool>(dangerInfo);
        routePlanningComplete = true;

        // 여기서는 상태 전환을 직접 호출하지 않고 Update에서 판단
    }

    #endregion
}
