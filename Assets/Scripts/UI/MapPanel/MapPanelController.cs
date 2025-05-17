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

    /// <summary>
    /// 월드 맵이 초기화 되었는지 여부
    /// </summary>
    private bool isMapInitialized = false;


    [Header("열려야 되는 위치")] [SerializeField] private Transform openedPosition;
    [Header("열리는 속도")] [SerializeField] private float slideSpeed = 0.1f;

    private Vector3 closedPosition;
    private Coroutine slideCoroutine;
    private bool isOpen = false;
    private GameObject currentPanel = null;

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
    }

    #endregion

    #region 월드 패널 설정

    private void InitializeWorldPanel()
    {
    }

    private void DrawWorldMap()
    {
        // 이미 초기화되었다면 건너뛰기
        if (isMapInitialized)
            return;

        // TODO : 테스트용 키 삭제코드.
        ES3.DeleteKey("planetList");
        GameManager.Instance.LoadPlanets();

        List<PlanetData> planetDatas = GameManager.Instance.PlanetDataList;
        RectTransform contentRect = worldPanelContent.GetComponent<RectTransform>();

        // 각 행성 데이터에 대해 행성 오브젝트 생성 및 배치
        foreach (PlanetData planetData in planetDatas)
        {
            // 행성 프리팹 인스턴스 생성
            Planet planetInstance = Instantiate(planetPrefab, worldPanelContent.transform).GetComponent<Planet>();

            planetInstance.SetPlanetData(planetData);
            RectTransform planetRect = planetInstance.GetComponent<RectTransform>();

            // 정규화된 좌표(0~1)를 실제 UI 좌표로 변환
            Vector2 mapPosition = new(
                planetData.normalizedPosition.x * contentRect.rect.width,
                planetData.normalizedPosition.y * contentRect.rect.height
            );

            // 앵커를 좌하단(0,0)으로 설정하여 좌표 계산을 단순화
            planetRect.anchorMin = Vector2.zero;
            planetRect.anchorMax = Vector2.zero;
            planetRect.pivot = new Vector2(0.5f, 0.5f); // 중앙 기준으로 배치

            // 위치 설정
            planetRect.anchoredPosition = mapPosition;

            // 크기 설정 (정규화된 크기를 실제 크기로 변환)
            float planetSize = Constants.Planets.PlanetSize *
                               Mathf.Min(contentRect.rect.width, contentRect.rect.height);
            planetRect.sizeDelta = new Vector2(planetSize, planetSize);
        }

        isMapInitialized = true;
    }

    #endregion
}
