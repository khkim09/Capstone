using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 노드 생성 맵을 생성하고 시각화하는 매니저.새
/// 클릭을 하여 노드를 찍고, 행성을 클릭하면 이벤트 트리 맵을 생성하게 호출한다.
/// </summary>
public class NodePlacementMap : MonoBehaviour
{
    #region 필드 및 프로퍼티

    [Header("UI References")] [SerializeField]
    private RectTransform mapContainer;

    [SerializeField] private GameObject validRangeIndicatorPrefab; // 유효 범위 표시기 프리팹
    [SerializeField] private GameObject nodePrefab; // 노드 프리팹
    [SerializeField] private GameObject planetPrefab; // 행성 프리팹
    [SerializeField] private GameObject tooltipPrefab; // 행성 툴팁 프리팹
    [SerializeField] private GameObject dangerZonePrefab; // 위험지역 표시 프리팹

    [Header("Settings")] [SerializeField] private float maxPlacementDistance;
    [SerializeField] private int numberOfPlanets = 20; // 행성 개수
    [SerializeField] private int maxPlacementAttempts = 30; // 위치 시도 최대 횟수
    [SerializeField] private float minimumDistanceBetweenPlanets = 100f; // 행성 간 최소 거리

    [Header("Danger Zone Settings")] [SerializeField]
    private int numDangerZones = 3; // 생성할 위험지역 개수

    [SerializeField] private float dangerZoneRadius = 100f; // 위험지역 반경
    [SerializeField] private Color dangerZoneColor = new(1f, 0f, 0f, 0.3f); // 위험지역 색상

    // 경로 완료 이벤트 정의
    public delegate void PathCompletedHandler(List<Vector2> path, List<bool> dangerInfo);

    public event PathCompletedHandler OnPathCompleted;

    private readonly List<RectTransform> placedNodeObjects = new(); // 배치된 노드 목록
    private readonly List<GameObject> planets = new(); // 생성된 행성 리스트
    private readonly List<GameObject> dangerZones = new(); // 생성된 위험지역 목록
    private Vector2 currentIndicatorPosition = Vector2.zero; // 현재 유효 범위 기준점
    private GameObject endPlanet; // 도착 행성
    private GameObject startPlanet; // 출발 행성
    private GameObject validRangeIndicatorInstance; // 생성된 인스턴스
    private EventSystem eventSystem;

    private bool nodePlacementCompleted = false;
    private List<Vector2> pathNodes = new();
    private List<bool> pathDangerInfo = new();

    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 초기화 시 호출되어 EventSystem을 설정합니다.
    /// </summary>
    private void Awake()
    {
        // EventSystem 참조 가져오기
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
            Debug.LogError("EventSystem not found in the scene!");
    }

    /// <summary>
    /// 외부에서 노드 맵을 초기화할 때 호출됩니다.
    /// 맵과 경로 데이터, 배치 상태를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        // 기존 초기화 로직
        ClearMapContents();

