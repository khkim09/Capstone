using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class MapPanelController : MonoBehaviour
{
    [Header("탭 버튼")] [SerializeField] private Button buttonWarp;
    [SerializeField] private Button buttonWorld;

    [Header("전체 탭")] [SerializeField] private GameObject Tabs;

    [Header("패널 목록")] [SerializeField] private GameObject panelWarp;
    [SerializeField] private GameObject panelWorld;
    [SerializeField] private GameObject panelUnselected;

    [Header("워프 패널 설정")] [SerializeField] private GameObject warpPanelContent;
    [SerializeField] private GameObject warpNodePrefab;
    [SerializeField] private GameObject nodeConnectionPrefab;
    [SerializeField] private GameObject checkWarpUI;
    private WarpNode recentClickedNode;

    [Header("행성 도착 버튼")] [SerializeField] private Button buttonPlanetLand;


    private Planet targetPlanet;

    private int nodeLayerCount => GameManager.Instance.WorldNodeDataList.Count;

    // 모든 워프 노드 데이터 리스트
    private List<WarpNodeData> warpNodes = new();

    // 레이어별 노드 데이터 저장
    private List<List<WarpNodeData>> nodesByLayer = new();


    [Header("월드 패널 설정")] [SerializeField] private GameObject worldPanelContent;
    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject currentPlayerIndicatorPrefab;
    [SerializeField] private GameObject validRangeIndicatorPrefab;
    [SerializeField] private GameObject worldNodePrefab;
    private Vector2 currentWorldNodePosition;
    private GameObject currentPositionIndicator;
    [SerializeField] private float indicatorRotationSpeed = 30f; // 초당 회전 각도


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

    private void Awake()
    {
        buttonPlanetLand.onClick.AddListener(() => GameManager.Instance.LandOnPlanet());
    }

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

        if (currentPositionIndicator != null)
            // 현재 회전에 계속해서 각도 추가
            currentPositionIndicator.transform.Rotate(Vector3.back, indicatorRotationSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            Debug.Log("게임 매니저가 NULL");
            return;
        }

        currentWorldNodePosition = GameManager.Instance.normalizedPlayerPosition;
        // 먼저 월드맵을 그려서 행성 오브젝트들을 생성
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

    // MapPanelController.cs 수정

    private void InitializeWarpPanel()
    {
        if (GameManager.Instance.CurrentState != GameState.Warp)
        {
            Debug.Log("워프 상태가 아닙니다.");
            return;
        }

        // GameManager에서 저장된 워프맵 확인
        if (GameManager.Instance.WarpNodeDataList.Count > 0)
        {
            SetTargetPlanetFromGameManager();

            // 저장된 워프맵 복원
            RestoreWarpMapFromGameManager();
            Debug.Log("저장된 워프맵을 복원했습니다.");
            return;
        }

        // 저장된 워프맵이 없다면 새로 생성
        ClearWarpNodes();

        if (targetPlanet != null)
        {
            GenerateNodeMap(nodeLayerCount);

            // 생성된 워프맵을 GameManager에 저장
            SaveWarpMapToGameManager();
            Debug.Log("새로운 워프맵을 생성하고 저장했습니다.");
        }
        else
        {
            Debug.LogWarning("타겟 행성이 설정되지 않았습니다.");
        }
    }

    private void SetTargetPlanetFromGameManager()
    {
        int targetPlanetIndex = GameManager.Instance.CurrentWarpTargetPlanetId;

        if (targetPlanetIndex >= 0 && targetPlanetIndex < GameManager.Instance.PlanetDataList.Count)
        {
            PlanetData targetPlanetData = GameManager.Instance.PlanetDataList[targetPlanetIndex];

            // 월드 패널에서 해당 데이터를 가진 행성 찾기
            foreach (Transform child in worldPanelContent.transform)
            {
                Planet planet = child.GetComponent<Planet>();
                if (planet != null && planet.PlanetData == targetPlanetData)
                {
                    targetPlanet = planet;
                    Debug.Log($"타겟 행성 복원 완료: 인덱스 {targetPlanetIndex}");
                    return;
                }
            }

            Debug.LogWarning($"인덱스 {targetPlanetIndex}에 해당하는 행성 오브젝트를 찾을 수 없습니다.");
        }
        else
        {
            Debug.LogWarning($"유효하지 않은 행성 인덱스: {targetPlanetIndex}");
        }
    }

    private void RestoreWarpMapFromGameManager()
    {
        List<WarpNodeData> savedNodes = GameManager.Instance.WarpNodeDataList;
        RestoreWarpMapFromData(savedNodes);
    }

    // 생성된 워프맵을 GameManager에 저장
    private void SaveWarpMapToGameManager()
    {
        if (warpNodes.Count > 0 && targetPlanet != null)
        {
            // 연결 ID 정보 업데이트
            foreach (WarpNodeData node in warpNodes)
            {
                node.connectionIds.Clear();
                foreach (WarpNodeData connectedNode in node.connections) node.connectionIds.Add(connectedNode.nodeId);
            }

            // 타겟 행성의 인덱스 찾기
            int targetPlanetIndex = targetPlanet.PlanetData.planetId;

            // 디버깅 로그
            Debug.Log($"타겟 행성 저장 - 인덱스: {targetPlanetIndex}, 행성명: {targetPlanet.PlanetData.planetName}");
            Debug.Log($"전체 행성 수: {GameManager.Instance.PlanetDataList.Count}");

            if (targetPlanetIndex >= 0)
            {
                GameManager.Instance.SetCurrentWarpMap(warpNodes, targetPlanetIndex);
                Debug.Log($"워프맵 저장 성공 - 타겟 인덱스: {targetPlanetIndex}");
            }
            else
            {
                Debug.LogError($"타겟 행성 데이터를 리스트에서 찾을 수 없습니다. 행성명: {targetPlanet.PlanetData.planetName}");

                // 추가 디버깅: 리스트의 모든 행성 로그
                for (int i = 0; i < GameManager.Instance.PlanetDataList.Count; i++)
                    Debug.Log($"PlanetDataList[{i}]: {GameManager.Instance.PlanetDataList[i].planetName}");
            }
        }
        else
        {
            Debug.LogWarning($"워프맵 저장 실패 - 노드 수: {warpNodes.Count}, 타겟 행성: {(targetPlanet != null ? "있음" : "없음")}");
        }

        RestoreWarpMapFromGameManager();

        GameManager.Instance.SaveGameData();
    }

    private void OnWarpNodeClicked(WarpNode clickedNode)
    {
        // 클릭한 워프 노드 처리
        Debug.Log($"워프 노드 클릭됨: 레이어 {clickedNode.NodeData.layer}, 인덱스 {clickedNode.NodeData.indexInLayer}");

        // 현재 플레이어와 직접 연결된 노드만 이동 가능
        if (!clickedNode.IsDirectlyConnectedToPlayer())
        {
            Debug.Log("해당 노드로 직접 이동할 수 없습니다.");
            return;
        }

        recentClickedNode = clickedNode;
        checkWarpUI.SetActive(true);
    }

    public void Warp()
    {
        if (recentClickedNode == null)
            return;
        checkWarpUI.SetActive(false);


        GameManager.Instance.AddYear();
        Ship playerShip = GameManager.Instance.GetPlayerShip();
        float fuelAmount = playerShip.CalculateWarpFuelCost();
        ResourceManager.Instance.ChangeResource(ResourceType.Fuel,
            -fuelAmount);
        // 플레이어 이동
        MovePlayerToNode(recentClickedNode);

        SlideClose();
    }

    // 플레이어를 특정 노드로 이동
    private void MovePlayerToNode(WarpNode targetNode)
    {
        // 이전 노드의 현재 플레이어 표시 해제
        foreach (Transform child in warpPanelContent.transform)
        {
            WarpNode node = child.GetComponent<WarpNode>();
            if (node != null) node.SetCurrentPlayerNode(false);
        }

        // 새 노드로 플레이어 이동
        GameManager.Instance.SetCurrentWarpNodeId(targetNode.NodeData.nodeId);
        targetNode.SetCurrentPlayerNode(true);


        if (!targetNode.NodeData.isEndNode)
            GameManager.Instance.normalizedPlayerPosition =
                GameManager.Instance.WorldNodeDataList[targetNode.NodeData.layer - 1].normalizedPosition;
        else
            GameManager.Instance.normalizedPlayerPosition = GameManager.Instance
                .PlanetDataList[GameManager.Instance.CurrentWarpTargetPlanetId].normalizedPosition;


        // 모든 노드의 상태 업데이트 (도달 가능성 재계산)
        RefreshAllNodeStates();

        Debug.Log($"플레이어가 노드 {targetNode.NodeData.nodeId}로 이동했습니다.");

        if (currentPositionIndicator != null) UpdatePlayerIndicator();


        // 끝 노드에 도달했다면 워프 완료 처리
        if (targetNode.NodeData.isEndNode) OnWarpCompleted();


        // 게임 상태 저장
        GameManager.Instance.SaveGameData();
    }

    private void OnWarpCompleted()
    {
        buttonPlanetLand.gameObject.SetActive(true);
    }

    private void RefreshAllNodeStates()
    {
        foreach (Transform child in warpPanelContent.transform)
        {
            WarpNode node = child.GetComponent<WarpNode>();
            if (node != null) node.SetNodeData(node.NodeData); // 상태 업데이트 트리거
        }
    }


    // 데이터로부터 워프맵 복원 (nodesByLayer 자동 재생성)
    private void RestoreWarpMapFromData(List<WarpNodeData> savedNodes)
    {
        ClearWarpNodes();

        // 노드 데이터 복원
        warpNodes = new List<WarpNodeData>(savedNodes);
        nodesByLayer.Clear();

        // 최대 레이어 수 찾기
        int maxLayer = 0;
        foreach (WarpNodeData node in warpNodes) maxLayer = Mathf.Max(maxLayer, node.layer);

        // nodesByLayer 재생성
        for (int i = 0; i <= maxLayer; i++) nodesByLayer.Add(new List<WarpNodeData>());

        // 노드들을 레이어별로 분류
        foreach (WarpNodeData node in warpNodes) nodesByLayer[node.layer].Add(node);

        // 연결 관계 복원
        SetupNodeConnections();

        // 시각적 요소 생성
        CreateVisualNodes();
    }


    private void GenerateNodeMap(int layerCount)
    {
        // 데이터 구조 초기화
        warpNodes.Clear();
        nodesByLayer.Clear();

        // 노드 ID 카운터
        int nodeIdCounter = 0;

        for (int i = 0; i <= layerCount + 1; i++) nodesByLayer.Add(new List<WarpNodeData>());

        // 1. 시작 노드 생성 (레이어 0)
        WarpNodeData startNode = new()
        {
            normalizedPosition = new Vector2(Constants.WarpNodes.EdgeMarginHorizontal, 0.5f),
            isStartNode = true,
            isEndNode = false,
            layer = 0,
            indexInLayer = 0,
            nodeId = nodeIdCounter++
        };

        warpNodes.Add(startNode);
        nodesByLayer[0].Add(startNode);

        // 2. 중간 레이어 노드 생성 (레이어 1 ~ layerCount)
        for (int layer = 1; layer <= layerCount; layer++)
        {
            // x 좌표 계산 (모든 노드가 같은 레이어에서 같은 x 값)
            float xPos = Constants.WarpNodes.EdgeMarginHorizontal +
                         (1f - 2f * Constants.WarpNodes.EdgeMarginHorizontal) * layer / (layerCount + 1);

            // 해당 레이어의 노드 수 결정 (2~5개)
            int nodesInLayer = Random.Range(
                Constants.WarpNodes.LayerNodeCountMin,
                Constants.WarpNodes.LayerNodeCountMax + 1);

            // 각 노드 생성
            for (int i = 0; i < nodesInLayer; i++)
            {
                // y 좌표 계산 (동일한 간격으로 분포)
                float yPos = Constants.WarpNodes.EdgeMarginVertical +
                             (1f - 2f * Constants.WarpNodes.EdgeMarginVertical) * i /
                             Mathf.Max(1, nodesInLayer - 1);

                WarpNodeData node = new()
                {
                    normalizedPosition = new Vector2(xPos, yPos),
                    isStartNode = false,
                    isEndNode = false,
                    layer = layer,
                    indexInLayer = i,
                    nodeId = nodeIdCounter++,
                    nodeType = Random.value < Constants.WarpNodes.EventNodeRate
                        ? WarpNodeType.Event
                        : WarpNodeType.Combat
                };

                warpNodes.Add(node);
                nodesByLayer[layer].Add(node);
            }
        }

        // 3. 끝 노드 생성 (레이어 layerCount+1)
        WarpNodeData endNode = new()
        {
            normalizedPosition = new Vector2(1f - Constants.WarpNodes.EdgeMarginHorizontal, 0.5f),
            isStartNode = false,
            isEndNode = true,
            layer = layerCount + 1,
            indexInLayer = 0,
            nodeId = nodeIdCounter++,
            nodeType = WarpNodeType.Planet
        };

        warpNodes.Add(endNode);
        nodesByLayer[layerCount + 1].Add(endNode);

        // 4. 노드 간 연결 설정
        ConnectNodes();

        // 5. ID 기반 연결 설정
        SetupNodeConnections();

        // 6. 시각적 노드 생성
        CreateVisualNodes();
    }

    private void SetupNodeConnections()
    {
        // 노드 ID를 키로 하는 딕셔너리 생성
        Dictionary<int, WarpNodeData> nodeMap = new();
        foreach (WarpNodeData node in warpNodes) nodeMap[node.nodeId] = node;

        // 각 노드의 연결 설정
        foreach (WarpNodeData node in warpNodes) node.SetConnectionsByIds(nodeMap);
    }

    private void ConnectNodes()
    {
        // 각 레이어를 순회하며 노드 연결
        for (int layer = 0; layer < nodesByLayer.Count - 1; layer++)
        {
            List<WarpNodeData> currentLayerNodes = nodesByLayer[layer];
            List<WarpNodeData> nextLayerNodes = nodesByLayer[layer + 1];

            // 시작 노드는 모든 다음 레이어 노드와 연결
            if (layer == 0 && currentLayerNodes.Count == 1 && currentLayerNodes[0].isStartNode)
            {
                foreach (WarpNodeData nextNode in nextLayerNodes) currentLayerNodes[0].AddConnection(nextNode);
                continue;
            }

            // 마지막-1 레이어는 모든 노드가 끝 노드와 연결
            if (layer == nodesByLayer.Count - 2 && nextLayerNodes.Count == 1 && nextLayerNodes[0].isEndNode)
            {
                foreach (WarpNodeData currentNode in currentLayerNodes) currentNode.AddConnection(nextLayerNodes[0]);
                continue;
            }

            // 일반 레이어 간 연결 (교차 없이)
            if (currentLayerNodes.Count > 0 && nextLayerNodes.Count > 0)
            {
                // 현재 레이어와 다음 레이어의 노드들을 y 좌표 기준으로 정렬 (위에서 아래로)
                currentLayerNodes.Sort((a, b) => b.normalizedPosition.y.CompareTo(a.normalizedPosition.y));
                nextLayerNodes.Sort((a, b) => b.normalizedPosition.y.CompareTo(a.normalizedPosition.y));

                // Step 1: 각 다음 레이어 노드에 최소 1개의 연결 보장 (후진 연결성)
                for (int i = 0; i < nextLayerNodes.Count; i++)
                {
                    // 현재 레이어 중에서 적절한 노드 찾기 (위치 기반)
                    float ratio = nextLayerNodes.Count > 1 ? (float)i / (nextLayerNodes.Count - 1) : 0f;
                    int targetCurrentIndex = Mathf.RoundToInt(ratio * (currentLayerNodes.Count - 1));

                    // 선택된 현재 노드와 다음 노드 연결
                    currentLayerNodes[targetCurrentIndex].AddConnection(nextLayerNodes[i]);
                }

                // Step 1.5: 각 현재 레이어 노드에 최소 1개의 연결 보장 (전진 연결성)
                for (int i = 0; i < currentLayerNodes.Count; i++)
                {
                    WarpNodeData currentNode = currentLayerNodes[i];

                    // 이미 연결이 있는지 확인
                    if (currentNode.connections.Count == 0)
                    {
                        // 연결이 없다면 가장 가까운 다음 레이어 노드와 연결
                        float ratio = currentLayerNodes.Count > 1 ? (float)i / (currentLayerNodes.Count - 1) : 0f;
                        int targetNextIndex = Mathf.RoundToInt(ratio * (nextLayerNodes.Count - 1));

                        currentNode.AddConnection(nextLayerNodes[targetNextIndex]);
                    }
                }

                // Step 2: 각 현재 레이어 노드에서 추가 연결 생성
                for (int i = 0; i < currentLayerNodes.Count; i++)
                {
                    WarpNodeData currentNode = currentLayerNodes[i];

                    // 현재 노드에 대한 연결 가능한 범위 계산
                    float segmentSize = (float)nextLayerNodes.Count / currentLayerNodes.Count;
                    int startIndex = Mathf.FloorToInt(i * segmentSize);
                    int endIndex = Mathf.FloorToInt((i + 1) * segmentSize);

                    // 마지막 노드는 남은 모든 노드와 연결 가능하도록 보정
                    if (i == currentLayerNodes.Count - 1)
                        endIndex = nextLayerNodes.Count;

                    // 범위가 0인 경우 최소 1개 보장
                    if (startIndex == endIndex && startIndex < nextLayerNodes.Count)
                        endIndex = startIndex + 1;

                    // 추가 연결 생성 (1~2개 더)
                    int additionalConnections = Random.Range(0, 4);
                    int maxConnections = Mathf.Min(
                        currentNode.connections.Count + additionalConnections,
                        endIndex - startIndex);

                    // 중복 없이 추가 연결 생성
                    HashSet<int> existingConnections = new();
                    foreach (WarpNodeData connectedNode in currentNode.connections)
                    {
                        int index = nextLayerNodes.IndexOf(connectedNode);
                        if (index >= 0) existingConnections.Add(index);
                    }

                    while (currentNode.connections.Count < maxConnections)
                    {
                        int randomIndex = Random.Range(startIndex, endIndex);
                        if (!existingConnections.Contains(randomIndex))
                        {
                            currentNode.AddConnection(nextLayerNodes[randomIndex]);
                            existingConnections.Add(randomIndex);
                        }
                    }
                }
            }
        }

        // Step 3: 연결성 검증 및 보정
        EnsureConnectivity();
    }

    private void EnsureConnectivity()
    {
        // 도달 가능한 노드들을 찾기 위한 BFS
        HashSet<WarpNodeData> reachableNodes = new();
        Queue<WarpNodeData> queue = new();

        // 시작 노드로부터 시작
        WarpNodeData startNode = nodesByLayer[0][0];
        queue.Enqueue(startNode);
        reachableNodes.Add(startNode);

        // BFS로 도달 가능한 모든 노드 찾기
        while (queue.Count > 0)
        {
            WarpNodeData currentNode = queue.Dequeue();

            foreach (WarpNodeData connectedNode in currentNode.connections)
                if (!reachableNodes.Contains(connectedNode))
                {
                    reachableNodes.Add(connectedNode);
                    queue.Enqueue(connectedNode);
                }
        }

        // 도달할 수 없는 노드들을 찾고 연결 추가
        for (int layer = 1; layer < nodesByLayer.Count; layer++)
            foreach (WarpNodeData node in nodesByLayer[layer])
                if (!reachableNodes.Contains(node))
                {
                    // 도달할 수 없는 노드를 이전 레이어의 적절한 노드와 연결
                    List<WarpNodeData> previousLayerNodes = nodesByLayer[layer - 1];

                    // 가장 가까운 이전 레이어 노드 찾기
                    WarpNodeData closestPreviousNode = null;
                    float minDistance = float.MaxValue;

                    foreach (WarpNodeData prevNode in previousLayerNodes)
                        if (reachableNodes.Contains(prevNode))
                        {
                            float distance = Mathf.Abs(prevNode.normalizedPosition.y - node.normalizedPosition.y);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closestPreviousNode = prevNode;
                            }
                        }

                    // 연결 추가
                    if (closestPreviousNode != null)
                    {
                        closestPreviousNode.AddConnection(node);
                        reachableNodes.Add(node);
                        queue.Enqueue(node);

                        // 새로 연결된 노드로부터 도달 가능한 노드 추가
                        while (queue.Count > 0)
                        {
                            WarpNodeData newReachableNode = queue.Dequeue();
                            foreach (WarpNodeData connectedNode in newReachableNode.connections)
                                if (!reachableNodes.Contains(connectedNode))
                                {
                                    reachableNodes.Add(connectedNode);
                                    queue.Enqueue(connectedNode);
                                }
                        }
                    }
                }
    }

    private void CreateVisualNodes()
    {
        RectTransform contentRect = warpPanelContent.GetComponent<RectTransform>();

        // 1. 노드 시각적 요소 먼저 생성
        Dictionary<WarpNodeData, WarpNode> nodeVisuals = new();

        foreach (WarpNodeData node in warpNodes)
        {
            WarpNode visualNode = CreateNodeVisual(node, contentRect);
            nodeVisuals.Add(node, visualNode);
        }

        // 2. 노드 연결선 그리기
        foreach (WarpNodeData node in warpNodes)
        foreach (WarpNodeData connectedNode in node.connections)
            if (nodeVisuals.ContainsKey(node) && nodeVisuals.ContainsKey(connectedNode))
                CreateNodeConnection(
                    nodeVisuals[node],
                    nodeVisuals[connectedNode]
                );
    }

    private WarpNode CreateNodeVisual(WarpNodeData nodeData, RectTransform contentRect)
    {
        // 노드 프리팹 생성
        GameObject nodeObj = Instantiate(warpNodePrefab, warpPanelContent.transform);

        // 노드 위치와 크기 설정
        RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();
        SetupMapObject(
            nodeRect,
            nodeData.normalizedPosition,
            Constants.WarpNodes.NodeSize,
            contentRect
        );

        // 노드 데이터 설정
        WarpNode node = nodeObj.GetComponent<WarpNode>();
        if (node != null)
        {
            node.SetNodeData(nodeData);

            // 시작/끝 노드에 따른 시각적 차별화
            if (nodeData.isStartNode)
                node.SetAsStartNode();
            else if (nodeData.isEndNode) node.SetAsEndNode(targetPlanet);

            // 노드 클릭 이벤트 설정
            node.onClicked = OnWarpNodeClicked;
        }

        return node;
    }

    private void CreateNodeConnection(WarpNode fromNode, WarpNode toNode)
    {
        // 연결선 프리팹 생성
        GameObject connectionObj = Instantiate(nodeConnectionPrefab, warpPanelContent.transform);

        // 연결선을 노드보다 먼저 그려지도록 설정 (레이어 순서)
        connectionObj.transform.SetSiblingIndex(0);

        RectTransform connectionRect = connectionObj.GetComponent<RectTransform>();

        // 시작점과 끝점 사이의 중간 위치 계산
        Vector2 startPos = fromNode.GetConnectionPosition();
        Vector2 endPos = toNode.GetConnectionPosition();

        Vector2 midPoint = (startPos + endPos) * 0.5f;

        // 두 점 사이의 거리 계산
        float distance = Vector2.Distance(startPos, endPos);

        // 회전 각도 계산
        float angle = Mathf.Atan2(endPos.y - startPos.y, endPos.x - startPos.x) * Mathf.Rad2Deg;

        // 화면 크기에 대한 상대적인 두께 계산
        float thickness = Constants.WarpNodes.ConnectionLineThickness *
                          Mathf.Min(connectionRect.rect.width, connectionRect.rect.height);

        connectionRect.position = midPoint;
        connectionRect.sizeDelta = new Vector2(distance, thickness);
        connectionRect.localRotation = Quaternion.Euler(0, 0, angle);
    }


    private void ClearWarpNodes()
    {
        // 워프 패널의 모든 자식 오브젝트 제거
        foreach (Transform child in warpPanelContent.transform) Destroy(child.gameObject);

        // 노드 데이터 초기화
        warpNodes.Clear();
        nodesByLayer.Clear();
    }

    #endregion

    #region 월드 패널 설정

    private void InitializeWorldPanel()
    {
        RectMask2D rectMask = worldPanelContent.GetComponent<RectMask2D>();
        if (rectMask == null) worldPanelContent.AddComponent<RectMask2D>();

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

        // 워프 상태가 아닐 때만 유효 범위 표시기 그리기
        if (GameManager.Instance.CurrentState != GameState.Warp)
        {
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
    }

    private void DrawWorldMap()
    {
        // 이미 초기화되었다면 건너뛰기
        if (isMapInitialized && !IsPlayerMoved)
            return;

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


        if (currentPositionIndicator != null)
            Destroy(currentPositionIndicator);

        currentPositionIndicator = Instantiate(currentPlayerIndicatorPrefab, worldPanelContent.transform);

        RectTransform positionIndicatorRect = currentPositionIndicator.GetComponent<RectTransform>();
        SetupMapObject(
            positionIndicatorRect,
            GameManager.Instance.normalizedPlayerPosition,
            Constants.Planets.PlanetCurrentPositionIndicatorSize,
            contentRect
        );

        playerPositionBefore = GameManager.Instance.normalizedPlayerPosition;


        isMapInitialized = true;
    }

    private void UpdatePlayerIndicator()
    {
        if (currentPositionIndicator == null)
            return;

        RectTransform contentRect = worldPanelContent.GetComponent<RectTransform>();
        RectTransform positionIndicatorRect = currentPositionIndicator.GetComponent<RectTransform>();

        SetupMapObject(
            positionIndicatorRect,
            GameManager.Instance.normalizedPlayerPosition,
            Constants.Planets.PlanetCurrentPositionIndicatorSize,
            contentRect
        );

        // 현재 위치 저장
        playerPositionBefore = GameManager.Instance.normalizedPlayerPosition;
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

        RectTransform contentRect = worldPanelContent.GetComponent<RectTransform>();
        float mapWidth = contentRect.rect.width;
        float mapHeight = contentRect.rect.height;
        float mapMin = Mathf.Min(mapWidth, mapHeight);

        // 종횡비를 고려한 위치 계산
        Vector2 planetPosition = clickedPlanet.PlanetData.normalizedPosition;
        Vector2 aspectAdjustedPlanetPosition = new(
            planetPosition.x * (mapWidth / mapMin),
            planetPosition.y * (mapHeight / mapMin)
        );

        Vector2 aspectAdjustedNodePosition = new(
            currentWorldNodePosition.x * (mapWidth / mapMin),
            currentWorldNodePosition.y * (mapHeight / mapMin)
        );

        // 종횡비가 반영된 거리 계산
        float distance = Vector2.Distance(aspectAdjustedPlanetPosition, aspectAdjustedNodePosition);
        float validRadius = Constants.Planets.PlanetNodeValidRadius / 2;

        // 거리가 유효 범위를 벗어나면 처리하지 않음
        if (distance > validRadius)
        {
            Debug.Log($"행성이 유효 범위를 벗어남: 거리 {distance}, 최대 {validRadius}");
            return;
        }

        targetPlanet = clickedPlanet;
        targetPlanet.HideTooltip();

        GameManager.Instance.SetCurrentWarpNodeId(0);

        int planetIndex = GameManager.Instance.PlanetDataList.IndexOf(clickedPlanet.PlanetData);
        Debug.Log($"클릭된 행성 인덱스: {planetIndex}");

        // 월드맵의 유효 범위 인디케이터 제거
        foreach (Transform child in worldPanelContent.transform)
            if (child.name.Contains("Valid Range Indicator"))
                Destroy(child.gameObject);

        GameManager.Instance.ChangeGameState(GameState.Warp);

        GameManager.Instance.SaveGameData();

        OnPanelButtonClicked(panelWarp);
    }

    #endregion
}
