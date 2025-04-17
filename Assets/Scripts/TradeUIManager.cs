using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 무역 UI를 관리하는 매니저입니다.
/// 메인 패널(TradeTestPanel)은 항상 보이며, Sell 버튼과 Buy 버튼을 통해 각각
/// TradeSellPanel과 TradeBuyPanel이 메인 패널 위에 슬라이드 애니메이션으로 덧씌워집니다.
/// </summary>
public class TradeUIManager : MonoBehaviour
{
    [Header("패널 설정")]
    [Tooltip("게임 실행 시 항상 보이는 메인 패널")]
    public RectTransform tradeTestPanel;
    [Tooltip("Sell 버튼 클릭 시 나타나는 오버레이 패널 (아래쪽에 위치, 슬라이드는 아래에서 위로)")]
    public RectTransform tradeSellPanel;
    [Tooltip("Buy 버튼 클릭 시 나타나는 오버레이 패널")]
    public RectTransform tradeBuyPanel;

    [Header("버튼 설정")]
    [Tooltip("메인 패널 내 Sell 버튼")]
    public Button sellButton;
    [Tooltip("메인 패널 내 Buy 버튼")]
    public Button buyButton;

    [Header("애니메이션 설정")]
    [Tooltip("오버레이 패널 슬라이드 애니메이션 속도")]
    public float slideSpeed = 0.3f;

    // 오버레이 패널의 숨김/표시 위치 변수
    private Vector2 sellPanelHiddenPosition;
    private Vector2 sellPanelVisiblePosition;
    private Vector2 buyPanelHiddenPosition;
    private Vector2 buyPanelVisiblePosition;

    // 슬라이드 애니메이션 진행 중인 코루틴 레퍼런스
    private Coroutine sellSlideCoroutine;
    private Coroutine buySlideCoroutine;

    public static TradeUIManager Instance { get; private set; }

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (sellButton != null)
            sellButton.onClick.AddListener(OpenSellPanel);
        if (buyButton != null)
            buyButton.onClick.AddListener(OpenBuyPanel);

        // 버튼 클릭 이벤트 연결 (기존 코드)
        if (sellButton != null)
            sellButton.onClick.AddListener(OpenSellPanel);
        if (buyButton != null)
            buyButton.onClick.AddListener(OpenBuyPanel);

        // ---- BuyPanel (왼쪽에서 오른쪽 슬라이드) ----
        buyPanelVisiblePosition = new Vector2(tradeBuyPanel.anchoredPosition.x + tradeBuyPanel.rect.width,
            tradeBuyPanel.anchoredPosition.y);
        buyPanelHiddenPosition  = new Vector2(buyPanelVisiblePosition.x - tradeBuyPanel.rect.width,
            buyPanelVisiblePosition.y);
        tradeBuyPanel.anchoredPosition = buyPanelHiddenPosition;

        // ---- SellPanel (오른쪽에서 왼쪽 슬라이드) ----
        sellPanelVisiblePosition = new Vector2(tradeSellPanel.anchoredPosition.x - tradeSellPanel.rect.width,
            tradeSellPanel.anchoredPosition.y);
        sellPanelHiddenPosition  = new Vector2(sellPanelVisiblePosition.x + tradeSellPanel.rect.width,
            sellPanelVisiblePosition.y);
        tradeSellPanel.anchoredPosition = sellPanelHiddenPosition;
    }

    /// <summary>
    /// Sell 버튼 클릭 시 호출되어 TradeSellPanel을 보여줍니다.
    /// 동시에 다른 오버레이(예, Buy 패널)이 열려 있다면 닫습니다.
    /// </summary>
    public void OpenSellPanel()
    {
        // 기존에 열려 있는 다른 패널 처리...
        if (buySlideCoroutine != null)
        {
            StopCoroutine(buySlideCoroutine);
            tradeBuyPanel.anchoredPosition = buyPanelHiddenPosition;
            buySlideCoroutine = null;
        }

        // Sell 패널 애니메이션 실행 (아래에서 위로 슬라이드하여 등장)
        if (sellSlideCoroutine != null)
            StopCoroutine(sellSlideCoroutine);

        sellSlideCoroutine = StartCoroutine(SlidePanel(tradeSellPanel, sellPanelHiddenPosition, sellPanelVisiblePosition));
    }

    /// <summary>
    /// Buy 버튼 클릭 시 호출되어 TradeBuyPanel을 보여줍니다.
    /// 동시에 다른 오버레이(예, Sell 패널)이 열려 있다면 닫습니다.
    /// </summary>
    public void OpenBuyPanel()
    {
        // 만약 Sell 패널이 열려 있다면 우선 닫습니다.
        if (sellSlideCoroutine != null)
        {
            StopCoroutine(sellSlideCoroutine);
            tradeSellPanel.anchoredPosition = sellPanelHiddenPosition;
            sellSlideCoroutine = null;
        }
        // Buy 패널 애니메이션 실행
        if (buySlideCoroutine != null)
            StopCoroutine(buySlideCoroutine);

        buySlideCoroutine = StartCoroutine(SlidePanel(tradeBuyPanel, buyPanelHiddenPosition, buyPanelVisiblePosition));
    }

    /// <summary>
    /// 현재 열려 있는 오버레이 패널들을 닫습니다.
    /// (별도의 클로즈 버튼이나 다른 이벤트에서 호출할 수 있습니다.)
    /// </summary>
    public void CloseOverlayPanels()
    {
        if (sellSlideCoroutine != null)
        {
            StopCoroutine(sellSlideCoroutine);
            sellSlideCoroutine = StartCoroutine(SlidePanel(tradeSellPanel, tradeSellPanel.anchoredPosition, sellPanelHiddenPosition));
        }
        if (buySlideCoroutine != null)
        {
            StopCoroutine(buySlideCoroutine);
            buySlideCoroutine = StartCoroutine(SlidePanel(tradeBuyPanel, tradeBuyPanel.anchoredPosition, buyPanelHiddenPosition));
        }

        // 슬라이드 애니메이션이 완료될 때까지 대기한 후 TradeTestPanel을 다시 활성화
        StartCoroutine(ReenableMainPanelAfterClosing());
    }

    /// <summary>
    /// 슬라이드 애니메이션 시간 (혹은 약간 더) 대기하는 함수입니다.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReenableMainPanelAfterClosing()
    {
        yield return new WaitForSeconds(slideSpeed);
        if (tradeTestPanel != null)
            tradeTestPanel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 지정한 패널을 from 위치에서 to 위치까지 slideSpeed 시간 동안 선형 보간하여 이동시키는 코루틴입니다.
    /// </summary>
    /// <param name="panel">이동시킬 패널</param>
    /// <param name="fromPos">시작 위치</param>
    /// <param name="toPos">목표 위치</param>
    /// <returns>IEnumerator</returns>
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
