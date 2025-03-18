using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSystemManager : MonoBehaviour
{
    [Header("Map Systems")] [SerializeField]
    private EventTreeMap eventTreeMapPrefab; // 프리팹으로 변경

    [SerializeField] private NodePlacementMap placementMapPrefab; // 프리팹으로 변경
    private EventTreeMap eventTreeMap; // 실제 인스턴스
    private NodePlacementMap placementMap; // 실제 인스턴스

    private MapType currentMapType = MapType.NodePlacement;

    [SerializeField] private Canvas mapCanvas; // 맵 전용 캔버스

    // 경로 정보를 저장할 변수
    private List<Vector2> pathNodes = new();

    // 경로의 위험 정보를 저장할 변수
    private List<bool> pathDangerInfo = new();

    // 이벤트 정의
    public delegate void RouteCompletedHandler(List<Vector2> path, List<bool> dangerInfo);

    public event RouteCompletedHandler OnRoutePlanningCompleted;

    // 맵 상태 변경 이벤트
    public delegate void MapStateChangedHandler(MapType newType);

    public event MapStateChangedHandler OnMapStateChanged;

    public static MapSystemManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 맵 캔버스가 없으면 생성
        if (mapCanvas == null)
        {
            GameObject canvasObj = new("Map Canvas");
            mapCanvas = canvasObj.AddComponent<Canvas>();
            mapCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 맵 인스턴스 생성하고 캔버스에 배치
        if (eventTreeMap == null && eventTreeMapPrefab != null)
        {
            GameObject eventTreeObj = Instantiate(eventTreeMapPrefab.gameObject, mapCanvas.transform);
            eventTreeMap = eventTreeObj.GetComponent<EventTreeMap>();
            eventTreeObj.SetActive(false); // 초기에는 비활성화
        }

        if (placementMap == null && placementMapPrefab != null)
        {
            Debug.Log(placementMapPrefab.gameObject);
            Debug.Log(mapCanvas.transform);
            GameObject placementObj = Instantiate(placementMapPrefab.gameObject, mapCanvas.transform);
            placementMap = placementObj.GetComponent<NodePlacementMap>();
            placementObj.SetActive(false); // 초기에는 비활성화
        }

        SwitchToPlacementMap();
    }

    public void SwitchToEventTreeMap()
    {
        placementMap.gameObject.SetActive(false);
        eventTreeMap.gameObject.SetActive(true);

        MapType oldType = currentMapType;
        currentMapType = MapType.EventTree;

        // 이미 생성된 경로가 있다면 트리 생성
        if (pathNodes.Count > 0)
            // 위험 정보와 함께 트리 생성
            eventTreeMap.GenerateTreeFromPath(pathNodes, pathDangerInfo);

        // 이벤트 발생
        OnMapStateChanged?.Invoke(currentMapType);
    }

    public void SwitchToPlacementMap()
    {
        eventTreeMap.gameObject.SetActive(false);
        placementMap.gameObject.SetActive(true);

        MapType oldType = currentMapType;
        currentMapType = MapType.NodePlacement;

        // 이벤트 발생
        OnMapStateChanged?.Invoke(currentMapType);
    }

    // NodePlacementMap으로부터 경로 정보를 받아 저장
    public void SetPathNodes(List<Vector2> nodes)
    {
        pathNodes = new List<Vector2>(nodes);

        // 이벤트 발생 (위험 정보가 있는 경우)
        if (pathDangerInfo.Count > 0 && OnRoutePlanningCompleted != null)
            OnRoutePlanningCompleted(pathNodes, pathDangerInfo);
    }

    // NodePlacementMap으로부터 위험 정보를 받아 저장
    public void SetPathDangerInfo(List<bool> dangerInfo)
    {
        pathDangerInfo = new List<bool>(dangerInfo);

        // 이벤트 발생 (경로 정보가 있는 경우)
        if (pathNodes.Count > 0 && OnRoutePlanningCompleted != null)
            OnRoutePlanningCompleted(pathNodes, pathDangerInfo);
    }

    /// <summary>
    /// 모든 맵 UI를 숨기는 메서드
    /// </summary>
    public void HideMapUI()
    {
        if (placementMap != null)
            placementMap.gameObject.SetActive(false);

        if (eventTreeMap != null)
            eventTreeMap.gameObject.SetActive(false);
    }

    /// <summary>
    /// 워프 UI 컨트롤러와의 통합을 위한 메서드
    /// </summary>
    public void SynchronizeWithUIController(WarpUIController uiController)
    {
        if (uiController == null)
        {
            Debug.LogError("Cannot synchronize with null UI Controller");
            return;
        }

        // UI 컨트롤러에 이벤트 데이터 전달
        if (pathNodes.Count > 0 && pathDangerInfo.Count > 0)
            uiController.InitializeEventSelectionUI(pathNodes, pathDangerInfo);
    }
}