        // 경로 데이터 초기화
        pathNodes.Clear();
        pathDangerInfo.Clear();
        nodePlacementCompleted = false;
    }

    /// <summary>
    /// 행성 개수 및 배치 거리 설정 후 맵을 초기화합니다.
    /// </summary>
    /// <param name="planetCount">행성 개수</param>
    /// <param name="maxDistance">최대 배치 거리</param>
    public void InitializeMap(int planetCount, float maxDistance)
    {
        // 매개변수 저장
        numberOfPlanets = planetCount;
        maxPlacementDistance = maxDistance;

        // 맵 초기화 및 행성 생성
        ClearMapContents();
        GenerateRandomPlanets(numberOfPlanets);
        GenerateDangerZones();
        InitializeValidRangeIndicator();

        // 경로 데이터 초기화
        pathNodes.Clear();
        pathDangerInfo.Clear();
        nodePlacementCompleted = false;
    }

    #endregion

    #region 업데이트 및 이벤트 처리

    /// <summary>
    /// 매 프레임마다 유효 범위 표시기를 갱신합니다.
    /// </summary>
    private void Update()
    {
        // 유효 범위 표시기 계속 갱신
        if (validRangeIndicatorInstance != null)
            UpdateValidRangeIndicator(currentIndicatorPosition);
    }

    /// <summary>
    /// 맵 클릭 시 호출되어 노드 배치를 시도합니다.
    /// </summary>
    /// <param name="eventData">PointerEventData</param>
    public void OnMapClick(BaseEventData eventData)
    {
        if (eventData is PointerEventData pointerData)
        {
            // 행성 클릭 처리는 이제 Planet 컴포넌트에서 직접 처리하므로
            // 여기서는 맵 클릭만 처리
            Vector2 clickPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapContainer,
                    pointerData.position,
                    pointerData.pressEventCamera,
                    out clickPosition))
                TryPlaceNode(clickPosition); // 노드 배치 시도
        }
    }

    /// <summary>
    /// 행성 클릭 시 경로에 추가하고 배치를 종료합니다.
    /// </summary>
    /// <param name="planet">클릭된 행성 오브젝트</param>
    private void OnPlanetClicked(GameObject planet)
    {
        // 이미 노드 배치가 완료된 경우 무시
        if (nodePlacementCompleted) return;

        // 클릭된 행성의 위치로 currentIndicatorPosition 업데이트
        RectTransform planetRect = planet.GetComponent<RectTransform>();
        Vector2 planetPosition = planetRect.anchoredPosition;

        // 먼저 행성이 현재 유효 범위 내에 있는지 확인
        bool isInRange = Vector2.Distance(currentIndicatorPosition, planetPosition) <= maxPlacementDistance;

        if (isInRange)
        {
            currentIndicatorPosition = planetRect.anchoredPosition;

            // 행성이 위험지역 안에 있는지 체크
            bool isDangerous = IsPositionInDangerZone(planetPosition);
            pathDangerInfo.Add(isDangerous);

            // 경로에 노드 위치 추가
            pathNodes.Add(planetPosition);

            // ValidRangeIndicator 위치 업데이트
            UpdateValidRangeIndicator(currentIndicatorPosition);

            // 행성 클릭 시 노드 배치 종료
            CompletePlacementMap();
        }
    }

    /// <summary>
    /// 노드 배치를 시도합니다.
    /// </summary>
    /// <param name="position">배치할 위치</param>
    private void TryPlaceNode(Vector2 position)
    {
        // 노드 배치가 완료된 경우 무시
        if (nodePlacementCompleted) return;

        // 클릭 위치가 유효 범위 내인지 확인
        if (IsValidPlacement(position))
        {
            // 새 노드 생성
            CreateNodeUI(position);

            // 노드가 위험지역 안에 있는지 체크
            bool isDangerous = IsPositionInDangerZone(position);
            pathDangerInfo.Add(isDangerous);

            // 경로에 노드 위치 추가
            pathNodes.Add(position);

            // 노드 기준으로 유효 반경 표시기 업데이트
            currentIndicatorPosition = position;
            UpdateValidRangeIndicator(currentIndicatorPosition);
        }
    }

    #endregion

    #region 맵 생성 및 관리

    /// <summary>
    /// 랜덤한 위치에 행성들을 배치합니다.
    /// </summary>
    /// <param name="count">배치할 행성 수</param>
    public void GenerateRandomPlanets(int count)
    {
    }


    /// <summary>
    /// 위험지역을 생성합니다.
    /// </summary>
    private void GenerateDangerZones()
    {
        // 기존 위험지역 제거
        ClearDangerZones();

        RectTransform rectTransform = mapContainer;
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        for (int i = 0; i < numDangerZones; i++)
        {
            // 위험지역 위치 결정 (행성들과 최소 거리 유지)
            Vector2 position = FindValidDangerZonePosition(width, height);

            // 위험지역 생성
            GameObject dangerZone = Instantiate(dangerZonePrefab, mapContainer);
            dangerZone.name = $"DangerZone_{i}";

            RectTransform zoneRect = dangerZone.GetComponent<RectTransform>();
            zoneRect.anchoredPosition = position;
            zoneRect.sizeDelta = new Vector2(dangerZoneRadius * 2, dangerZoneRadius * 2);

            // 위험지역 스타일 적용
            Image zoneImage = dangerZone.GetComponent<Image>();
            if (zoneImage != null) zoneImage.color = dangerZoneColor;

            dangerZones.Add(dangerZone);
        }

        // 행성들이 항상 위험지역 위에 표시되도록
        EnsurePlanetsOnTop();
    }

    /// <summary>
    /// 유효 범위 표시기를 초기화합니다.
    /// </summary>
    private void InitializeValidRangeIndicator()
    {
        // 가장 왼쪽에 있는 행성 찾기 (행성이 없으면 기본값 사용)
        if (planets.Count > 0)
        {
            GameObject leftmostPlanet = FindLeftmostPlanet();
            if (leftmostPlanet != null)
            {
                RectTransform planetRect = leftmostPlanet.GetComponent<RectTransform>();
                currentIndicatorPosition = planetRect.anchoredPosition;
            }
        }

        if (validRangeIndicatorPrefab != null)
        {
            // 유효 범위 표시기 초기화 (Instantiate 사용)
            validRangeIndicatorInstance = Instantiate(validRangeIndicatorPrefab, mapContainer);


            // RectTransform 컴포넌트 가져오기 확인
            if (validRangeIndicatorInstance.TryGetComponent(out RectTransform rectTransform))
            {
                // 인스턴스 초기화 (활성화, 위치 설정)
                rectTransform.anchoredPosition = currentIndicatorPosition;
                rectTransform.sizeDelta = new Vector2(maxPlacementDistance * 2, maxPlacementDistance * 2);
                rectTransform.localScale = Vector3.one; // 스케일 조정

                // Raycast Target 비활성화
                Image image = validRangeIndicatorInstance.GetComponent<Image>();
                if (image != null) image.raycastTarget = false;
            }
            else
            {
                Debug.LogError(
                    "The instantiated ValidRangeIndicatorPrefab does not have a RectTransform component!");
            }

            validRangeIndicatorInstance.SetActive(true); // 활성화
        }
        else
        {
            Debug.LogError("ValidRangeIndicatorPrefab is not assigned in the Inspector!");
        }
    }

    /// <summary>
    /// 유효 범위 표시기의 위치를 업데이트합니다.
    /// </summary>
    /// <param name="localPosition">새 위치</param>
    private void UpdateValidRangeIndicator(Vector2 localPosition)
    {
        if (!validRangeIndicatorInstance) return;

        // RectTransform 컴포넌트 가져오기
        RectTransform rectTransform = validRangeIndicatorInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 위치 및 크기 업데이트
            rectTransform.anchoredPosition = localPosition;
            rectTransform.sizeDelta = new Vector2(maxPlacementDistance * 2, maxPlacementDistance * 2);
        }
        else
        {
            Debug.LogError("ValidRangeIndicator instance does not have a RectTransform component.");
        }
    }

    /// <summary>
    /// 맵 요소(행성, 노드, 위험지역)를 초기화합니다.
    /// </summary>
    private void ClearMapContents()
    {
        ClearPlanets();
        ClearNodes();
        ClearDangerZones();

        // 유효 범위 표시기 제거
        if (validRangeIndicatorInstance != null)
        {
            Destroy(validRangeIndicatorInstance);
            validRangeIndicatorInstance = null;
        }

        // 기준점 초기화
        currentIndicatorPosition = Vector2.zero;
    }

    /// <summary>
    /// 기존 행성들을 제거합니다.
    /// </summary>
    private void ClearPlanets()
    {
        // 기존 행성 제거
        foreach (GameObject planet in planets)
            Destroy(planet);

        planets.Clear();
        startPlanet = null;
        endPlanet = null;
    }

    /// <summary>
    /// 배치된 노드를 초기화합니다.
    /// </summary>
    private void ClearNodes()
    {
        // 기존 노드 제거
        foreach (RectTransform node in placedNodeObjects)
            if (node != null)
                Destroy(node.gameObject);

        placedNodeObjects.Clear();
        nodePlacementCompleted = false;
        pathDangerInfo.Clear();
        pathNodes.Clear();
    }

    /// <summary>
    /// 기존 위험지역을 초기화합니다.
    /// </summary>
    private void ClearDangerZones()
    {
        foreach (GameObject zone in dangerZones)
            if (zone != null)
                Destroy(zone);

        dangerZones.Clear();
    }

    /// <summary>
    /// 행성들이 UI 상에서 가장 위에 표시되도록 합니다.
    /// </summary>
    private void EnsurePlanetsOnTop()
    {
        foreach (GameObject planet in planets)
            planet.transform.SetAsLastSibling();
    }

    #endregion

    #region 헬퍼 메서드

    /// <summary>
    /// 행성을 배치할 수 있는 유효한 위치를 찾습니다.
    /// </summary>
    private Vector2 FindValidPlanetPosition(float width, float height)
    {
        Vector2 position;
        bool validPosition = false;
        int attempts = 0;

        // 최대 시도 횟수만큼 반복
        do
        {
            // 랜덤 위치 생성
            float randomX = UnityEngine.Random.Range(-width / 2 + 5f, width / 2 - 5f);
            float randomY = UnityEngine.Random.Range(-height / 2 + 5f, height / 2 - 5f);
            position = new Vector2(randomX, randomY);

            // 기존 행성들과 충분히 떨어져 있는지 확인
            validPosition = IsFarEnoughFromOtherPlanets(position);

            attempts++;
        } while (!validPosition && attempts < maxPlacementAttempts);

        // 최대 시도 횟수를 초과하면 마지막 위치 사용
        if (!validPosition)
            Debug.LogWarning(
                $"Could not find valid position for planet after {maxPlacementAttempts} attempts. Using last generated position.");

        return position;
    }

    /// <summary>
    /// 해당 위치가 다른 행성과 충분히 떨어져 있는지 확인합니다.
    /// </summary>
    private bool IsFarEnoughFromOtherPlanets(Vector2 position)
    {
        foreach (GameObject planet in planets)
        {
            RectTransform planetRect = planet.GetComponent<RectTransform>();
            float distance = Vector2.Distance(position, planetRect.anchoredPosition);

            // 최소 거리보다 가까우면 false 반환
            if (distance < minimumDistanceBetweenPlanets) return false;
        }

        // 모든 행성과 충분히 떨어져 있으면 true 반환
        return true;
    }

    /// <summary>
    /// 위험지역의 유효 위치를 찾습니다.
    /// </summary>
    private Vector2 FindValidDangerZonePosition(float width, float height)
    {
        Vector2 position;
        bool validPosition = false;
        int attempts = 0;

        do
        {
            // 랜덤 위치 생성
            float randomX = UnityEngine.Random.Range(-width / 2 + dangerZoneRadius, width / 2 - dangerZoneRadius);
            float randomY = UnityEngine.Random.Range(-height / 2 + dangerZoneRadius, height / 2 - dangerZoneRadius);
            position = new Vector2(randomX, randomY);

            // 행성들과 다른 위험지역으로부터 충분히 떨어져 있는지 확인
            validPosition = IsFarEnoughFromOtherObjects(position);

            attempts++;
        } while (!validPosition && attempts < maxPlacementAttempts);

        return position;
    }

    /// <summary>
    /// 해당 위치가 행성 또는 위험지역과 충돌하지 않는지 확인합니다.
    /// </summary>
    private bool IsFarEnoughFromOtherObjects(Vector2 position)
    {
        // 행성들과의 거리 확인
        foreach (GameObject planet in planets)
        {
            RectTransform planetRect = planet.GetComponent<RectTransform>();
            float distance = Vector2.Distance(position, planetRect.anchoredPosition);

            if (distance < minimumDistanceBetweenPlanets + dangerZoneRadius)
                return false;
        }

        // 기존 위험지역들과의 거리 확인
        foreach (GameObject zone in dangerZones)
        {
            RectTransform zoneRect = zone.GetComponent<RectTransform>();
            float distance = Vector2.Distance(position, zoneRect.anchoredPosition);

            if (distance < dangerZoneRadius * 2)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 주어진 위치가 위험지역 안에 있는지 확인합니다.
    /// </summary>
    private bool IsPositionInDangerZone(Vector2 position)
    {
        foreach (GameObject zone in dangerZones)
        {
            RectTransform zoneRect = zone.GetComponent<RectTransform>();
            float distance = Vector2.Distance(position, zoneRect.anchoredPosition);

            if (distance <= dangerZoneRadius)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 가장 왼쪽에 있는 행성을 찾습니다.
    /// </summary>
    private GameObject FindLeftmostPlanet()
    {
        if (planets.Count == 0) return null;

        GameObject leftmostPlanet = planets[0];
        float minX = leftmostPlanet.GetComponent<RectTransform>().anchoredPosition.x;

        foreach (GameObject planet in planets)
        {
            float x = planet.GetComponent<RectTransform>().anchoredPosition.x;
            if (x < minX)
            {
                minX = x;
                leftmostPlanet = planet;
            }
        }

        return leftmostPlanet;
    }

    /// <summary>
    /// 가장 오른쪽에 있는 행성을 찾습니다.
    /// </summary>
    private GameObject FindRightmostPlanet()
    {
        if (planets.Count == 0) return null;

        GameObject rightmostPlanet = planets[0];
        float maxX = rightmostPlanet.GetComponent<RectTransform>().anchoredPosition.x;

        foreach (GameObject planet in planets)
        {
            float x = planet.GetComponent<RectTransform>().anchoredPosition.x;
            if (x > maxX)
            {
                maxX = x;
                rightmostPlanet = planet;
            }
        }

        return rightmostPlanet;
    }

    /// <summary>
    /// 주어진 위치가 현재 배치 기준점으로부터 유효한 거리인지 확인합니다.
    /// </summary>
    private bool IsValidPlacement(Vector2 position)
    {
        // 현재 기준점으로부터의 거리가 최대 배치 거리 이내인지 확인
        return Vector2.Distance(currentIndicatorPosition, position) <= maxPlacementDistance;
    }

    /// <summary>
    /// 실제 노드를 생성하여 UI에 배치합니다.
    /// </summary>
    private void CreateNodeUI(Vector2 position)
    {
        // 노드 생성
        GameObject nodeObj = Instantiate(nodePrefab, mapContainer);
        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        placedNodeObjects.Add(rectTransform);
    }

    #endregion

    #region 경로 완료 처리

    /// <summary>
    /// 노드 배치가 완료되었을 때 호출되며, 최종 경로를 생성합니다.
    /// </summary>
    private void CompletePlacementMap()
    {
        if (nodePlacementCompleted) return;

        // 노드 배치 완료 상태로 설정
        nodePlacementCompleted = true;

        // 경로 완성 (처음과 끝에 출발/도착 행성 추가)
        List<Vector2> completePath = CollectNodePath();

        // 경로 완료 이벤트 발생
        OnPathCompleted?.Invoke(completePath, pathDangerInfo);
    }

    /// <summary>
    /// 현재까지의 배치된 노드 경로를 수집합니다.
    /// </summary>
    private List<Vector2> CollectNodePath()
    {
        List<Vector2> path = new();

        // 시작 행성 추가 (항상 첫 번째로)
        if (startPlanet != null)
        {
            Vector2 startPos = startPlanet.GetComponent<RectTransform>().anchoredPosition;

            // 시작 행성이 아직 경로에 없으면 추가
            if (pathNodes.Count == 0 || Vector2.Distance(pathNodes[0], startPos) > 1f)
            {
                path.Add(startPos);

                // 시작 행성이 위험지역인지도 확인
                bool startIsDangerous = IsPositionInDangerZone(startPos);
                if (pathDangerInfo.Count == 0 || pathDangerInfo.Count < path.Count)
                    pathDangerInfo.Insert(0, startIsDangerous);
            }
        }

        // 중간 노드들 추가
        path.AddRange(pathNodes);

        // 마지막으로 클릭한 위치가 도착 행성이 아니면 도착 행성 추가
        if (endPlanet != null)
        {
            Vector2 endPos = endPlanet.GetComponent<RectTransform>().anchoredPosition;

            // 도착 행성이 아직 경로에 없으면 추가
            if (path.Count == 0 || Vector2.Distance(path[path.Count - 1], endPos) > 1f)
            {
                path.Add(endPos);

                // 도착 행성이 위험지역인지도 확인
                bool endIsDangerous = IsPositionInDangerZone(endPos);
                if (pathDangerInfo.Count < path.Count)
                    pathDangerInfo.Add(endIsDangerous);
            }
        }

        // 경로와 위험 정보의 길이가 일치하는지 확인
        while (pathDangerInfo.Count < path.Count)
            pathDangerInfo.Add(false);

        return path;
    }

    #endregion

    /// <summary>
    /// Planet.cs에서 행성 클릭 시 호출되는 메서드입니다.
    /// </summary>
    /// <param name="planet">클릭된 행성</param>
    public void OnPlanetClickedFromComponent(GameObject planet)
    {
        // 기존 OnPlanetClicked 메서드와 동일한 내용
        OnPlanetClicked(planet);
    }
}
