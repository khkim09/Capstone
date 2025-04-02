using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이벤트 트리 맵을 생성하고 시각화하는 매니저.
/// 노드를 자동으로 배치하고, 위험 여부를 표시하며, 연결선까지 구성합니다.
/// 노드 클릭 시 선택 이벤트를 발생시킵니다.
/// </summary>
public class EventTreeMap : MonoBehaviour
{
    #region 필드 및 참조

    [Header("UI References")] [SerializeField]
    private RectTransform treeContainer;

    [SerializeField] private GameObject eventNodePrefab;
    [SerializeField] private GameObject connectionLinePrefab;

    [Header("Map Settings")] [SerializeField]
    private float minNodeDistance = 80f;

    [SerializeField] private int minNodesPerLayer = 1; // 레이어당 최소 노드 수
    [SerializeField] private int maxNodesPerLayer = 4; // 레이어당 최대 노드 수

    [Header("Node Type Settings")] [SerializeField]
    private Color startNodeColor = Color.green;

    [SerializeField] private Color endNodeColor = Color.red;
    [SerializeField] private Color pirateNodeColor = new(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color spaceStationNodeColor = new(0.2f, 0.6f, 0.8f);
    [SerializeField] private Color randomEventNodeColor = new(0.8f, 0.6f, 0.8f);

    [Header("Danger Node Colors")] [SerializeField]
    private Color dangerNodeColor = new(0.6f, 0.2f, 0.8f); // 보라색

    // 노드 선택 이벤트 정의
    public delegate void NodeSelectedHandler(EventNode node);

    public event NodeSelectedHandler OnNodeSelected;

    private readonly List<EventNode> allNodes = new();
    private readonly List<GameObject> connectionLines = new();
    private List<List<EventNode>> layerNodes = new();
    private List<bool> layerDangerInfo = new();

    #endregion

    #region 초기화 및 트리 생성

    /// <summary>
    /// 트리 맵 초기화. 기존 노드와 연결을 모두 제거합니다.
    /// </summary>
    public void Initialize()
    {
        // 초기화 로직
        ClearCurrentTree();
    }

    /// <summary>
    /// 외부 경로 정보(pathNodes)와 위험 정보(dangerInfo)를 받아 트리를 생성합니다.
    /// </summary>
    public void GenerateTreeFromPath(List<Vector2> pathNodes, List<bool> dangerInfo = null)
    {
        if (pathNodes == null || pathNodes.Count < 2)
        {
            Debug.LogError("Path must have at least a start and end node!");
            return;
        }

        ClearCurrentTree();

        int levelCount = pathNodes.Count; // 경로의 노드 수로 레이어 수 결정

        // 위험 정보 저장
        layerDangerInfo = dangerInfo ?? new List<bool>(new bool[levelCount]);

        // 위험 정보 길이가 맞지 않으면 조정
        if (layerDangerInfo.Count != levelCount)
        {
            Debug.LogWarning(
                $"Danger info length ({layerDangerInfo.Count}) doesn't match path nodes length ({levelCount}). Adjusting...");
            while (layerDangerInfo.Count < levelCount) layerDangerInfo.Add(false);
            if (layerDangerInfo.Count > levelCount) layerDangerInfo = layerDangerInfo.GetRange(0, levelCount);
        }

        GenerateImprovedTree(levelCount);
    }

    /// <summary>
    /// 레이어 수(layers)를 기반으로 트리를 생성합니다.
    /// 각 레이어는 랜덤한 노드 개수를 갖고, 노드는 평면적으로 연결됩니다.
    /// </summary>
    public void GenerateImprovedTree(int layers)
    {
        ClearCurrentTree();

        // 위험 정보가 없으면 기본값 설정
        if (layerDangerInfo == null || layerDangerInfo.Count != layers)
            layerDangerInfo = new List<bool>(new bool[layers]);

        // 트리 컨테이너의 크기 가져오기
        float width = treeContainer.rect.width;
        float height = treeContainer.rect.height;

        // 맵 컨테이너의 좌표계를 고려 (중앙이 0,0)
        float halfWidth = width / 2;
        float halfHeight = height / 2;

        // 레이어별 노드 생성
        layerNodes = new List<List<EventNode>>();

        for (int layer = 0; layer < layers; layer++)
        {
            List<EventNode> currentLayerNodes = new();

            // 레이어당 노드 수 결정
            int nodeCount;
            if (layer == 0 || layer == layers - 1)
            {
                nodeCount = 1; // 시작과 끝은 한 개의 노드만
            }
            else
            {
                // 완전 랜덤한 노드 개수
                nodeCount = UnityEngine.Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);

                // 선택적: 첫 번째와 마지막 레이어 근처일수록 노드 수 감소
                int distanceFromEdge = Mathf.Min(layer, layers - 1 - layer);
                if (distanceFromEdge == 1) // 시작/끝 바로 다음/이전 레이어
                    nodeCount = Mathf.Min(nodeCount, 3); // 최대 3개로 제한
            }

            // 레이어의 노드 배치 공간 계산
            float usableHeight = height * 0.8f; // 위아래 여백 고려
            float nodeSpacing = nodeCount > 1 ? usableHeight / (nodeCount - 1) : 0;

            // 각 레이어의 X 위치 계산 (맵 왼쪽부터 오른쪽까지 균등하게)
            float layerX = Mathf.Lerp(-halfWidth + 50, halfWidth - 50, (float)layer / (layers - 1));

            // 현재 레이어의 위험 여부
            bool isLayerDangerous = layer < layerDangerInfo.Count ? layerDangerInfo[layer] : false;

            for (int i = 0; i < nodeCount; i++)
            {
                // Y 위치 계산
                float nodeY;
                if (nodeCount == 1)
                {
                    nodeY = 0; // 중앙
                }
                else
                {
                    float margin = 20f; // 원하는 상하 여백 (픽셀 단위)
                    nodeY = Mathf.Lerp(-halfHeight * 0.8f + margin, halfHeight * 0.8f - margin,
                        (float)i / (nodeCount - 1));
                }

                // 시작 노드의 경우 맵 왼쪽 가장자리에 배치
                if (layer == 0) layerX = -halfWidth + 20; // 왼쪽 가장자리에서 약간 여백

                // 끝 노드의 경우 맵 오른쪽 가장자리에 배치
                if (layer == layers - 1) layerX = halfWidth - 20; // 오른쪽 가장자리에서 약간 여백

                // 노드 타입 결정
                NodeType nodeType;
                if (layer == 0) nodeType = NodeType.Start;
                else if (layer == layers - 1) nodeType = NodeType.End;
                else nodeType = DetermineNodeType(layer, layers);

                // 노드 생성 - 위험 정보 전달
                EventNode node = CreateNode(
                    new Vector2(layerX, nodeY),
                    layer,
                    i,
                    nodeType,
                    isLayerDangerous
                );

                currentLayerNodes.Add(node);
            }

            layerNodes.Add(currentLayerNodes);
        }

        // 교차하지 않는 노드 간 연결 생성 - 새로운 방식 적용
        CreateStrictlyPlanarConnections();

        // 연결선 시각화
        CreateConnectionLines();
    }

