using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class MapPanelController : MonoBehaviour
{
    [Header("탭 버튼")] [SerializeField] private Button buttonWarp;
    [SerializeField] private Button buttonWorld;

    [Header("전체 탭")] [SerializeField] private GameObject Tabs;

    [Header("패널 목록")] [SerializeField] private GameObject panelWarp;
    [SerializeField] private GameObject panelWorld;
    [SerializeField] private GameObject panelUnselected;

    [Header("워프 패널 설정")] [SerializeField] private GameObject warpPanelContent;

    [Header("월드 패널 설정")] [SerializeField] private GameObject worldPanelContent;
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject currentPlayerIndicatorPrefab;
    [SerializeField] private GameObject validRangeIndicatorPrefab;
    [SerializeField] private GameObject worldNodePrefab;
    private Vector2 currentWorldNodePosition;


    [Header("열려야 되는 위치")] [SerializeField] private Transform openedPosition;
    [Header("열리는 속도")] [SerializeField] private float slideSpeed = 0.1f;

    private Vector3 closedPosition;
    private Coroutine slideCoroutine;
    private bool isOpen = false;
    private GameObject currentPanel = null;

    /// <summary>
    /// 월드 맵이 초기화 되었는지 여부
    /// </summary>
    private bool isMapInitialized = false;

    private Vector2 playerPositionBefore = new();

    private bool IsPlayerMoved => GameManager.Instance.normalizedPlayerPosition != playerPositionBefore;


    private void Start()
    {
        closedPosition = Tabs.transform.position; // 시작 위치 저장
        AddButtonListeners();
    }

    private void Update()
    {
        if (isOpen && Input.GetMouseButtonDown(0))
            // 클릭된 UI 요소가 현재 패널이 아닌지 체크
            if (!IsClickingOnSelf())
                SlideClose();
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            Debug.Log("게임 매니저가 NULL");
            return;
        }

        currentWorldNodePosition = GameManager.Instance.normalizedPlayerPosition;
        DrawWorldMap();
    }

    private void AddButtonListeners()
    {
        buttonWarp.onClick.AddListener(() => OnPanelButtonClicked(panelWarp));
        buttonWorld.onClick.AddListener(() => OnPanelButtonClicked(panelWorld));
    }

    private void OnPanelButtonClicked(GameObject targetPanel)
    {
        if (currentPanel == targetPanel && isOpen)
        {
            SlideClose();
            return;
        }

        if (targetPanel == panelWarp) InitializeWarpPanel();
        if (targetPanel == panelWorld) InitializeWorldPanel();


        ShowOnly(targetPanel);
        SlideOpen();
    }

    private bool IsClickingOnSelf()
    {
        PointerEventData pointerData = new(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
            // 클릭된 오브젝트가 현재 패널이거나 그 자식인지 확인
            if (result.gameObject.transform.IsChildOf(transform) || result.gameObject == gameObject)
                return true;
        return false;
    }

    private void SlideOpen()
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideToPosition(openedPosition.position));
        isOpen = true;
    }

    public void SlideClose()
    {
        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideToPosition(closedPosition));
        panelUnselected.SetActive(true);
        currentPanel?.SetActive(false);
        currentPanel = null;
        isOpen = false;
    }

    private IEnumerator SlideToPosition(Vector3 targetPosition)
    {
        float elapsed = 0f;
        Vector3 start = Tabs.transform.position;

        while (elapsed < slideSpeed)
        {
            Tabs.transform.position = Vector3.Lerp(start, targetPosition, elapsed / slideSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Tabs.transform.position = targetPosition;
    }

    private void ShowOnly(GameObject panelToShow)
    {
        panelUnselected.SetActive(false);
        panelWarp.SetActive(false);
        panelWorld.SetActive(false);
        panelToShow.SetActive(true);
        currentPanel = panelToShow;
    }

    #region 워프 패널 설정

    private void InitializeWarpPanel()
    {
        if (GameManager.Instance.CurrentState != GameState.Warp)
        {
        }
    }

    #endregion

    #region 월드 패널 설정

    private void InitializeWorldPanel()
    {
        // 월드 패널 컨텐츠에 클릭 이벤트 추가
        EventTrigger trigger = worldPanelContent.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = worldPanelContent.AddComponent<EventTrigger>();

        // 기존 트리거 제거하여 중복 방지
        trigger.triggers.Clear();

        EventTrigger.Entry entry = new();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { OnWorldMapClicked((PointerEventData)data); });
        trigger.triggers.Add(entry);
    }

    private void OnWorldMapClicked(PointerEventData eventData)
    {
        if (GameManager.Instance.CurrentState == GameState.Warp)
            return;

        // 클릭한 위치를 월드 패널 내 로컬 좌표로 변환
        RectTransform contentRect = worldPanelContent.GetComponent<RectTransform>();
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                contentRect, eventData.position, eventData.pressEventCamera, out localPoint))
            return; // 패널 밖 클릭은 무시

        if (localPoint == new Vector2(0, 0))
            return;

        // 컨텐츠 크기 비율에 맞게 좌표 보정
        float mapWidth = contentRect.rect.width;
        float mapHeight = contentRect.rect.height;
        float mapMin = Mathf.Min(mapWidth, mapHeight);

        // 정규화된 좌표 계산 (가로/세로 비율 고려)
        Vector2 normalizedPosition = new(
            Mathf.Clamp01((localPoint.x + mapWidth / 2) / mapWidth),
            Mathf.Clamp01((localPoint.y + mapHeight / 2) / mapHeight)
        );

        // 종횡비를 고려한 위치 계산
        Vector2 aspectAdjustedPosition = new(
            normalizedPosition.x * (mapWidth / mapMin),
            normalizedPosition.y * (mapHeight / mapMin)
        );

        Vector2 aspectAdjustedNodePosition = new(
            currentWorldNodePosition.x * (mapWidth / mapMin),
            currentWorldNodePosition.y * (mapHeight / mapMin)
        );

        // 종횡비가 반영된 거리 계산
        float distance = Vector2.Distance(aspectAdjustedPosition, aspectAdjustedNodePosition);
        float validRadius = Constants.Planets.PlanetNodeValidRadius / 2;

        if (distance <= validRadius)
        {
            // 클릭한 위치에 새 월드 노드 생성
            WorldNodeData newNodeData = new()
            {
                normalizedPosition = normalizedPosition, isVisited = false, isCurrentNode = true
            };

            // 기존 현재 노드 상태 해제
            foreach (WorldNodeData nodeData in GameManager.Instance.WorldNodeDataList) nodeData.isCurrentNode = false;

            // 새 노드를 게임 매니저 리스트에 추가
            GameManager.Instance.WorldNodeDataList.Add(newNodeData);


            // 현재 노드 위치 업데이트
            SetCurrentNodePosition(normalizedPosition);

            // 기존 노드와 표시기 제거
            ClearWorldNodesAndIndicator();

            // 노드와 표시기 다시 그리기
            DrawWorldNodesAndIndicator();
        }
    }

    private void ClearWorldNodesAndIndicator()
    {
        foreach (Transform child in worldPanelContent.transform)
        {
            // 월드 노드와 유효 범위 표시기만 제거
            WorldNode worldNode = child.GetComponent<WorldNode>();
            if (worldNode != null || child.name.Contains("Valid Range Indicator")) Destroy(child.gameObject);
        }
    }

    private void DrawWorldNodesAndIndicator()
    {
        RectTransform contentRect = worldPanelContent.GetComponent<RectTransform>();

        // 월드 노드 그리기
        List<WorldNodeData> worldNodeDatas = GameManager.Instance.WorldNodeDataList;

        foreach (WorldNodeData worldNode in worldNodeDatas)
        {
            WorldNode worldNodeInstance =
                Instantiate(worldNodePrefab, worldPanelContent.transform).GetComponent<WorldNode>();
            worldNodeInstance.worldNodeData = worldNode;

            if (worldNode.isCurrentNode)
                currentWorldNodePosition = worldNode.normalizedPosition;

            RectTransform worldNoderect = worldNodeInstance.GetComponent<RectTransform>();
            SetupMapObject(
                worldNoderect,
                worldNode.normalizedPosition,
                Constants.Planets.PlanetNodeSize,
                contentRect
            );
        }

        if (currentWorldNodePosition == new Vector2(0, 0))
            currentWorldNodePosition = GameManager.Instance.normalizedPlayerPosition;

        // 유효 범위 표시기 그리기
        GameObject validRangeIndicator = Instantiate(validRangeIndicatorPrefab, worldPanelContent.transform);
        validRangeIndicator.name = "Valid Range Indicator";
        RectTransform validRangeIndicatorRect = validRangeIndicator.GetComponent<RectTransform>();
        SetupMapObject(
            validRangeIndicatorRect,
            currentWorldNodePosition,
            Constants.Planets.PlanetNodeValidRadius,
            contentRect
        );
    }

    private void DrawWorldMap()
    {
        // 이미 초기화되었다면 건너뛰기
        if (isMapInitialized && !IsPlayerMoved)
            return;

        // TODO : 테스트용 키 삭제코드.
        //GameManager.Instance.DeleteWorldMap();
        GameManager.Instance.LoadWorldMap();

        List<PlanetData> planetDatas = GameManager.Instance.PlanetDataList;
        RectTransform contentRect = worldPanelContent.GetComponent<RectTransform>();

        // 각 행성 데이터에 대해 행성 오브젝트 생성 및 배치
        foreach (PlanetData planetData in planetDatas)
        {
            Planet planetInstance = Instantiate(planetPrefab, worldPanelContent.transform).GetComponent<Planet>();
            planetInstance.SetPlanetData(planetData);

            planetInstance.onClicked = OnPlanetClicked;

            RectTransform planetRect = planetInstance.GetComponent<RectTransform>();
            SetupMapObject(
                planetRect,
                planetData.normalizedPosition,
                Constants.Planets.PlanetSize,
                contentRect
            );
        }

        DrawWorldNodesAndIndicator();

        // 플레이어의 현재 위치 표시기 배치
        GameObject currentPositionIndicator = Instantiate(currentPlayerIndicatorPrefab, worldPanelContent.transform);
        RectTransform positionIndicatorRect = currentPositionIndicator.GetComponent<RectTransform>();
        SetupMapObject(
            positionIndicatorRect,
            GameManager.Instance.normalizedPlayerPosition,
            Constants.Planets.PlanetCurrentPositionIndicatorSize,
            contentRect
        );


        isMapInitialized = true;
    }

    private void SetupMapObject(RectTransform rectTransform, Vector2 normalizedPosition, float sizeScale,
        RectTransform contentRect)
    {
        // 앵커 설정
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 위치 계산
        Vector2 mapPosition = new(
            normalizedPosition.x * contentRect.rect.width,
            normalizedPosition.y * contentRect.rect.height
        );
        rectTransform.anchoredPosition = mapPosition;

        // 크기 설정
        float size = sizeScale * Mathf.Min(contentRect.rect.width, contentRect.rect.height);
        rectTransform.sizeDelta = new Vector2(size, size);
    }

    public void SetCurrentNodePosition(Vector2 normalizedPosition)
    {
        currentWorldNodePosition = normalizedPosition;
    }

    private void OnPlanetClicked(Planet clickedPlanet)
    {
        if (GameManager.Instance.CurrentState == GameState.Warp)
            return;

        Debug.Log($"MapPanelController: 행성 클릭됨 -");

        GameManager.Instance.SaveWorldMap();

        GameManager.Instance.ChangeGameState(GameState.Warp);

        OnPanelButtonClicked(panelWarp);
    }

    #endregion
}
