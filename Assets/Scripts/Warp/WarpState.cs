using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// 워프 과정의 상태를 관리하는 클래스
/// 수정된 WarpUIController와 통합
/// </summary>
public class WarpState : IGameState
{
    // 워프 과정의 단계를 정의하는 열거형
    private enum WarpPhase
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

    // 워프 과정 데이터
    private List<Vector2> selectedPath;
    private List<bool> pathDangerInfo;
    private EventNode selectedEventNode;

    // 워프 프로세스 제어 변수
    private bool waitingForUserInput;
    private float phaseTimer;

    // 상태 진입 시 호출
    public void Enter()
    {
        Debug.Log("Entering WarpState");

        if (UIManager.Instance != null)
        {
            uiController = UIManager.Instance.GetOrCreateWarpUIController();
        }
        else
        {
            Debug.LogError("UIManager not found!");
            //GameStateManager.Instance.ChangeState(new MainMenuState());
            return;
        }

        // UI 컨트롤러 이벤트 구독
        uiController.OnUIStateChanged += HandleUIStateChanged;
        uiController.OnContinueButtonClicked += HandleContinueButtonClicked;
        uiController.OnEventChoiceSelected += HandleEventChoiceSelected;
        uiController.OnEventNodeSelected += HandleEventNodeSelected;
        uiController.OnRoutePlanningCompleted += HandleRoutePlanningCompleted;
        uiController.OnPathDataSet += HandlePathDataSet;

        // UI 컨트롤러 초기화 및 표시
        uiController.Initialize();
        uiController.Show();

        // 워프 과정 초기화
        currentPhase = WarpPhase.Initializing;
        waitingForUserInput = false;
        selectedPath = new List<Vector2>();
        pathDangerInfo = new List<bool>();
        selectedEventNode = null;

        // 단계 전환
        StartNextPhase();
    }

