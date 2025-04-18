using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 무역 UI를 관리하는 매니저입니다.
/// 메인 패널(TradeTestPanel)과 구매/판매 오버레이 패널을 슬라이드 애니메이션으로 제어하며,
/// 각창의 서브 패널 전환(아이템 선택, 뒤로가기) 기능을 포함합니다.
/// </summary>
public class TradeUIManager : MonoBehaviour
{
    /// <summary>
    /// TradeBuyPanel 안의 StoragePanel Content Transform입니다.
    /// 해당 Content 하위에 있는 모든 InventoryItemUI의 상호작용을 Buy 시에 비활성화합니다.
    /// </summary>
    [Header("Buy 모드 전용")]
    [SerializeField] private Transform storagePanelContent;

    /// <summary>
    /// TradeSellPanel 안의 InventoryPanel Content Transform입니다.
    /// Sell 시에만 상호작용을 허용합니다.
    /// </summary>
    [Header("Sell 모드 전용")]
    [SerializeField] private Transform inventoryPanelContent;

    [Header("판매 인벤토리 UI")]
    /// <summary>
    /// 인벤토리 슬롯을 동적으로 생성/갱신해 주는 스크립트 참조입니다.
    /// Inspector에서 InventoryUI 컴포넌트를 드래그&드롭하세요.
    /// </summary>
    [SerializeField] private InventoryUI buyInventoryUI;
    [SerializeField] private InventoryUI sellInventoryUI;

    [Header("패널 설정")]
    /// <summary>
    /// 메인 화면 패널. 모든 오버레이 창이 닫힐 때 다시 활성화합니다.
    /// </summary>
    public RectTransform tradeTestPanel;
    /// <summary>
    /// 판매창(TradeSellPanel)의 RectTransform 참조입니다.
    /// </summary>
    public RectTransform tradeSellPanel;
    /// <summary>
    /// 구매창(TradeBuyPanel)의 RectTransform 참조입니다.
    /// </summary>
    public RectTransform tradeBuyPanel;

    [Header("메인 버튼 설정")]
    /// <summary>
    /// 메인 화면의 "Sell" 버튼. 클릭 시 판매창을 엽니다.
    /// </summary>
    public Button sellButton;
    /// <summary>
    /// 메인 화면의 "Buy" 버튼. 클릭 시 구매창을 엽니다.
    /// </summary>
    public Button buyButton;

    [Header("애니메이션 설정")]
    /// <summary>
    /// 오버레이 패널 슬라이드 애니메이션 시간(초)입니다.
    /// </summary>
    public float slideSpeed = 0.3f;

    // 내부 슬라이드 위치 캐시
    private Vector2 sellPanelHiddenPosition;
    private Vector2 sellPanelVisiblePosition;
    private Vector2 buyPanelHiddenPosition;
    private Vector2 buyPanelVisiblePosition;
    private Coroutine sellSlideCoroutine;
    private Coroutine buySlideCoroutine;

    [Header("구매창 서브 패널 및 뒤로가기 버튼")]
    /// <summary>
    /// 구매창 내 행성 아이템 목록 패널입니다.
    /// </summary>
    public GameObject planetItemPanel;
    /// <summary>
    /// 구매창 내 상세 정보(MiddlePanel) 패널입니다.
    /// </summary>
    public GameObject buyMiddlePanel;
    /// <summary>
    /// 구매창 내 StoragePanel(플레이어 인벤토리) 패널입니다.
    /// </summary>
    public GameObject storagePanel;
    /// <summary>
    /// 구매창 상세/Storage에서 뒤로 돌아갈 때 누르는 버튼입니다.
    /// </summary>
    public Button buyBackButton;

    [Header("판매창 서브 패널 및 뒤로가기 버튼")]
    /// <summary>
    /// 판매창 내 플레이어 인벤토리 목록 패널입니다.
    /// </summary>
    public GameObject inventoryPanel;
    /// <summary>
    /// 판매창 내 상세 정보(MidSellPanel) 패널입니다.
    /// </summary>
    public GameObject sellMiddlePanel;
    /// <summary>
    /// 판매창 상세에서 뒤로 돌아갈 때 누르는 버튼입니다.
    /// </summary>
    public Button sellBackButton;

    /// <summary>
    /// 이 클래스의 싱글톤 인스턴스입니다.
    /// </summary>
    public static TradeUIManager Instance { get; private set; }