    #endregion

    #region 노드 타입 및 생성

    // 노드 타입 정의 (필요시 외부로 이동 가능)
    public enum NodeType
    {
        Start,
        Pirate,
        SpaceStation,
        RandomEvent,
        End
    }

    /// <summary>
    /// 노드 타입을 결정합니다. 위치(layer)에 따라 우선순위를 다르게 적용합니다.
    /// </summary>
    private NodeType DetermineNodeType(int layer, int totalLayers)
    {
        float random = UnityEngine.Random.value;

        // 보스 바로 전 레이어는 우주 정거장 확률 높임
        if (layer == totalLayers - 2)
        {
            if (random < 0.7f)
                return NodeType.SpaceStation;
            else if (random < 0.85f)
                return NodeType.RandomEvent;
            else
                return NodeType.Pirate;
        }

        // 일반적인 레이어
        if (random < 0.5f)
            return NodeType.Pirate;
        else if (random < 0.8f)
            return NodeType.SpaceStation;
        else
            return NodeType.RandomEvent;
    }

    /// <summary>
    /// 노드를 생성하고 위치, 색상, 클릭 이벤트 등을 설정합니다.
    /// </summary>
    private EventNode CreateNode(Vector2 position, int layer, int index, NodeType type, bool isDangerous)
    {
        GameObject nodeObj = Instantiate(eventNodePrefab, treeContainer);
        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;

        // 노드 컴포넌트 설정
        EventNode nodeComponent = nodeObj.GetComponent<EventNode>();
        if (nodeComponent == null) nodeComponent = nodeObj.AddComponent<EventNode>();
        nodeComponent.SetNodeData(layer, index, isDangerous);

        // 노드 시각적 스타일 적용
        ApplyNodeStyle(nodeObj, type, isDangerous);

        // 노드 이름 설정
        string dangerText = isDangerous ? "Danger_" : "";
        nodeObj.name = $"{dangerText}Node_{layer}_{index}_{type}";

        // 클릭 이벤트 설정
        Button button = nodeObj.GetComponent<Button>();
        if (button == null) button = nodeObj.AddComponent<Button>();

        EventNode capturedNode = nodeComponent; // 클로저 문제 방지
        button.onClick.AddListener(() => OnNodeClicked(capturedNode));

        allNodes.Add(nodeComponent);
        return nodeComponent;
    }