    // 상태 업데이트 시 호출
    public void Update()
    {
        // 현재 단계에 따른 처리
        switch (currentPhase)
        {
            case WarpPhase.Initializing:
                // 초기화는 Enter 또는 StartNextPhase에서 이미 처리됨
                StartNextPhase(); // 바로 다음 단계로 진행
                break;

            case WarpPhase.PlanningRoute:
                // WarpUIController에서 사용자 입력을 처리하므로 여기서는 특별한 처리 필요 없음
                // 경로 계획이 완료되면 HandleRoutePlanningCompleted가 호출될 것임
                break;

            case WarpPhase.SelectingEvents:
                // WarpUIController에서 사용자 입력을 처리하므로 여기서는 특별한 처리 필요 없음
                // 이벤트 노드 선택 시 HandleEventNodeSelected가 호출될 것임
                break;

            case WarpPhase.ExecutingWarp:
                // 워프 애니메이션 진행
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0) StartNextPhase(); // 애니메이션 완료 후 다음 단계로
                break;

            case WarpPhase.ProcessingEvent:
                // 이벤트 처리
                // 이벤트가 완료되면 HandleEventChoiceSelected가 호출될 것임
                break;

            case WarpPhase.Arriving:
                // 도착 처리
                phaseTimer -= Time.deltaTime;
                if (phaseTimer <= 0) CompleteWarpProcess(); // 워프 과정 완료
                break;
        }
    }

    // 상태 종료 시 호출
    public void Exit()
    {
        Debug.Log("Exiting WarpState");

        // UI 컨트롤러 정리
        if (uiController != null)
        {
            // 이벤트 구독 해제
            uiController.OnUIStateChanged -= HandleUIStateChanged;
            uiController.OnContinueButtonClicked -= HandleContinueButtonClicked;
            uiController.OnEventChoiceSelected -= HandleEventChoiceSelected;
            uiController.OnEventNodeSelected -= HandleEventNodeSelected;
            uiController.OnRoutePlanningCompleted -= HandleRoutePlanningCompleted;
            uiController.OnPathDataSet -= HandlePathDataSet;

            // UI 숨기기
            uiController.Hide();
        }

        // 리소스 정리
        selectedPath = null;
        pathDangerInfo = null;
        selectedEventNode = null;
    }

    // 다음 단계로 전환
    private void StartNextPhase()
    {
        switch (currentPhase)
        {
            case WarpPhase.Initializing:
                // 경로 계획 단계로 전환
                currentPhase = WarpPhase.PlanningRoute;
                Debug.Log("WarpState: Starting Route Planning Phase");

                // UI 컨트롤러 상태 설정
                uiController.SetUIState(WarpUIController.WarpUIState.RoutePlanning);
                uiController.InitializeRoutePlanningUI(20, 100f); // 행성 수와 배치 거리 설정

                waitingForUserInput = true;
                break;

            case WarpPhase.PlanningRoute:
                // 이벤트 선택 단계로 전환
                currentPhase = WarpPhase.SelectingEvents;
                Debug.Log("WarpState: Starting Event Selection Phase");

                // UI 컨트롤러 상태 설정
                uiController.SetUIState(WarpUIController.WarpUIState.EventSelection);

                // 이벤트 트리 초기화 (경로 데이터는 이미 UI 컨트롤러에 설정되어 있음)
                uiController.InitializeEventSelectionUI(selectedPath, pathDangerInfo);

                waitingForUserInput = true;
                break;

            case WarpPhase.SelectingEvents:
                // 워프 실행 단계로 전환
                currentPhase = WarpPhase.ExecutingWarp;
                Debug.Log("WarpState: Starting Warp Execution Phase");

                // 워프 애니메이션 UI 표시
                uiController.SetUIState(WarpUIController.WarpUIState.WarpAnimation);

                // 타이머 설정 (애니메이션 지속 시간)
                phaseTimer = 2.0f; // 2초 동안 애니메이션 실행
                waitingForUserInput = false;
                break;

            case WarpPhase.ExecutingWarp:
                // 이벤트 처리 단계로 전환
                currentPhase = WarpPhase.ProcessingEvent;
                Debug.Log("WarpState: Starting Event Processing Phase");

                // 이벤트 처리 UI 표시
                uiController.SetUIState(WarpUIController.WarpUIState.EventProcessing);

                // 선택된 이벤트 노드에 따른 이벤트 처리 - 예시 데이터
                string title = "우주 방랑자 조우";
                string description = "당신은 우주를 여행하는 방랑자를 만났습니다. 그는 무언가 교환하고 싶어하는 것 같습니다.";
                string[] choices = new string[] { "그와 거래한다", "조심스럽게 거절한다", "곧바로 떠난다" };

                uiController.SetEventInfo(title, description, choices);

                waitingForUserInput = true;
                break;

            case WarpPhase.ProcessingEvent:
                // 도착 처리 단계로 전환
                currentPhase = WarpPhase.Arriving;
                Debug.Log("WarpState: Starting Arrival Phase");

                // 도착 UI 표시
                uiController.SetUIState(WarpUIController.WarpUIState.Arrival);
                uiController.SetArrivalPlanetInfo("행성-42B"); // 실제 도착 행성 이름으로 대체 필요

                // 타이머 설정 (도착 지연 시간)
                phaseTimer = 1.5f;
                waitingForUserInput = false;
                break;

            case WarpPhase.Arriving:
                // 이 단계 이후에는 CompleteWarpProcess()를 통해
                // 워프 상태에서 다른 상태로 전환하게 됨
                break;
        }
    }

    // 워프 과정 완료 처리
    private void CompleteWarpProcess()
    {
        Debug.Log("WarpState: Warp Process Completed");

        // 도착한 행성에서의 상태로 전환
        // GameStateManager.Instance.ChangeState(new PlanetExplorationState()); // 예시

        // 테스트용: 다시 워프 상태로 돌아가기
        GameStateManager.Instance.ChangeState(new WarpState());
    }

    #region 이벤트 핸들러

    // UI 상태 변경 이벤트 핸들러
    private void HandleUIStateChanged(WarpUIController.WarpUIState newState)
    {
        Debug.Log($"WarpUIController state changed to: {newState}");

        // UI 상태에 따른 워프 단계 동기화 필요시 여기서 처리
    }

    // 계속 버튼 클릭 이벤트 핸들러
    private void HandleContinueButtonClicked()
    {
        Debug.Log("Continue button clicked");

        // 현재 단계가 도착 단계라면 워프 과정 완료
        if (currentPhase == WarpPhase.Arriving) CompleteWarpProcess();
    }

    // 이벤트 선택지 선택 이벤트 핸들러
    private void HandleEventChoiceSelected(int choiceIndex)
    {
        Debug.Log($"Event choice selected: {choiceIndex}");

        // 이벤트 선택 처리 (실제로는 여기서 게임 상태에 영향을 주는 로직 구현)

        // 이벤트 처리 완료, 다음 단계로 진행
        waitingForUserInput = false;
        StartNextPhase();
    }

    // 이벤트 노드 선택 이벤트 핸들러
    private void HandleEventNodeSelected(EventNode node)
    {
        Debug.Log($"Event node selected: Layer {node.LevelIndex}, Index {node.NodeIndex}");

        // 선택된 이벤트 노드 저장
        selectedEventNode = node;

        // 이벤트 노드 선택 완료, 다음 단계로 진행
        if (currentPhase == WarpPhase.SelectingEvents) StartNextPhase();
    }

    // 경로 계획 완료 이벤트 핸들러
    private void HandleRoutePlanningCompleted()
    {
        Debug.Log("Route planning completed");

        // 경로 계획 완료, 다음 단계로 진행
        if (currentPhase == WarpPhase.PlanningRoute) StartNextPhase();
    }

    // 경로 데이터 설정 이벤트 핸들러
    private void HandlePathDataSet(List<Vector2> pathNodes, List<bool> dangerInfo)
    {
        Debug.Log($"Path data set: {pathNodes.Count} nodes, {dangerInfo.Count} danger info");

        // 경로 데이터 저장
        selectedPath = new List<Vector2>(pathNodes);
        pathDangerInfo = new List<bool>(dangerInfo);
    }

    #endregion
}
