using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TradeUIController : MonoBehaviour
{
    [Header("재화 패널")] [SerializeField] private TextMeshProUGUI COMAText;
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private TextMeshProUGUI missileText;
    [SerializeField] private TextMeshProUGUI hypersonicText;

    [SerializeField] private TextMeshProUGUI COMAChangeText;
    [SerializeField] private TextMeshProUGUI fuelChangeText;
    [SerializeField] private TextMeshProUGUI missileChangeText;
    [SerializeField] private TextMeshProUGUI hypersonicChangeText;

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
        InitializeResourcesText();
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
        ResourceManager.Instance.OnResourceChanged += SetResourcesText;
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

    public void OnGoBackButtonClicked()
    {
        SceneChanger.Instance.LoadScene("Planet");
    }

    #region 재화 패널 설정

    public void InitializeResourcesText()
    {
        COMAText.text = ResourceManager.Instance.COMA.ToString("N0");
        fuelText.text =
            $"{((int)ResourceManager.Instance.Fuel).ToString()}/{((int)GameManager.Instance.GetPlayerShip().GetStat(ShipStat.FuelStoreCapacity)).ToString()}";
        missileText.text = ResourceManager.Instance.Missle.ToString();
        hypersonicText.text = ResourceManager.Instance.Hypersonic.ToString();
    }


    public void SetResourcesText(ResourceType type, float amount)
    {
        InitializeResourcesText();

        if (Mathf.Abs(amount) > 0.01f) ShowChangeText(type, amount);
    }

    private void ShowChangeText(ResourceType type, float diff)
    {
        TextMeshProUGUI changeText = null;

        switch (type)
        {
            case ResourceType.COMA: changeText = COMAChangeText; break;
            case ResourceType.Fuel: changeText = fuelChangeText; break;
            case ResourceType.Missile: changeText = missileChangeText; break;
            case ResourceType.Hypersonic: changeText = hypersonicChangeText; break;
        }

        if (changeText == null) return;

        changeText.text = diff > 0 ? $"+{(int)diff}" : $"{(int)diff}";
        changeText.color = diff > 0 ? Color.green : Color.red;
        changeText.gameObject.SetActive(true);

        StartCoroutine(HideChangeText(changeText, 1f));
    }

    private IEnumerator HideChangeText(TextMeshProUGUI text, float delay)
    {
        yield return new WaitForSeconds(delay);
        text.gameObject.SetActive(false);
    }

    #endregion

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

        InitializeResourcesText();
    }

    // 호버 시 아이템 맵 표시 (SellItemInfoPanel에서 OnMouseEnter에서 호출)
    public void ShowItemMap(TradingItemData selectedItem)
    {
        // 아이템 맵 활성화 및 초기화
        itemMapPanel.gameObject.SetActive(true);
        itemMapPanel.Initialize(selectedItem);
    }

    // 호버가 벗어났을 때 아이템 맵 숨김 (SellItemInfoPanel에서 OnMouseExit에서 호출)
    public void HideItemMap()
    {
        // 선택된 아이템이 있는 경우에는 맵을 유지 (선택 상태를 우선시)
        if (selectedSellItemInfoPanel != null)
            // 선택된 아이템의 맵을 다시 표시
            itemMapPanel.Initialize(selectedSellItemInfoPanel.CurrentItem);
        else
            // 선택된 아이템이 없는 경우에는 맵을 숨김
            itemMapPanel.gameObject.SetActive(false);
    }

    public void SellItem(TradingItemData selectedItem)
    {
        TradingItem item = GameManager.Instance.playerShip.GetAllItems().Find(r => r.GetItemData() == selectedItem);

        if (item == null)
        {
            Debug.LogError("이 메시지 보이면 클난거임");
            return;
        }

        GameManager.Instance.playerShip.RemoveItem(item);

        int price = GameManager.Instance.WhereIAm().GetItemPrice(selectedItem);

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, price);

        InitializeSellPanel();
        itemMapPanel.gameObject.SetActive(false);
    }

    #endregion
}