    /// <summary>
    /// 노드의 색상과 스타일을 적용합니다. 위험 지역 여부에 따라 색상이 다릅니다.
    /// </summary>
    private void ApplyNodeStyle(GameObject nodeObj, NodeType type, bool isDangerous)
    {
        Image image = nodeObj.GetComponent<Image>();
        if (image == null) return;

        // 위험 지대 여부에 따라 색상 선택
        if (isDangerous)
            // 위험 지대 노드는 모두 보라색으로 설정
            image.color = dangerNodeColor;
        else
            // 일반 노드는 타입에 따른 기본 색상 사용
            switch (type)
            {
                case NodeType.Start:
                    image.color = startNodeColor;
                    break;
                case NodeType.End:
                    image.color = endNodeColor;
                    break;
                case NodeType.Pirate:
                    image.color = pirateNodeColor;
                    break;
                case NodeType.SpaceStation:
                    image.color = spaceStationNodeColor;
                    break;
                case NodeType.RandomEvent:
                    image.color = randomEventNodeColor;
                    break;
                default:
                    image.color = Color.white;
                    break;
            }

        // 디버그용: 노드 이름에 타입과 위험 정보 추가
        nodeObj.name = $"Node_{type}_{(isDangerous ? "Danger" : "Safe")}";
    }

    #endregion

    #region 노드 연결 생성

