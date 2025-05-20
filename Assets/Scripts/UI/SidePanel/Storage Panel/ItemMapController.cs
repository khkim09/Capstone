using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ItemMapController : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private GameObject mapContent;

    [SerializeField] private GameObject planetPrefab;
    [SerializeField] private GameObject currentPlayerIndicatorPrefab;
    private GameObject currentPositionIndicator;
    [SerializeField] private float indicatorRotationSpeed = 30f;

    [SerializeField] private Color upColor = Color.green;
    [SerializeField] private Color downColor = Color.red;
    [SerializeField] private Color sameColor = Color.white;

    private bool isOpen = false;
    private TradingItemData currentItem;

    // Store references to instantiated planet objects
    private Dictionary<int, Planet> planetInstances = new();
    private bool isMapInitialized = false;

    private void Start()
    {
        RectMask2D rectMask = mapContent.GetComponent<RectMask2D>();
        if (rectMask == null) mapContent.AddComponent<RectMask2D>();
    }

    private void OnEnable()
    {
        // 패널이 활성화될 때 isOpen을 true로 설정
        isOpen = true;
    }

    private void OnDisable()
    {
        // 패널이 비활성화될 때 isOpen을 false로 설정
        isOpen = false;
    }

    private void Update()
    {
        // 패널이 열려있고 마우스 왼쪽 버튼이 클릭되었을 때
        if (isOpen && Input.GetMouseButtonDown(0))
            // 클릭된 UI 요소가 현재 패널이 아닌지 체크
            if (!IsClickingOnSelf())
                gameObject.SetActive(false);

        if (currentPositionIndicator != null)
            // 현재 회전에 계속해서 각도 추가
            currentPositionIndicator.transform.Rotate(Vector3.back, indicatorRotationSpeed * Time.deltaTime);
    }

    public void Initialize(TradingItemData tradingItemData)
    {
        currentItem = tradingItemData;

        if (!isMapInitialized)
        {
            // First time initialization - create all planet objects
            CreateWorldMap();
            isMapInitialized = true;
        }

        // Update only what's necessary: prices and player position
        UpdatePriceDisplay();
        UpdatePlayerPosition();
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

    private void CreateWorldMap()
    {
        // Clear any existing planet objects
        foreach (Transform child in mapContent.transform)
            if (child.GetComponent<Planet>() != null)
                Destroy(child.gameObject);

        planetInstances.Clear();
        GameManager.Instance.LoadWorldMap();

        List<PlanetData> planetDatas = GameManager.Instance.PlanetDataList;
        RectTransform contentRect = mapContent.GetComponent<RectTransform>();

        foreach (PlanetData planetData in planetDatas)
        {
            Planet planetInstance = Instantiate(planetPrefab, mapContent.transform).GetComponent<Planet>();
            planetInstance.SetPlanetData(planetData);

            RectTransform planetRect = planetInstance.GetComponent<RectTransform>();
            SetupMapObject(
                planetRect,
                planetData.normalizedPosition,
                Constants.Planets.PlanetSize,
                contentRect
            );

            // Store reference to the planet instance
            planetInstances[planetData.planetId] = planetInstance;
        }

        // Create player position indicator
        if (currentPositionIndicator == null)
            currentPositionIndicator = Instantiate(currentPlayerIndicatorPrefab, mapContent.transform);
    }

    private void UpdatePriceDisplay()
    {
        if (currentItem == null) return;

        List<PlanetData> planetDatas = GameManager.Instance.PlanetDataList;

        foreach (PlanetData planetData in planetDatas)
        {
            if (!planetInstances.TryGetValue(planetData.planetId, out Planet planetInstance))
                continue;

            int planetPrice = planetData.itemPriceDictionary[currentItem.id];
            int boughtPrice = currentItem.boughtCost;

            TextMeshProUGUI planetPriceText = planetInstance.GetComponentInChildren<TextMeshProUGUI>();
            if (planetPrice < boughtPrice)
            {
                planetPriceText.color = downColor;
                planetPriceText.text = planetPrice.ToString() + "▼";
            }
            else if (planetPrice == boughtPrice)
            {
                planetPriceText.color = sameColor;
                planetPriceText.text = planetPrice.ToString() + "-";
            }
            else
            {
                planetPriceText.color = upColor;
                planetPriceText.text = planetPrice.ToString() + "▲";
            }
        }
    }

    private void UpdatePlayerPosition()
    {
        if (currentPositionIndicator == null)
            currentPositionIndicator = Instantiate(currentPlayerIndicatorPrefab, mapContent.transform);

        RectTransform contentRect = mapContent.GetComponent<RectTransform>();
        RectTransform positionIndicatorRect = currentPositionIndicator.GetComponent<RectTransform>();

        SetupMapObject(
            positionIndicatorRect,
            GameManager.Instance.normalizedPlayerPosition,
            Constants.Planets.PlanetCurrentPositionIndicatorSize,
            contentRect
        );
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

    // Optional: Add a method to completely refresh the map if needed
    public void RefreshFullMap()
    {
        isMapInitialized = false;
        Initialize(currentItem);
    }
}