    /// <summary>
    /// 싱글톤 패턴을 설정합니다.
    /// </summary>
    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    /// <summary>
    /// 초기화: 메인 버튼, 슬라이드 위치, 서브 패널/뒤로가기 버튼 리스너를 연결하고,
    /// 구매/판매창의 서브 패널 초기 상태를 설정합니다.
    /// </summary>
    void Start()
    {
        // 메인 Sell/Buy 버튼 연결 (기존 기능)
        if (sellButton != null) sellButton.onClick.AddListener(OpenSellPanel);
        if (buyButton  != null) buyButton .onClick.AddListener(OpenBuyPanel);

        // ---- BuyPanel 슬라이드 초기 위치 계산 (기존 코드) ----
        buyPanelVisiblePosition = new Vector2(
            tradeBuyPanel.anchoredPosition.x + tradeBuyPanel.rect.width,
            tradeBuyPanel.anchoredPosition.y
        );
        buyPanelHiddenPosition = new Vector2(
            buyPanelVisiblePosition.x - tradeBuyPanel.rect.width,
            buyPanelVisiblePosition.y
        );
        tradeBuyPanel.anchoredPosition = buyPanelHiddenPosition;

        // ---- SellPanel 슬라이드 초기 위치 계산 (기존 코드) ----
        sellPanelVisiblePosition = new Vector2(
            tradeSellPanel.anchoredPosition.x - tradeSellPanel.rect.width,
            tradeSellPanel.anchoredPosition.y
        );
        sellPanelHiddenPosition = new Vector2(
            sellPanelVisiblePosition.x + tradeSellPanel.rect.width,
            sellPanelVisiblePosition.y
        );
        tradeSellPanel.anchoredPosition = sellPanelHiddenPosition;

        // —— 구매창 서브 패널 초기 상태 ——
        planetItemPanel.SetActive(true);
        buyMiddlePanel .SetActive(false);
        storagePanel   .SetActive(false);

        // —— 판매창 서브 패널 초기 상태 ——
        inventoryPanel .SetActive(true);
        sellMiddlePanel.SetActive(false);

        // 뒤로가기 버튼 리스너 연결
        if (buyBackButton  != null) buyBackButton .onClick.AddListener(OnBackPressed);
        if (sellBackButton != null) sellBackButton.onClick.AddListener(OnBackPressed);
    }

    /// <summary>
    /// 행성 아이템 목록에서 아이템을 선택했을 때 호출합니다.
    /// 구매창의 상세 정보와 StoragePanel을 활성화합니다.
    /// Inspector에서 PlanetItemPanel 내 각 아이템 버튼의 OnClick에 연결하세요.
    /// </summary>
    public void OnBuyItemSelected()
    {
        buyMiddlePanel .SetActive(true);
        storagePanel   .SetActive(true);
    }

    /// <summary>
    /// 인벤토리 목록에서 아이템을 선택했을 때 호출합니다.
    /// 판매창의 상세 정보 패널(MidSellPanel)을 활성화합니다.
    /// Inspector에서 InventoryPanel 내 각 아이템 버튼의 OnClick에 연결하세요.
    /// </summary>
    public void OnSellItemSelected()
    {
        sellMiddlePanel.SetActive(true);
    }

    /// <summary>
    /// 씬에 배치된 Back 버튼을 클릭했을 때 호출됩니다.
    /// 열려 있는 상세 패널을 닫고, 원래 목록 패널로 복귀시킵니다.
    /// </summary>
    private void OnBackPressed()
    {
        if (buyMiddlePanel.activeSelf)
        {
            buyMiddlePanel .SetActive(false);
            storagePanel   .SetActive(false);
            // planetItemPanel은 그대로 활성
        }
        else if (sellMiddlePanel.activeSelf)
        {
            sellMiddlePanel.SetActive(false);
            // inventoryPanel은 그대로 활성
        }
    }

    /// <summary>
    /// 메인 화면의 "Sell" 버튼 클릭 시 호출됩니다.
    /// 기존에 열려 있던 구매창이 있으면 닫고, 판매창을 슬라이드 인합니다.
    /// </summary>
    public void OpenSellPanel()
    {
        if (buySlideCoroutine != null)
        {
            StopCoroutine(buySlideCoroutine);
            tradeBuyPanel.anchoredPosition = buyPanelHiddenPosition;
            buySlideCoroutine = null;
        }

        if (buyInventoryUI != null)
        {
            buyInventoryUI.PopulateInventory(false);  // 클릭 안 되게
        }

        if (sellSlideCoroutine != null) StopCoroutine(sellSlideCoroutine);

        sellSlideCoroutine = StartCoroutine(
            SlidePanel(tradeSellPanel, sellPanelHiddenPosition, sellPanelVisiblePosition)
        );
        string previously = InventoryItemUI.currentlySelectedItemName;
        if (!string.IsNullOrEmpty(previously))
        {
            // contentPanel 은 sellInventoryUI 에서 SerializeField 로 드래그한 Transform
            foreach (var slotUI in sellInventoryUI.GetComponentsInChildren<InventoryItemUI>())
            {
                if (slotUI.GetStoredItemName() == previously)
                {
                    slotUI.SetSelected(true);
                    break;
                }
            }
            // contentPanel 은 buyInventoryUI 에서 SerializeField 로 드래그한 Transform
            foreach (var slotUI in buyInventoryUI.GetComponentsInChildren<InventoryItemUI>())
            {
                if (slotUI.GetStoredItemName() == previously)
                {
                    slotUI.SetSelected(true);
                    break;
                }
            }
        }
        tradeBuyPanel.gameObject.SetActive(false);
        tradeSellPanel.gameObject.SetActive(true);

        // 클릭 가능한 상태로 인벤토리 다시 채움
        sellInventoryUI.PopulateInventory(true);
    }