    /// <summary>
    /// 평면성을 유지하면서 노드들을 계층적으로 연결합니다.
    /// 연결 로직은 중복 방지, 균형 유지, 연결 누락 보정까지 처리합니다.
    /// </summary>
    private void CreateStrictlyPlanarConnections()
    {
        // 모든 연결 초기화
        foreach (EventNode node in allNodes) node.ClearConnections();

        // 층별 연결 생성
        for (int layer = 0; layer < layerNodes.Count - 1; layer++)
        {
            List<EventNode> currentLayer = layerNodes[layer];
            List<EventNode> nextLayer = layerNodes[layer + 1];

            // 특별한 경우: 마지막 레이어 전에는 모든 노드가 마지막 노드(보스)에 연결
            if (layer == layerNodes.Count - 2)
            {
                foreach (EventNode node in currentLayer) node.AddNextNode(nextLayer[0]); // 끝 노드는 하나뿐
                continue;
            }

            // 특별한 경우: 시작 노드는 다음 레이어의 모든 노드에 연결 가능 (교차 없음)
            if (layer == 0 && currentLayer.Count == 1)
            {
                // 최소 1개, 최대 전체 노드 연결
                int connectionsCount = UnityEngine.Random.Range(1, nextLayer.Count + 1);

                // 연결할 노드 무작위 선택
                List<int> indices = Enumerable.Range(0, nextLayer.Count).ToList();
                for (int i = 0; i < nextLayer.Count - connectionsCount; i++)
                    indices.RemoveAt(UnityEngine.Random.Range(0, indices.Count));

                // 정렬하여 아래에서 위로 연결
                indices.Sort();

                foreach (int idx in indices) currentLayer[0].AddNextNode(nextLayer[idx]);

                continue;
            }

            // 일반적인 경우: 엄격한 연결 범위 적용

            // 1. 두 계층의 노드 수에 따라 연결 방식 결정
            if (currentLayer.Count == 1)
            {
                // 하나의 노드에서 여러 노드로: 모든 연결 가능
                int maxConnections = Mathf.Min(nextLayer.Count, 3); // 최대 3개 연결 제한
                int connectionsCount = UnityEngine.Random.Range(1, maxConnections + 1);

                List<int> indices = new();
                // 항상 가장 바깥쪽 노드들을 포함하여 자연스러운 느낌 부여
                if (nextLayer.Count > 1)
                {
                    indices.Add(0); // 맨 위 노드
                    indices.Add(nextLayer.Count - 1); // 맨 아래 노드
                    connectionsCount = Mathf.Max(2, connectionsCount); // 최소 2개 보장
                }
                else
                {
                    indices.Add(0); // 유일한 노드
                }

                // 필요하면 중간 노드 추가
                if (connectionsCount > indices.Count && nextLayer.Count > 2)
                {
                    List<int> middleIndices = Enumerable.Range(1, nextLayer.Count - 2).ToList();
                    while (indices.Count < connectionsCount && middleIndices.Count > 0)
                    {
                        int randomIdx = UnityEngine.Random.Range(0, middleIndices.Count);
                        indices.Add(middleIndices[randomIdx]);
                        middleIndices.RemoveAt(randomIdx);
                    }

                    // 인덱스 정렬
                    indices.Sort();
                }

                foreach (int idx in indices) currentLayer[0].AddNextNode(nextLayer[idx]);
            }
            else if (nextLayer.Count == 1)
            {
                // 여러 노드에서 하나의 노드로: 모든 현재 노드가 다음 노드에 연결
                foreach (EventNode node in currentLayer) node.AddNextNode(nextLayer[0]);
            }
            else
            {
                // 여러 노드에서 여러 노드로 연결: 평면성 유지를 위한 주요 알고리즘

                // 2. 구간 기반 연결 방식 적용
                // 각 노드의 연결 가능 구간을 정의하여 교차 방지
                List<List<int>> nodeSegments = new();

                // 현재 레이어와 다음 레이어의 노드 수 비율 계산
                float ratio = (float)nextLayer.Count / currentLayer.Count;

                // 각 노드별 연결 가능 구간 계산
                for (int i = 0; i < currentLayer.Count; i++)
                {
                    int startIdx = Mathf.FloorToInt(i * ratio);
                    int endIdx = Mathf.CeilToInt((i + 1) * ratio) - 1;

                    // 범위 조정 (배열 범위 초과 방지)
                    endIdx = Mathf.Min(endIdx, nextLayer.Count - 1);

                    // 구간이 없는 경우 최소 하나의 연결 보장
                    if (startIdx > endIdx) startIdx = endIdx = Mathf.Min(startIdx, nextLayer.Count - 1);

                    // 이 노드의 연결 가능 인덱스들
                    List<int> connectable = new();
                    for (int j = startIdx; j <= endIdx; j++) connectable.Add(j);

                    nodeSegments.Add(connectable);
                }

                // 3. 각 노드마다 연결 생성
                for (int i = 0; i < currentLayer.Count; i++)
                {
                    List<int> possibleConnections = nodeSegments[i];

                    // 연결할 노드 수 결정 (최소 1개, 최대 구간 내 전체)
                    int connectionCount = possibleConnections.Count;
                    if (connectionCount > 1)
                        // 0.5 확률로 모든 노드 연결, 나머지는 랜덤하게 1~전체
                        if (UnityEngine.Random.value > 0.5f)
                            connectionCount = UnityEngine.Random.Range(1, connectionCount + 1);

                    // 연결할 노드 선택
                    List<int> selectedIndices = new(possibleConnections);
                    while (selectedIndices.Count > connectionCount)
                        selectedIndices.RemoveAt(UnityEngine.Random.Range(0, selectedIndices.Count));

                    // 구간의 양 끝을 우선적으로 유지 (자연스러운 경로 생성)
                    if (possibleConnections.Count > 1 && selectedIndices.Count >= 2)
                    {
                        if (!selectedIndices.Contains(possibleConnections[0]))
                        {
                            selectedIndices.Add(possibleConnections[0]);
                            if (selectedIndices.Count > connectionCount)
                            {
                                // 중간 노드 중 하나 제거
                                int removeIndex = UnityEngine.Random.Range(1, selectedIndices.Count - 1);
                                selectedIndices.RemoveAt(removeIndex);
                            }
                        }

                        if (!selectedIndices.Contains(possibleConnections[possibleConnections.Count - 1]))
                        {
                            selectedIndices.Add(possibleConnections[possibleConnections.Count - 1]);
                            if (selectedIndices.Count > connectionCount)
                            {
                                // 중간 노드 중 하나 제거
                                int removeIndex = UnityEngine.Random.Range(1, selectedIndices.Count - 1);
                                selectedIndices.RemoveAt(removeIndex);
                            }
                        }
                    }

                    // 인덱스 정렬
                    selectedIndices.Sort();

                    // 실제 연결 생성
                    foreach (int idx in selectedIndices) currentLayer[i].AddNextNode(nextLayer[idx]);
                }

                // 4. 연결되지 않은 노드 검사 및 수정
                for (int j = 0; j < nextLayer.Count; j++)
                {
                    bool hasIncomingConnection = false;
                    foreach (EventNode node in currentLayer)
                        if (node.NextNodes.Contains(nextLayer[j]))
                        {
                            hasIncomingConnection = true;
                            break;
                        }

                    // 연결되지 않은 노드 발견
                    if (!hasIncomingConnection)
                    {
                        // 이 노드의 인덱스에 해당하는 구간을 가진 현재 레이어의 노드 찾기
                        int sourceNodeIdx = -1;

                        // 비율 기반으로 가장 적절한 소스 노드 찾기
                        sourceNodeIdx = Mathf.Min(Mathf.FloorToInt(j / ratio), currentLayer.Count - 1);

                        // 연결 추가
                        if (sourceNodeIdx >= 0 && sourceNodeIdx < currentLayer.Count)
                        {
                            currentLayer[sourceNodeIdx].AddNextNode(nextLayer[j]);
                        }
                        else
                        {
                            // 예상치 못한 경우를 대비한 폴백
                            Debug.LogWarning($"Unexpected: Couldn't find source node for next layer node {j}");
                            currentLayer[currentLayer.Count - 1].AddNextNode(nextLayer[j]);
                        }
                    }
                }
            }
        }

        // 최종 검증: 모든 노드가 적절한 연결을 가지는지 확인
        VerifyAndFixConnections();
    }

