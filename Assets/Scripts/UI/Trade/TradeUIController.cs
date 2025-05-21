using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TradeUIController : MonoBehaviour
{
    [Header("탭 버튼")] [SerializeField] private Button buttonBuy;
    [SerializeField] private Button buttonSell;

    [Header("패널 목록")] [SerializeField] private GameObject panelBuy;
    [SerializeField] private GameObject panelSell;

    [Header("구매 패널 설정")] [SerializeField] private GameObject buyPanelContent;

    [Header("판매 패널 설정")] [SerializeField] private GameObject sellPanelContent;
    [SerializeField] private GameObject sellItemInfoPrefab;
    [SerializeField] private ItemMapController itemMapPanel;
    private SellItemInfoPanel selectedSellItemInfoPanel;
    private List<SellItemInfoPanel> sellItemInfoPanelInstance = new();


    [Header("열려야 되는 위치")] [SerializeField] private Transform buyPanelOpenedPosition;
    [SerializeField] private Transform SellPanelOpenendPosition;

    [Header("열리는 속도")] [SerializeField] private float slideSpeed = 0.1f;

    private Vector3 buyPanelClosedPosition;
    private Vector3 sellPanelClosedPosition;

    // 각 패널별 코루틴 변수 분리
    private Coroutine buySlideCoroutine;
    private Coroutine sellSlideCoroutine;

    private bool isBuyPanelOpen = false;
    private bool isSellPanelOpen = false;


    private void Start()
    {
        buyPanelClosedPosition = panelBuy.transform.position;
        sellPanelClosedPosition = panelSell.transform.position;

        AddButtonListeners();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isBuyPanelOpen)
                if (!IsClickingOnSelf(panelBuy))
                    SlideBuyClose();

            if (isSellPanelOpen)
                if (!IsClickingOnSelf(panelSell))
                    SlideSellClose();
        }
    }

    private void OnEnable()
    {
    }

    private void AddButtonListeners()
    {
        buttonBuy.onClick.AddListener(() => OnPanelButtonClicked(panelBuy));
        buttonSell.onClick.AddListener(() => OnPanelButtonClicked(panelSell));
    }

    public void OnPanelButtonClicked(GameObject targetPanel)
    {
        if (targetPanel == panelBuy)
        {
            InitializeBuyPanel();
            SlideBuyOpen();
        }

        if (targetPanel == panelSell)
        {
            InitializeSellPanel();
            SlideSellOpen();
        }
    }

    private bool IsClickingOnSelf(GameObject currentPanel)
    {
        PointerEventData pointerData = new(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
            if (result.gameObject.transform.IsChildOf(currentPanel.transform) || result.gameObject == currentPanel)
                return true;
        return false;
    }

    private void SlideBuyOpen()
    {
        if (buySlideCoroutine != null)
            StopCoroutine(buySlideCoroutine);

        buySlideCoroutine = StartCoroutine(SlideToPosition(panelBuy, buyPanelOpenedPosition.position));
        isBuyPanelOpen = true;

        buttonBuy.gameObject.SetActive(false);
    }

    private void SlideSellOpen()
    {
        if (sellSlideCoroutine != null)
            StopCoroutine(sellSlideCoroutine);

        sellSlideCoroutine = StartCoroutine(SlideToPosition(panelSell, SellPanelOpenendPosition.position));
        isSellPanelOpen = true;

        buttonSell.gameObject.SetActive(false);
    }

    private void SlideBuyClose()
    {
        if (buySlideCoroutine != null)
            StopCoroutine(buySlideCoroutine);

        buySlideCoroutine = StartCoroutine(
            SlideToPosition(panelBuy, buyPanelClosedPosition, () => buttonBuy.gameObject.SetActive(true)));
        isBuyPanelOpen = false;
    }

    private void SlideSellClose()
    {
        if (sellSlideCoroutine != null)
            StopCoroutine(sellSlideCoroutine);

        sellSlideCoroutine = StartCoroutine(
            SlideToPosition(panelSell, sellPanelClosedPosition, () => buttonSell.gameObject.SetActive(true)));
        isSellPanelOpen = false;
    }

    private IEnumerator SlideToPosition(GameObject targetPanel, Vector3 targetPosition, Action onComplete = null)
    {
        float elapsed = 0f;

        while (elapsed < slideSpeed)
        {
            targetPanel.transform.position =
                Vector3.Lerp(targetPanel.transform.position, targetPosition, elapsed / slideSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        targetPanel.transform.position = targetPosition;

        onComplete?.Invoke();
    }

    #region 구매 패널 설정

    public void InitializeBuyPanel()
    {
    }

    #endregion

    #region 판매 패널 설정

    public void InitializeSellPanel()
    {
        foreach (SellItemInfoPanel panel in sellItemInfoPanelInstance) Destroy(panel.gameObject);

        sellItemInfoPanelInstance.Clear();

        List<TradingItem> allItems = GameManager.Instance.playerShip.GetAllItems();

        foreach (TradingItem item in allItems)
        {
            SellItemInfoPanel sellItemInfoPanel = Instantiate(sellItemInfoPrefab, sellPanelContent.transform)
                .GetComponent<SellItemInfoPanel>();

            sellItemInfoPanel.Initialize(item);
            sellItemInfoPanelInstance.Add(sellItemInfoPanel);
        }
    }

    public void OnSellItemInfoPanelButtonClicked(TradingItemData selectedItem)
    {
        // 이미 선택된 패널의 아이템과 동일한지 확인
        if (itemMapPanel.gameObject.activeInHierarchy &&
            selectedSellItemInfoPanel != null &&
            selectedSellItemInfoPanel.CurrentItem == selectedItem)
            // 이미 같은 아이템 맵이 열려있으면 아무 작업도 하지 않음
            return;

        // 새 아이템이거나 패널이 닫혀있으면 맵 활성화 및 초기화
        itemMapPanel.gameObject.SetActive(true);
        itemMapPanel.Initialize(selectedItem);
    }

    public void SetSelectedSellItemInfoPanel(SellItemInfoPanel panel)
    {
        if (selectedSellItemInfoPanel != null) selectedSellItemInfoPanel.SetSelected(false);

        selectedSellItemInfoPanel = panel;
        if (selectedSellItemInfoPanel != null) selectedSellItemInfoPanel.SetSelected(true);
    }

    #endregion
}