    /// <summary>
    /// 메인 화면의 "Buy" 버튼 클릭 시 호출됩니다.
    /// 기존에 열려 있던 판매창이 있으면 닫고, 구매창을 슬라이드 인합니다.
    /// </summary>
    public void OpenBuyPanel()
    {
        // 1. 이전 선택 초기화
        InventoryItemUI.currentlySelectedItemName = string.Empty;

        // 2. 판매창 닫기
        if (sellSlideCoroutine != null)
        {
            StopCoroutine(sellSlideCoroutine);
            tradeSellPanel.anchoredPosition = sellPanelHiddenPosition;
            sellSlideCoroutine = null;
        }

        // 3. 구매창 슬라이드 인
        if (buySlideCoroutine != null) StopCoroutine(buySlideCoroutine);
        buySlideCoroutine = StartCoroutine(
            SlidePanel(tradeBuyPanel, buyPanelHiddenPosition, buyPanelVisiblePosition)
        );
        tradeBuyPanel.gameObject.SetActive(true);
        tradeSellPanel.gameObject.SetActive(false);

        // (이미 있던) BuyInventoryUI 갱신 — 클릭 불가 모드
        buyInventoryUI.PopulateInventory(false);

        // 4. StoragePanel 전체 비활성화 & 강조 해제
        var storageGroup = storagePanelContent.GetComponent<CanvasGroup>();
        if (storageGroup == null)
            storageGroup = storagePanelContent.gameObject.AddComponent<CanvasGroup>();
        storageGroup.interactable    = false;  // UI 요소 비인터랙티브
        storageGroup.blocksRaycasts  = false;  // 클릭 이벤트 차단

        // 기존에 강조된 슬롯이 남아 있다면 모두 해제
        foreach (var itemUI in storagePanelContent
                     .GetComponentsInChildren<InventoryItemUI>())
        {
            itemUI.SetSelected(false);
        }
    }

    /// <summary>
    /// 현재 열려 있는 모든 오버레이 패널(구매/판매)을 순차적으로 닫습니다.
    /// 슬라이드 아웃 애니메이션 후 메인 패널을 다시 활성화합니다.
    /// </summary>
    public void CloseOverlayPanels()
    {
        if (sellSlideCoroutine != null)
        {
            StopCoroutine(sellSlideCoroutine);
            sellSlideCoroutine = StartCoroutine(
                SlidePanel(tradeSellPanel, tradeSellPanel.anchoredPosition, sellPanelHiddenPosition)
            );
        }
        if (buySlideCoroutine != null)
        {
            StopCoroutine(buySlideCoroutine);
            buySlideCoroutine = StartCoroutine(
                SlidePanel(tradeBuyPanel, tradeBuyPanel.anchoredPosition, buyPanelHiddenPosition)
            );
        }
        StartCoroutine(ReenableMainPanelAfterClosing());
        tradeBuyPanel.gameObject.SetActive(false);
        tradeSellPanel.gameObject.SetActive(true);

        // InventoryPanel의 모든 InventoryItemUI 활성화
        foreach (var itemUI in inventoryPanelContent.GetComponentsInChildren<InventoryItemUI>())
        {
            itemUI.isInteractable = true;
        }
    }

    /// <summary>
    /// 슬라이드 애니메이션이 완료된 후, 메인 화면 패널(tradeTestPanel)을 활성화합니다.
    /// </summary>
    private IEnumerator ReenableMainPanelAfterClosing()
    {
        yield return new WaitForSeconds(slideSpeed);
        if (tradeTestPanel != null)
            tradeTestPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 지정된 RectTransform 패널을 fromPos 위치에서 toPos 위치로
    /// slideSpeed 시간에 걸쳐 선형 보간(Lerp)하여 이동시킵니다.
    /// </summary>
    /// <param name="panel">슬라이드할 RectTransform 패널</param>
    /// <param name="fromPos">시작 앵커드 위치</param>
    /// <param name="toPos">목표 앵커드 위치</param>
    private IEnumerator SlidePanel(RectTransform panel, Vector2 fromPos, Vector2 toPos)
    {
        float elapsed = 0f;
        while (elapsed < slideSpeed)
        {
            panel.anchoredPosition = Vector2.Lerp(fromPos, toPos, elapsed / slideSpeed);
            elapsed += Time.deltaTime;
            yield return null;
        }
        panel.anchoredPosition = toPos;
    }
}