    /// <summary>
    /// 모든 연결 관계를 검사하고, 연결이 없는 노드를 보정합니다.
    /// </summary>
    private void VerifyAndFixConnections()
    {
        // 1. 시작 노드가 적어도 하나의 연결을 가지는지 확인
        if (layerNodes[0][0].NextNodes.Count == 0 && layerNodes.Count > 1)
            layerNodes[0][0].AddNextNode(layerNodes[1][0]);

        // 2. 각 레이어의 모든 노드가 적어도 하나의 나가는 연결을 가지는지 확인 (마지막 레이어 제외)
        for (int layer = 0; layer < layerNodes.Count - 1; layer++)
            foreach (EventNode node in layerNodes[layer])
                if (node.NextNodes.Count == 0)
                {
                    // 다음 레이어에서 가장 적합한 노드 찾기
                    int nodeIdx = node.NodeIndex;
                    List<EventNode> nextLayer = layerNodes[layer + 1];

                    // 비율 기반으로 목적지 노드 인덱스 계산
                    float ratio = (float)nextLayer.Count / layerNodes[layer].Count;
                    int targetIdx = Mathf.Min(Mathf.RoundToInt(nodeIdx * ratio), nextLayer.Count - 1);

                    // 연결 추가
                    node.AddNextNode(nextLayer[targetIdx]);
                }

        // 3. 각 레이어의 모든 노드가 적어도 하나의 들어오는 연결을 가지는지 확인 (첫 레이어 제외)
        for (int layer = 1; layer < layerNodes.Count; layer++)
            foreach (EventNode node in layerNodes[layer])
            {
                bool hasIncomingConnection = false;
                foreach (EventNode prevNode in layerNodes[layer - 1])
                    if (prevNode.NextNodes.Contains(node))
                    {
                        hasIncomingConnection = true;
                        break;
                    }

                if (!hasIncomingConnection)
                {
                    // 이전 레이어에서 가장 적합한 노드 찾기
                    int nodeIdx = node.NodeIndex;
                    List<EventNode> prevLayer = layerNodes[layer - 1];

                    // 비율 기반으로 소스 노드 인덱스 계산
                    float ratio = (float)prevLayer.Count / layerNodes[layer].Count;
                    int sourceIdx = Mathf.Min(Mathf.RoundToInt(nodeIdx * ratio), prevLayer.Count - 1);

                    // 연결 추가
                    prevLayer[sourceIdx].AddNextNode(node);
                }
            }
    }

