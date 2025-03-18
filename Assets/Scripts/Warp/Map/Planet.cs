using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum SpeciesType
{
    Aquatic, // 어인
    Avian, // 조류
    Ancient, // 선인
    Humanoid, // 인간형
    Amorphous // 부정형
}

public class Planet : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Planet Info")] public string planetName;
    public float distanceInLightYears;
    public bool hasEvent;
    public bool hasQuest;
    public float fuelPrice;

    [Header("Species Info")] public SpeciesType dominantSpecies;

    private float lastPositionUpdateTime = 0f;
    private float positionUpdateInterval = 0.2f; // 0.2초마다 위치 업데이트


    [Header("References")] [SerializeField]
    public GameObject tooltipPrefab;

    private GameObject activeTooltip;

    // 노드 배치 함수 참조 델리게이트
    public System.Action<GameObject> OnPlanetClickedCallback;

    private void Update()
    {
        // 툴팁 활성화 상태에서만 위치 업데이트
        if (activeTooltip != null)
            // 일정 시간마다 툴팁 위치 업데이트 (성능 최적화)
            if (Time.time - lastPositionUpdateTime >= positionUpdateInterval)
            {
                PositionTooltipOnMap();
                lastPositionUpdateTime = Time.time;
            }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 간단하게 툴팁 표시만 처리
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 간단하게 툴팁 숨기기만 처리
        HideTooltip();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Planet clicked: " + planetName);

        // 클릭 시 툴팁 숨기기
        HideTooltip();

        // NodePlacementMap 찾기
        NodePlacementMap nodeMap = FindObjectOfType<NodePlacementMap>();
        if (nodeMap != null)
            // 행성이 클릭되었을 때 노드맵에 알림
            nodeMap.OnPlanetClickedFromComponent(gameObject);
    }


    private void HideTooltip()
    {
        if (activeTooltip != null)
        {
            Destroy(activeTooltip);
            activeTooltip = null;
        }
    }

    private void Start()
    {
        // If name is not set yet, generate a random name
        if (string.IsNullOrEmpty(planetName))
        {
            // Randomly assign a species type if not set
            if (!System.Enum.IsDefined(typeof(SpeciesType), dominantSpecies))
                dominantSpecies = (SpeciesType)Random.Range(0, System.Enum.GetValues(typeof(SpeciesType)).Length);

            GenerateRandomName();
        }

        // Initialize random fuel price
        fuelPrice = Random.Range(25f, 75f);

        // EventTrigger 컴포넌트 추가 (없는 경우)
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = gameObject.AddComponent<EventTrigger>();

        // 포인터 진입 이벤트 추가
        EventTrigger.Entry enterEntry = new();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { ShowTooltip(); });
        eventTrigger.triggers.Add(enterEntry);

        // 포인터 이탈 이벤트 추가
        EventTrigger.Entry exitEntry = new();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { HideTooltip(); });
        eventTrigger.triggers.Add(exitEntry);

        EventTrigger.Entry clickEntry = new();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) =>
        {
            Debug.Log("Planet clicked via EventTrigger: " + planetName);
            OnPlanetClickedCallback?.Invoke(gameObject);
        });
        eventTrigger.triggers.Add(clickEntry);
    }

    // Generate a random name based on our pattern
    private void GenerateRandomName()
    {
        // Get prefix based on dominant species
        string prefix = GetSpeciesPrefix(dominantSpecies);

        // Generate random number (3-4 digits)
        int randomNumber = Random.Range(100, 10000);

        // Generate random capital letter
        char randomLetter = (char)Random.Range('A', 'Z' + 1);

        // Combine to form planet name: PREFIX-NUMBER+LETTER
        planetName = $"{prefix}-{randomNumber}{randomLetter}";
    }

    // Get prefix based on species type
    private string GetSpeciesPrefix(SpeciesType species)
    {
        switch (species)
        {
            case SpeciesType.Aquatic: // 어인
                return "SIS";
            case SpeciesType.Avian: // 조류
                return "CCK";
            case SpeciesType.Ancient: // 선인
                return "ICM";
            case SpeciesType.Humanoid: // 인간형
                return "RCE";
            case SpeciesType.Amorphous: // 부정형
                return "KTL";
            default:
                return "UNK"; // Unknown, fallback
        }
    }

    // Calculate light years based on Unity distance
    public void SetLightYearsFromUnityDistance(Vector2 referencePosition)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float unityDistance = Vector2.Distance(rectTransform.anchoredPosition, referencePosition);

        // Convert Unity distance to light years (1 Unity unit = 1 light year in this example)
        distanceInLightYears = unityDistance;
    }

    private void ShowTooltip()
    {
        if (tooltipPrefab == null)
        {
            Debug.LogWarning("Tooltip prefab not assigned to Planet object!");
            return;
        }

        // 이미 툴팁이 활성화되어 있다면 다시 생성하지 않음
        if (activeTooltip != null) return;

        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene for tooltip!");
            return;
        }

        // 툴팁 생성
        activeTooltip = Instantiate(tooltipPrefab, canvas.transform);

        // 툴팁 컴포넌트 설정
        PlanetTooltip tooltip = activeTooltip.GetComponent<PlanetTooltip>();
        if (tooltip != null)
            tooltip.SetupTooltip(this);
        else
            Debug.LogError("PlanetTooltip component not found on tooltip prefab!");

        // 레이캐스트 타겟 비활성화 (툴팁 자체가 이벤트를 가로채지 않도록)
        Image[] images = activeTooltip.GetComponentsInChildren<Image>();
        foreach (Image img in images) img.raycastTarget = false;

        // TextMeshPro 텍스트도 레이캐스트 타겟 비활성화
        TMPro.TextMeshProUGUI[] texts = activeTooltip.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        foreach (TMPro.TextMeshProUGUI text in texts) text.raycastTarget = false;

        // 툴팁 위치 설정 (맵 좌표계 기준)
        PositionTooltipOnMap();
    }

    private void PositionTooltipOnMap()
    {
        if (activeTooltip == null) return;

        RectTransform tooltipRect = activeTooltip.GetComponent<RectTransform>();
        RectTransform planetRect = GetComponent<RectTransform>();

        // 행성의 맵상 위치
        Vector2 planetPosition = planetRect.anchoredPosition;

        // 더 가까운 거리로 설정
        float offsetDistance = 5f; // 행성으로부터의 거리 감소

        // 행성 사이즈
        float planetRadius = Mathf.Max(planetRect.rect.width, planetRect.rect.height) / 2f;

        // 툴팁 크기 계산 (레이아웃 리빌드 후)
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
        Vector2 tooltipSize = tooltipRect.rect.size;

        // 기본 오프셋 초기화
        Vector2 tooltipOffset = Vector2.zero;

        // 맵의 경계 확인 (맵 컨테이너 찾기)
        RectTransform mapContainer = planetRect.parent as RectTransform;
        if (mapContainer != null)
        {
            // 맵 경계
            float mapWidth = mapContainer.rect.width;
            float mapHeight = mapContainer.rect.height;

            // 맵 중심에서의 상대적 위치 (-0.5 ~ 0.5)
            float relativeX = planetPosition.x / mapWidth;
            float relativeY = planetPosition.y / mapHeight;

            // 행성이 맵의 어느 부분에 있는지에 따라 툴팁 방향 결정
            // 우선순위: 오른쪽 > 왼쪽 > 위쪽 > 아래쪽

            // X축 오프셋 결정
            if (relativeX < 0.3f) // 맵의 왼쪽 부분
            {
                // 행성 오른쪽에 툴팁 배치
                tooltipOffset.x = planetRadius + offsetDistance;
                // 툴팁의 왼쪽 가장자리가 행성 오른쪽에 위치하도록
                tooltipRect.pivot = new Vector2(0f, 0.5f); // 왼쪽 중앙 피벗
            }
            else if (relativeX > 0.7f) // 맵의 오른쪽 부분
            {
                // 행성 왼쪽에 툴팁 배치
                tooltipOffset.x = -planetRadius - offsetDistance;
                // 툴팁의 오른쪽 가장자리가 행성 왼쪽에 위치하도록
                tooltipRect.pivot = new Vector2(1f, 0.5f); // 오른쪽 중앙 피벗
            }
            else // 맵의 중앙 X 영역
            {
                // Y축 위치에 따라 결정
                if (relativeY < 0.3f) // 맵의 아래쪽
                {
                    // 행성 위쪽에 툴팁 배치
                    tooltipOffset.y = planetRadius + offsetDistance;
                    // 툴팁의 아래쪽 가장자리가 행성 위쪽에 위치하도록
                    tooltipRect.pivot = new Vector2(0.5f, 0f); // 중앙 아래 피벗
                    // X 오프셋은 없음
                    tooltipOffset.x = 0;
                }
                else // 중앙 또는 위쪽
                {
                    // 행성 아래쪽에 툴팁 배치
                    tooltipOffset.y = -planetRadius - offsetDistance;
                    // 툴팁의 위쪽 가장자리가 행성 아래쪽에 위치하도록
                    tooltipRect.pivot = new Vector2(0.5f, 1f); // 중앙 위 피벗
                    // X 오프셋은 없음
                    tooltipOffset.x = 0;
                }
            }

            // Y축 오프셋이 설정되지 않은 경우 (X축만 설정된 경우)
            if (Mathf.Approximately(tooltipOffset.y, 0f))
            {
                // 맵에서의 Y 위치에 따라 약간 위나 아래에 배치
                if (relativeY > 0.5f) // 맵의 위쪽 절반
                {
                    // 약간 아래로 배치
                    tooltipOffset.y = -planetRadius / 2f;
                    tooltipRect.pivot = new Vector2(tooltipRect.pivot.x, 0.75f);
                }
                else // 맵의 아래쪽 절반
                {
                    // 약간 위로 배치
                    tooltipOffset.y = planetRadius / 2f;
                    tooltipRect.pivot = new Vector2(tooltipRect.pivot.x, 0.25f);
                }
            }
        }

        // 툴팁 위치 설정 (행성 위치 + 오프셋)
        tooltipRect.anchoredPosition = planetPosition + tooltipOffset;
    }
}
