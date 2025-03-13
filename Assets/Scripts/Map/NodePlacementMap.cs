using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NodePlacementMap : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private RectTransform mapContainer;

    [SerializeField] private GameObject validRangeIndicatorPrefab; // 유효 범위 표시기 프리팹
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject planetPrefab; // 행성을 표시할 프리팹
    [SerializeField] private GameObject tooltipPrefab; // 행성 툴팁 프리팹

    [Header("Settings")] [SerializeField] private float maxPlacementDistance = 200f;

    [Header("Random Placement Settings")] [SerializeField]
    private int numberOfPlanets = 20; // 행성 개수

    [SerializeField] private int maxPlacementAttempts = 30; // 위치 시도 최대 횟수
    [SerializeField] private float minimumDistanceBetweenPlanets = 100f; // 행성 간 최소 거리

    // 위험지역 설정 추가
    [Header("Danger Zone Settings")] [SerializeField]
    private GameObject dangerZonePrefab; // 위험지역 표시 프리팹

    [SerializeField] private int numDangerZones = 3; // 생성할 위험지역 개수
    [SerializeField] private float dangerZoneRadius = 100f; // 위험지역 반경
    [SerializeField] private Color dangerZoneColor = new(1f, 0f, 0f, 0.3f); // 위험지역 색상

    // Planet Names Data
    [Header("Planet Names")] [SerializeField]
    private TextAsset planetNamesJson; // JSON 파일 참조

    private readonly List<RectTransform> placedNodeObjects = new(); // 배치된 노드 목록
    private readonly List<GameObject> planets = new(); // 생성된 행성 리스트
    private readonly List<GameObject> dangerZones = new(); // 생성된 위험지역 목록

    private Vector2 currentIndicatorPosition = Vector2.zero; // 현재 유효 범위 기준점
    private GameObject endPlanet; // 도착 행성

    // 각 경로 노드의 위험 여부
    private readonly List<bool> pathNodeIsDangerous = new();

    // EventSystem 참조 추가
    private EventSystem eventSystem;

    // 노드 배치 관련 변수
    private bool nodePlacementCompleted;
    private GameObject startPlanet; // 출발 행성
    private GameObject validRangeIndicatorInstance; // 생성된 인스턴스

    // Planet names data container
    private PlanetNamesData planetNames;

    private void Awake()
    {
        // EventSystem 참조 가져오기
        eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null) Debug.LogError("EventSystem not found in the scene!");

        // Load planet names if available
        LoadPlanetNamesData();

        // 행성 생성 후 초기화
        GenerateRandomPlanets();

        // 위험지역 생성
        GenerateDangerZones();
        EnsurePlanetsOnTop();

        InitializeValidRangeIndicator();
        EnsurePlanetsOnTop();
    }

    private void LoadPlanetNamesData()
    {
        // Load planet names from JSON if available
        if (planetNamesJson != null)
        {
            planetNames = JsonUtility.FromJson<PlanetNamesData>(planetNamesJson.text);
            Debug.Log($"Loaded {planetNames.prefixes.Length} planet name prefixes from JSON");
        }
        else
        {
            // Default names if JSON not found
            planetNames = new PlanetNamesData
            {
                prefixes = new string[]
                {
                    "Kepler", "Gliese", "HD", "Trappist", "Wolf", "Ross", "Proxima", "TOI", "WASP", "TYC"
                }
            };
            Debug.LogWarning("Planet names JSON not found, using default names");
        }
    }

    private void Update()
    {
        // 유효 범위 표시기 계속 갱신
        if (!validRangeIndicatorInstance) UpdateValidRangeIndicator(currentIndicatorPosition);
    }

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
            }
            else
            {
                Debug.LogError("The instantiated ValidRangeIndicatorPrefab does not have a RectTransform component!");
            }

            validRangeIndicatorInstance.SetActive(true); // 활성화
        }
        else
        {
            Debug.LogError("ValidRangeIndicatorPrefab is not assigned in the Inspector!");
        }
    }

    private GameObject FindLeftmostPlanet()
    {
        if (planets.Count == 0) return null;

        // x좌표가 가장 작은(왼쪽) 행성 찾기
        return planets.OrderBy(p => p.GetComponent<RectTransform>().anchoredPosition.x).FirstOrDefault();
    }

    private GameObject FindRightmostPlanet()
    {
        if (planets.Count == 0) return null;

        // x좌표가 가장 큰(오른쪽) 행성 찾기
        return planets.OrderByDescending(p => p.GetComponent<RectTransform>().anchoredPosition.x).FirstOrDefault();
    }

    private Vector2 FindValidPlanetPosition(float width, float height)
    {
        Vector2 position;
        bool validPosition = false;
        int attempts = 0;

        // 최대 시도 횟수만큼 반복
        do
        {
            // 랜덤 위치 생성
            float randomX = Random.Range(-width / 2 + 5f, width / 2 - 5f);
            float randomY = Random.Range(-height / 2 + 5f, height / 2 - 5f);
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

    // 위험지역 생성 메서드
    private void GenerateDangerZones()
    {
        // 기존 위험지역 제거
        ClearDangerZones();

        RectTransform rectTransform = mapContainer.GetComponent<RectTransform>();
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
    }

    private Vector2 FindValidDangerZonePosition(float width, float height)
    {
        Vector2 position;
        bool validPosition = false;
        int attempts = 0;

        do
        {
            // 랜덤 위치 생성
            float randomX = Random.Range(-width / 2 + dangerZoneRadius, width / 2 - dangerZoneRadius);
            float randomY = Random.Range(-height / 2 + dangerZoneRadius, height / 2 - dangerZoneRadius);
            position = new Vector2(randomX, randomY);

            // 행성들과 다른 위험지역으로부터 충분히 떨어져 있는지 확인
            validPosition = IsFarEnoughFromOtherObjects(position);

            attempts++;
        } while (!validPosition && attempts < maxPlacementAttempts);

        return position;
    }

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

    private void ClearDangerZones()
    {
        foreach (GameObject zone in dangerZones)
            if (zone != null)
                Destroy(zone);

        dangerZones.Clear();
    }

    // 위치가 위험지역 안에 있는지 확인하는 함수
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

    // Generate random planet name using available prefixes
    private string GenerateRandomPlanetName()
    {
        if (planetNames == null || planetNames.prefixes == null || planetNames.prefixes.Length == 0)
            return "Planet-" + Random.Range(100, 10000);

        string prefix = planetNames.prefixes[Random.Range(0, planetNames.prefixes.Length)];
        int number = Random.Range(100, 10000);

        return $"{prefix}-{number}";
    }

    private void EnsurePlanetsOnTop()
    {
        foreach (GameObject planet in planets) planet.transform.SetAsLastSibling();
    }

    public void GenerateRandomPlanets()
    {
        ClearPlanets();
        ClearNodes();

        RectTransform rectTransform = mapContainer.GetComponent<RectTransform>();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        for (int i = 0; i < numberOfPlanets; i++)
        {
            GameObject planet = Instantiate(planetPrefab, mapContainer);
            planet.name = $"Planet_{i}";

            // 적절한 위치 찾기 (다른 행성과 충분히 떨어진 위치)
            Vector2 newPosition = FindValidPlanetPosition(width, height);

            RectTransform planetRect = planet.GetComponent<RectTransform>();
            planetRect.anchoredPosition = newPosition;
            planetRect.SetAsLastSibling();

            // Add Planet component to the GameObject if not already present
            Planet planetComponent = planet.GetComponent<Planet>();
            if (planetComponent == null) planetComponent = planet.AddComponent<Planet>();

            // Initialize planet data
            planetComponent.planetName = GenerateRandomPlanetName();
            planetComponent.fuelPrice = Random.Range(25f, 75f);
            planetComponent.hasEvent = Random.value > 0.7f; // 30% chance to have an event
            planetComponent.hasQuest = Random.value > 0.8f; // 20% chance to have a quest
            // Assign tooltip prefab if not already set
            if (tooltipPrefab != null &&
                planetComponent.GetType().GetField("tooltipPrefab").GetValue(planetComponent) == null)
                planetComponent.GetType().GetField("tooltipPrefab").SetValue(planetComponent, tooltipPrefab);

            planets.Add(planet);
        }

        // 시작 행성과 도착 행성 설정
        startPlanet = FindLeftmostPlanet();
        endPlanet = FindRightmostPlanet();

        // Calculate distances for all planets from the start planet
        if (startPlanet != null)
        {
            Vector2 startPosition = startPlanet.GetComponent<RectTransform>().anchoredPosition;
            foreach (GameObject planet in planets)
            {
                Planet planetComponent = planet.GetComponent<Planet>();
                if (planetComponent != null) planetComponent.SetLightYearsFromUnityDistance(startPosition);
            }
        }

        // 초기 노드 배치 상태 재설정
        nodePlacementCompleted = false;
        pathNodeIsDangerous.Clear();
    }

    private void ClearPlanets()
    {
        // 기존 행성 제거
        foreach (GameObject planet in planets) Destroy(planet);

        planets.Clear();
        startPlanet = null;
        endPlanet = null;
    }

    private void ClearNodes()
    {
        // 기존 노드 제거
        foreach (RectTransform node in placedNodeObjects)
            if (node != null)
                Destroy(node.gameObject);

        placedNodeObjects.Clear();
        nodePlacementCompleted = false;
        pathNodeIsDangerous.Clear();
    }

    // NodePlacementMap.cs에 새 메서드 추가
    public void OnPlanetClickedFromComponent(GameObject planet)
    {
        // 기존 OnPlanetClicked 메서드와 동일한 내용
        OnPlanetClicked(planet);
    }

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
            pathNodeIsDangerous.Add(isDangerous);

            // ValidRangeIndicator 위치 업데이트
            UpdateValidRangeIndicator(currentIndicatorPosition);

            // 행성 클릭 시 노드 배치 종료
            StopNodePlacement();
        }
    }

    public void StopNodePlacement()
    {
        if (nodePlacementCompleted) return;

        nodePlacementCompleted = true;

        // 배치된 노드 경로 수집
        List<Vector2> nodePath = CollectNodePath();

        // MapSystemManager에 노드 경로 및 위험 정보 전달
        if (MapSystemManager.Instance != null)
        {
            // 경로 정보 전달
            MapSystemManager.Instance.SetPathNodes(nodePath);

            // 위험지역 정보 전달
            MapSystemManager.Instance.SetPathDangerInfo(pathNodeIsDangerous);

            // 이벤트 트리 맵으로 전환
            MapSystemManager.Instance.SwitchToEventTreeMap();
        }
        else
        {
            Debug.LogError("MapSystemManager reference not set in NodePlacementMap!");
        }
    }

    private List<Vector2> CollectNodePath()
    {
        List<Vector2> path = new();

        // 시작 행성 추가
        if (startPlanet != null)
        {
            Vector2 startPos = startPlanet.GetComponent<RectTransform>().anchoredPosition;
            path.Add(startPos);

            // 시작 행성이 위험지역인지 확인 (pathNodeIsDangerous가 비었다면)
            if (pathNodeIsDangerous.Count == 0)
            {
                bool isDangerous = IsPositionInDangerZone(startPos);
                pathNodeIsDangerous.Add(isDangerous);
            }
        }

        // 배치된 모든 중간 노드 추가
        foreach (RectTransform nodeTransform in placedNodeObjects)
        {
            Vector2 nodePos = nodeTransform.anchoredPosition;
            path.Add(nodePos);

            // 노드가 위험지역인지 확인 (아직 처리되지 않았으면)
            if (pathNodeIsDangerous.Count < path.Count)
            {
                bool isDangerous = IsPositionInDangerZone(nodePos);
                pathNodeIsDangerous.Add(isDangerous);
            }
        }

        // 마지막으로 클릭한 행성의 위치가 이미 경로에 있는지 확인
        Vector2 lastPosition = currentIndicatorPosition;
        if (path.Count == 0 || Vector2.Distance(path[path.Count - 1], lastPosition) > 1f)
        {
            // 마지막 위치가 경로에 없으면 추가
            path.Add(lastPosition);

            // 마지막 위치가 위험지역인지 확인
            if (pathNodeIsDangerous.Count < path.Count)
            {
                bool isDangerous = IsPositionInDangerZone(lastPosition);
                pathNodeIsDangerous.Add(isDangerous);
            }
        }

        // 경로와 위험 정보의 길이가 일치하는지 확인
        while (pathNodeIsDangerous.Count < path.Count) pathNodeIsDangerous.Add(false);

        return path;
    }

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

    private void TryPlaceNode(Vector2 position)
    {
        if (IsValidPlacement(position))
        {
            CreateNodeUI(position); // 새 노드 생성

            // 노드가 위험지역 안에 있는지 체크
            bool isDangerous = IsPositionInDangerZone(position);
            pathNodeIsDangerous.Add(isDangerous);

            // 노드 기준으로 유효 반경 표시기 업데이트
            currentIndicatorPosition = position; // 현재 위치를 갱신
            UpdateValidRangeIndicator(currentIndicatorPosition);
        }
    }

    private void CreateNodeUI(Vector2 position)
    {
        GameObject nodeObj = Instantiate(nodePrefab, mapContainer); // 노드 생성
        RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position; // 노드 위치 설정
        placedNodeObjects.Add(rectTransform); // 노드 목록에 추가
    }

    private bool IsValidPlacement(Vector2 position)
    {
        Vector2 referencePosition = currentIndicatorPosition;

        return Vector2.Distance(referencePosition, position) <= maxPlacementDistance;
    }

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
}