    #endregion

    /// <summary>
    /// 생성된 노드 간의 연결선을 시각적으로 생성합니다.
    /// 중복 연결은 방지됩니다.
    /// </summary>
    private void CreateConnectionLines()
    {
        // 기존 연결선 제거
        foreach (GameObject line in connectionLines)
            if (line != null)
                Destroy(line);
        connectionLines.Clear();

        // 새 연결선 생성
        HashSet<string> processedConnections = new();

        foreach (EventNode node in allNodes)
        {
            RectTransform sourceRect = node.gameObject.GetComponent<RectTransform>();

            foreach (EventNode nextNode in node.NextNodes)
            {
                // 중복 연결 방지
                string connectionKey = $"{node.LevelIndex}_{node.NodeIndex}_{nextNode.LevelIndex}_{nextNode.NodeIndex}";
                if (processedConnections.Contains(connectionKey)) continue;
                processedConnections.Add(connectionKey);

                RectTransform targetRect = nextNode.gameObject.GetComponent<RectTransform>();

                // 연결선 생성 - 이 부분이 중요
                GameObject lineObj = CreateConnectionLine(sourceRect, targetRect);

                if (lineObj != null) connectionLines.Add(lineObj);
            }
        }
    }

    /// <summary>
    /// 연결선 오브젝트를 생성하고, 위치/회전/길이를 자동으로 설정합니다.
    /// </summary>
    private GameObject CreateConnectionLine(RectTransform startNode, RectTransform endNode)
    {
        if (connectionLinePrefab == null)
        {
            Debug.LogError("Connection line prefab is not assigned!");
            return null;
        }

        // 연결선 오브젝트 생성
        GameObject lineObj = Instantiate(connectionLinePrefab, treeContainer);
        lineObj.transform.SetAsFirstSibling(); // 라인이 노드 뒤에 그려지도록
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        if (lineRect == null)
        {
            Debug.LogError("Connection line prefab does not have RectTransform component!");
            Destroy(lineObj);
            return null;
        }

        // 선의 위치 설정
        Vector2 startPosition = startNode.anchoredPosition;
        Vector2 endPosition = endNode.anchoredPosition;

        // 노드 가장자리에서 연결되도록 조정
        float startNodeHalfWidth = startNode.rect.width / 2;
        float endNodeHalfWidth = endNode.rect.width / 2;
        float startNodeHalfHeight = startNode.rect.height / 2;
        float endNodeHalfHeight = endNode.rect.height / 2;

        // 노드 간 방향 벡터
        Vector2 direction = (endPosition - startPosition).normalized;

        // 각 노드의 가장자리 위치 계산 (벡터 투영으로 더 정확하게)
        Vector2 adjustedStartPos =
            CalculateNodeEdgePosition(startPosition, direction, startNodeHalfWidth, startNodeHalfHeight);
        Vector2 adjustedEndPos =
            CalculateNodeEdgePosition(endPosition, -direction, endNodeHalfWidth, endNodeHalfHeight);

        // 두 위치의 중간점
        Vector2 centerPosition = (adjustedStartPos + adjustedEndPos) / 2f;
        lineRect.anchoredPosition = centerPosition;

        // 연결선의 길이 계산
        float distance = Vector2.Distance(adjustedStartPos, adjustedEndPos);

        // 이전 sizeDelta의 높이(선의 두께) 유지
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);

        // 회전 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        return lineObj;
    }

    /// <summary>
    /// 노드의 중심 위치에서 가장자리까지의 연결 지점을 계산합니다.
    /// </summary>
    private Vector2 CalculateNodeEdgePosition(Vector2 nodeCenter, Vector2 direction, float halfWidth, float halfHeight)
    {
        // 방향 벡터 정규화
        direction = direction.normalized;

        // 노드의 가장자리를 향한 벡터의 크기 계산 (타원 방정식 사용)
        float t;
        if (Mathf.Approximately(direction.x, 0))
        {
            // y축 방향일 경우
            t = halfHeight * Mathf.Sign(direction.y);
            return new Vector2(nodeCenter.x, nodeCenter.y + t);
        }
        else if (Mathf.Approximately(direction.y, 0))
        {
            // x축 방향일 경우
            t = halfWidth * Mathf.Sign(direction.x);
            return new Vector2(nodeCenter.x + t, nodeCenter.y);
        }
        else
        {
            // 직선과 타원의 교점 계산 (근사치)
            float slope = direction.y / direction.x;
            float absSlope = Mathf.Abs(slope);

            if (absSlope > halfHeight / halfWidth)
                // y 방향이 더 크면 y축 기준
                t = halfHeight / Mathf.Abs(direction.y);
            else
                // x 방향이 더 크면 x축 기준
                t = halfWidth / Mathf.Abs(direction.x);

            return nodeCenter + direction * t;
        }
    }

    /// <summary>
    /// 트리 내부의 모든 노드와 연결선을 제거하고 초기화합니다.
    /// </summary>
    private void ClearCurrentTree()
    {
        // 기존 노드와 연결선 제거
        foreach (EventNode node in allNodes)
            if (node != null && node.gameObject != null)
                Destroy(node.gameObject);

        foreach (GameObject line in connectionLines)
            if (line != null)
                Destroy(line);

        allNodes.Clear();
        connectionLines.Clear();
        layerNodes.Clear();

        // TreeContainer 내 모든 자식 오브젝트 제거 (혹시 모를 누수 방지)
        for (int i = treeContainer.childCount - 1; i >= 0; i--)
            Destroy(treeContainer.GetChild(i).gameObject);
    }

    /// <summary>
    /// 노드 클릭 시 발생하는 내부 메서드입니다.
    /// OnNodeSelected 이벤트를 트리거합니다.
    /// </summary>
    private void OnNodeClicked(EventNode node)
    {
        Debug.Log($"Clicked on node: Layer {node.LevelIndex}, Index {node.NodeIndex}");

        // 노드 선택 이벤트 발생
        OnNodeSelected?.Invoke(node);
    }
}
