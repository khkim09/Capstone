using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TradeUIController : MonoBehaviour
{
    /// <summary>
    /// 배의 위치 렌더링 조정을 위해 필요한 카메라 참조
    /// </summary>
    [Header("카메라")] [SerializeField] private ShipFollowCamera shipFollowCamera;

    /// <summary>
    /// 구매 패널이 열려있을 때 배가 위치해야하는 정규화된 위치
    /// </summary>
    [SerializeField] private Vector2 buyPanelOpenedShipPosition = new(0.6f, 0.5f);

    /// <summary>
    /// 판매 패널이 열려있을 때 배가 위치해야하는 정규화된 위치
    /// </summary>
    [SerializeField] private Vector2 sellPanelOpenedShipPosition = new(0.4f, 0.5f);

    /// <summary>
    /// COMA 보유량 텍스트
    /// </summary>
    [Header("재화 패널")] [SerializeField] private TextMeshProUGUI COMAText;

    /// <summary>
    /// 연료 보유량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI fuelText;

    /// <summary>
    /// 미사일 보유량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI missileText;

    /// <summary>
    /// 초음속탄 보유량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI hypersonicText;

    /// <summary>
    /// COMA 변동량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI COMAChangeText;

    /// <summary>
    /// 연료 변동량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI fuelChangeText;

    /// <summary>
    /// 미사일 변동량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI missileChangeText;

    /// <summary>
    /// 초음속탄 변동량 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI hypersonicChangeText;

    /// <summary>
    /// 구매 패널을 여는 버튼
    /// </summary>
    [Header("탭 버튼")] [SerializeField] private Button buttonBuy;

    /// <summary>
    /// 판매 패널을 여는 버튼
    /// </summary>
    [SerializeField] private Button buttonSell;

    /// <summary>
    /// 구매 패널
    /// </summary>
    [Header("패널 목록")] [SerializeField] private GameObject panelBuy;

    /// <summary>
    /// 판매 패널
    /// </summary>
    [SerializeField] private GameObject panelSell;

    /// <summary>
    /// 아이템 구매 패널에 표시될 정보를 담는 곳
    /// </summary>
    [Header("구매 패널 설정")] [SerializeField] private GameObject buyPanelContent;

    /// <summary>
    /// 아이템 구매 패널에 담기는 정보 게임 오브젝트 프리팹
    /// </summary>
    [SerializeField] private GameObject buyItemInfoPrefab;


    /// <summary>
    /// 구매 중일 때의 아이템 맵 위치
    /// </summary>
    [SerializeField] private Transform itemMapBuyPosition;

    /// <summary>
    /// 구매 확인창 패널
    /// </summary>
    [SerializeField] private BuyCheckPanel buyCheckPanel;

    /// <summary>
    /// 구매 확정 버튼
    /// </summary>
    [SerializeField] private Button buyConfirmButton;

    /// <summary>
    /// 구매 취소 버튼
    /// </summary>
    [SerializeField] private Button buyCancelButton;

    /// <summary>
    /// 임시 창고의 위치를 계산하기 위한 메인 카메라 참조
    /// </summary>
    [SerializeField] private Camera mainCamera;

    /// <summary>
    /// 임시 창고를 설치할 창고용 배
    /// </summary>
    [SerializeField] private Ship tradeShip;

    /// <summary>
    /// 임시 창고의 정규화된 좌표 (뷰포트를 따름)
    /// </summary>
    [SerializeField] private Vector2 normalizedTradeShipPosition = new(0.1f, 0.5f);

    /// <summary>
    /// 임시 창고가 비어있지 않다는 경고 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI notEmptyWarningText;

    /// <summary>
    /// 임시 창고에 함선을 두지 마라는 경고 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI dontPlaceItemHereText;

    /// <summary>
    /// 선택 중인 구매 아이템 정보 오브젝트 참조
    /// </summary>
    private BuyItemInfoPanel selectedBuyItemInfoPanel;

    /// <summary>
    /// 모든 구매 아이템 정보 오브젝트 리스트
    /// </summary>
    private List<BuyItemInfoPanel> buyItemInfoPanelInstance = new();

    /// <summary>
    /// 구매 패널이 초기화 되었는지 여부
    /// </summary>
    private bool isBuyPanelInitialized = false;

    /// <summary>
    /// 거래용 임시 창고 참조
    /// </summary>
    private StorageRoomBase tradeStorage;

    /// <summary>
    /// 거래 중인 아이템 참조
    /// </summary>
    private TradingItem itemInstance;

    /// <summary>
    /// 아이템 판매 패널에 표시될 정보를 담는 곳
    /// </summary>
    [Header("판매 패널 설정")] [SerializeField] private GameObject sellPanelContent;

    /// <summary>
    /// 아이템 판매 패널에 담기는 정보 오브젝트 프리팹
    /// </summary>
    [SerializeField] private GameObject sellItemInfoPrefab;

    /// <summary>
    /// 아이템 판매 중일 때 아이템 맵의 위치
    /// </summary>
    [SerializeField] private Transform itemMapSellPosition;

    /// <summary>
    /// 선택 중인 아이템 판매 정보 오브젝트 참조
    /// </summary>
    private SellItemInfoPanel selectedSellItemInfoPanel;

    /// <summary>
    /// 모든 아이템 판매 정보 오브젝트 리스트
    /// </summary>
    private List<SellItemInfoPanel> sellItemInfoPanelInstance = new();

    [Header("장비 패널 설정")] [SerializeField] private EquipmentDatabase equipmentDatabase;
    [SerializeField] private GameObject equipmentItemInfoPrefab;
    private EquipmentInfoPanel equipmentLeftInstance;
    private EquipmentInfoPanel equipmentRightInstance;
    private EquipmentInfoPanel equipmentMiddleInstance;
    [SerializeField] private GameObject equipmentLeftContainer;
    [SerializeField] private GameObject equipmentMiddleContainer;
    [SerializeField] private GameObject equipmentRightContainer;
    [SerializeField] private GameObject equipmentSoldOutPrefab;
    private GameObject equipmentLeftSoldOutInstance;
    private GameObject equipmentRightSoldOutInstance;
    private GameObject equipmentMiddleSoldOutInstance;
    [SerializeField] private GameObject equipmentGlobalApplyAllPanel;
    private EquipmentItem recentBoughtEquipment;

    /// <summary>
    /// 연료 구매 버튼
    /// </summary>
    [Header("자원 구매 패널")] [SerializeField] private Button fuelBuyButton;

    /// <summary>
    /// 미사일 구매 버튼
    /// </summary>
    [SerializeField] private Button missileBuyButton;

    /// <summary>
    /// 초음속탄 구매 버튼
    /// </summary>
    [SerializeField] private Button hypersonicBuyButton;

    /// <summary>
    /// 연료 가격
    /// </summary>
    [SerializeField] private TextMeshProUGUI fuelPrice;

    /// <summary>
    /// 미사일 가격
    /// </summary>
    [SerializeField] private TextMeshProUGUI missilePrice;

    /// <summary>
    /// 미사일 가격
    /// </summary>
    [SerializeField] private TextMeshProUGUI hypersonicPrice;

    [SerializeField] private TextMeshProUGUI notEnoughFuelCapacityText;

    /// <summary>
    /// 구매 패널이 열려야되는 위치
    /// </summary>
    [Header("열려야 되는 위치")] [SerializeField] private Transform buyPanelOpenedPosition;

    /// <summary>
    /// 판매 패널이 열려야되는 위치
    /// </summary>
    [SerializeField] private Transform SellPanelOpenendPosition;

    /// <summary>
    /// 패널들이 열리는 속도
    /// </summary>
    [Header("열리는 속도")] [SerializeField] private float slideSpeed = 0.1f;

    /// <summary>
    /// 아이템 맵 패널
    /// </summary>
    [SerializeField] private ItemMapController itemMapPanel;

    /// <summary>
    /// 씬 나가기 버튼
    /// </summary>
    [SerializeField] private Button exitButton;

    /// <summary>
    /// 돈 부족 알림 텍스트
    /// </summary>
    [SerializeField] private TextMeshProUGUI notEnoughCOMAText;

    /// <summary>
    /// 구매 패널의 닫혀있는 위치 참조
    /// </summary>
    private Vector3 buyPanelClosedPosition;

    /// <summary>
    /// 판매 패널의 닫혀있는 위치 참조
    /// </summary>
    private Vector3 sellPanelClosedPosition;

    /// <summary>
    /// 구매 패널이 슬라이드 되는 코루틴
    /// </summary>
    private Coroutine buySlideCoroutine;

    /// <summary>
    /// 판매 패널이 슬라이드 되는 코루틴
    /// </summary>
    private Coroutine sellSlideCoroutine;

    /// <summary>
    /// 카메라 최초 위치  참조
    /// </summary>
    private Vector2 originalCameraPosition;

    /// <summary>
    /// 패널이 열릴 때 카메라의 위치를 조정하기 위한 코루틴
    /// </summary>
    private Coroutine cameraTransitionCoroutine;

    /// <summary>
    /// 구매 패널이 열려있는지 여부
    /// </summary>
    private bool isBuyPanelOpen = false;

    /// <summary>
    /// 판매 패널이 열려있는지 여부
    /// </summary>
    private bool isSellPanelOpen = false;

    /// <summary>
    /// 거래용 임시 배 및 창고를 초기화하고 각종 UI들의 위치 관계와 버튼의 Listener를 추가함.
    /// </summary>
    private void Start()
    {
        tradeShip.Initialize();
        tradeStorage =
            GameObjectFactory.Instance.RoomFactory.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        tradeShip.AddRoom(tradeStorage);
        tradeShip.gameObject.SetActive(false);


        buyPanelClosedPosition = panelBuy.transform.position;
        sellPanelClosedPosition = panelSell.transform.position;

        // 원래 카메라 위치 저장
        if (shipFollowCamera != null)
            originalCameraPosition =
                new Vector2(shipFollowCamera.TargetShipPositionX, shipFollowCamera.TargetShipPositionY);

        AddButtonListeners();
        InitializeResourcesText();

        recentBoughtEquipment = null;

        equipmentRightSoldOutInstance = Instantiate(equipmentSoldOutPrefab, equipmentRightContainer.transform);
        equipmentLeftSoldOutInstance = Instantiate(equipmentSoldOutPrefab, equipmentLeftContainer.transform);
        equipmentMiddleSoldOutInstance = Instantiate(equipmentSoldOutPrefab, equipmentMiddleContainer.transform);
        equipmentLeftSoldOutInstance.SetActive(false);
        equipmentRightSoldOutInstance.SetActive(false);
        equipmentMiddleSoldOutInstance.SetActive(false);

        isBuyPanelInitialized = false;
    }

    /// <summary>
    /// 패널 외 다른 부분을 클릭하는지 검사하고, 클릭했다면 패널을 닫는다.
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isBuyPanelOpen)
            {
                if (buyCheckPanel.gameObject.activeInHierarchy ||
                    equipmentGlobalApplyAllPanel.gameObject.activeInHierarchy)
                    return;

                if (!IsClickingOnSelf(panelBuy))
                    SlideBuyClose();
            }


            if (isSellPanelOpen)
                if (!IsClickingOnSelf(panelSell))
                    SlideSellClose();
        }
    }

    /// <summary>
    /// 자원 변화 효과를 위해 ResourceManager 의 OnResourceChanged 를 구독.
    /// </summary>
    private void OnEnable()
    {
        ResourceManager.Instance.OnResourceChanged += SetResourcesText;
    }

    /// <summary>
    /// 각 패널을 여는 버튼들에 Listners 추가.
    /// </summary>
    private void AddButtonListeners()
    {
        buttonBuy.onClick.AddListener(() => OnPanelButtonClicked(panelBuy));
        buttonSell.onClick.AddListener(() => OnPanelButtonClicked(panelSell));
    }

    /// <summary>
    /// 각 패널을 여는 버튼들의 기능.
    /// </summary>
    /// <param name="targetPanel"></param>
    public void OnPanelButtonClicked(GameObject targetPanel)
    {
        if (targetPanel == panelBuy)
        {
            InitializeBuyPanel();
            InitializeEquipmentPanel();
            InitializeResourceBuyPanel();
            SlideBuyOpen();
        }

        if (targetPanel == panelSell)
        {
            InitializeSellPanel();
            SlideSellOpen();
        }
    }

    /// <summary>
    /// 스스로를 클릭하는 여부를 반환하는 함수
    /// </summary>
    /// <param name="currentPanel">확인하고픈 패널</param>
    /// <returns>스스로를 클릭 중인지 여부</returns>
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

    /// <summary>
    /// 구매 패널을 여는 함수.
    /// </summary>
    private void SlideBuyOpen()
    {
        if (buySlideCoroutine != null)
            StopCoroutine(buySlideCoroutine);

        buySlideCoroutine = StartCoroutine(SlideToPosition(panelBuy, buyPanelOpenedPosition.position));
        isBuyPanelOpen = true;

        // 카메라 위치도 함께 변경
        TransitionCameraToPosition(buyPanelOpenedShipPosition);
        MoveShipToPoint();

        buttonBuy.gameObject.SetActive(false);
    }

    /// <summary>
    /// 판매 패널을 여는 함수
    /// </summary>
    private void SlideSellOpen()
    {
        if (sellSlideCoroutine != null)
            StopCoroutine(sellSlideCoroutine);

        sellSlideCoroutine = StartCoroutine(SlideToPosition(panelSell, SellPanelOpenendPosition.position));
        isSellPanelOpen = true;

        // 카메라 위치도 함께 변경
        TransitionCameraToPosition(sellPanelOpenedShipPosition);

        buttonSell.gameObject.SetActive(false);
    }

    /// <summary>
    /// 구매 패널을 닫는 함수
    /// </summary>
    private void SlideBuyClose()
    {
        if (buySlideCoroutine != null)
            StopCoroutine(buySlideCoroutine);

        buySlideCoroutine = StartCoroutine(
            SlideToPosition(panelBuy, buyPanelClosedPosition, () => buttonBuy.gameObject.SetActive(true)));
        isBuyPanelOpen = false;

        // 카메라 위치를 원래대로 복귀
        TransitionCameraToPosition(originalCameraPosition);
    }

    /// <summary>
    /// 판매 패널을 닫는 함수
    /// </summary>
    private void SlideSellClose()
    {
        if (sellSlideCoroutine != null)
            StopCoroutine(sellSlideCoroutine);

        sellSlideCoroutine = StartCoroutine(
            SlideToPosition(panelSell, sellPanelClosedPosition, () => buttonSell.gameObject.SetActive(true)));
        isSellPanelOpen = false;

        // 카메라 위치를 원래대로 복귀
        TransitionCameraToPosition(originalCameraPosition);
    }

    /// <summary>
    /// 패널을 원하는 위치로 슬라이드(개폐)하는 함수
    /// </summary>
    /// <param name="targetPanel">열고 닫을 패널</param>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="onComplete">완료했을 때의 콜백</param>
    /// <returns>코루틴</returns>
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

    /// <summary>
    /// 패널을 열고 닫을 때 배가 적절한 위치에 렌더링되기 위해 카메라를 이동시키는 함수
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    private void TransitionCameraToPosition(Vector2 targetPosition)
    {
        if (shipFollowCamera == null) return;

        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        cameraTransitionCoroutine = StartCoroutine(CameraTransitionCoroutine(targetPosition));
    }

    /// <summary>
    /// 카메라를 목표 위치로 이동시키는 코루틴
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <returns>코루틴</returns>
    private IEnumerator CameraTransitionCoroutine(Vector2 targetPosition)
    {
        if (shipFollowCamera == null) yield break;

        Vector2 startPosition = new(shipFollowCamera.TargetShipPositionX, shipFollowCamera.TargetShipPositionY);
        float elapsed = 0f;

        while (elapsed < slideSpeed)
        {
            float t = elapsed / slideSpeed;
            Vector2 currentPosition = Vector2.Lerp(startPosition, targetPosition, t);

            shipFollowCamera.TargetShipPositionX = currentPosition.x;
            shipFollowCamera.TargetShipPositionY = currentPosition.y;

            // 카메라 위치 재계산
            shipFollowCamera.GetCameraStartPositionToOriginShip();

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 설정
        shipFollowCamera.TargetShipPositionX = targetPosition.x;
        shipFollowCamera.TargetShipPositionY = targetPosition.y;
        shipFollowCamera.GetCameraStartPositionToOriginShip();
    }

    /// <summary>
    /// 나가기 버튼이 눌렸을 때 호출됨. 행성 씬으로 돌아간다.
    /// </summary>
    public void OnGoBackButtonClicked()
    {
        SceneChanger.Instance.LoadScene("Planet");
    }

    #region 재화 패널 설정

    /// <summary>
    /// 재화 패널의 텍스트들을 초기화한다.
    /// </summary>
    public void InitializeResourcesText()
    {
        COMAText.text = ResourceManager.Instance.COMA.ToString("N0");
        fuelText.text =
            $"{((int)ResourceManager.Instance.Fuel).ToString()}/{((int)GameManager.Instance.GetPlayerShip().GetStat(ShipStat.FuelStoreCapacity)).ToString()}";
        missileText.text = ResourceManager.Instance.Missle.ToString();
        hypersonicText.text = ResourceManager.Instance.Hypersonic.ToString();
    }

    /// <summary>
    /// 재화 패널의 텍스트를 업데이트하고, 변화값을 표시한다.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="amount"></param>
    public void SetResourcesText(ResourceType type, float amount)
    {
        InitializeResourcesText();

        if (Mathf.Abs(amount) > 0.01f) ShowChangeText(type, amount);
    }

    /// <summary>
    /// 재화의 변화량을 표시하는 함수
    /// </summary>
    /// <param name="type">재화의 타입</param>
    /// <param name="diff">변화량</param>
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

    /// <summary>
    /// 텍스트를 몇 초 있다가 없어지게 하는 코루틴
    /// </summary>
    /// <param name="text">해당 텍스트 참조</param>
    /// <param name="delay">기다릴 시간</param>
    /// <returns>코루틴</returns>
    private IEnumerator HideChangeText(TextMeshProUGUI text, float delay)
    {
        yield return new WaitForSeconds(delay);
        text.gameObject.SetActive(false);
    }

    #endregion

    #region 구매 패널 설정

    /// <summary>
    /// 구매 패널을 초기화한다. 현재 거래량에 따라 정해진 행성에서 판매하는 아이템의 목록을 미리 불러오고 구매 패널에 추가한다.
    /// </summary>
    public void InitializeBuyPanel()
    {
        if (isBuyPanelInitialized)
            return;
        foreach (BuyItemInfoPanel panel in buyItemInfoPanelInstance) Destroy(panel.gameObject);

        buyItemInfoPanelInstance.Clear();

        PlanetData currentPlanet = GameManager.Instance.WhereIAm();
        List<TradingItemData> tier1 = currentPlanet.itemsTier1;
        List<TradingItemData> tier2 = currentPlanet.itemsTier2;
        List<TradingItemData> tier3 = currentPlanet.itemsTier3;

        foreach (TradingItemData item in tier1)
        {
            BuyItemInfoPanel buyItemInfoPanel = Instantiate(buyItemInfoPrefab, buyPanelContent.transform)
                .GetComponent<BuyItemInfoPanel>();

            buyItemInfoPanel.Initialize(item);
            buyItemInfoPanelInstance.Add(buyItemInfoPanel);
            buyItemInfoPanel.gameObject.SetActive(false);
        }

        foreach (TradingItemData item in tier2)
        {
            BuyItemInfoPanel buyItemInfoPanel = Instantiate(buyItemInfoPrefab, buyPanelContent.transform)
                .GetComponent<BuyItemInfoPanel>();

            buyItemInfoPanel.Initialize(item);
            buyItemInfoPanelInstance.Add(buyItemInfoPanel);
            buyItemInfoPanel.gameObject.SetActive(false);
        }

        foreach (TradingItemData item in tier3)
        {
            BuyItemInfoPanel buyItemInfoPanel = Instantiate(buyItemInfoPrefab, buyPanelContent.transform)
                .GetComponent<BuyItemInfoPanel>();

            buyItemInfoPanel.Initialize(item);
            buyItemInfoPanelInstance.Add(buyItemInfoPanel);
            buyItemInfoPanel.gameObject.SetActive(false);
        }

        UpdateBuyPanel();
        InitializeResourcesText();

        isBuyPanelInitialized = true;
    }

    /// <summary>
    /// 구매 패널을 갱신한다.
    /// </summary>
    public void UpdateBuyPanel()
    {
        PlanetData currentPlanet = GameManager.Instance.WhereIAm();
        if (currentPlanet.CurrentTier >= ItemTierLevel.T1)
        {
            List<BuyItemInfoPanel> tier1 = buyItemInfoPanelInstance.Where(i => i.CurrentItem.tier == ItemTierLevel.T1)
                .ToList();

            foreach (BuyItemInfoPanel item in tier1) item.gameObject.SetActive(true);
        }

        if (currentPlanet.CurrentTier >= ItemTierLevel.T2)
        {
            List<BuyItemInfoPanel> tier2 = buyItemInfoPanelInstance.Where(i => i.CurrentItem.tier == ItemTierLevel.T2)
                .ToList();

            foreach (BuyItemInfoPanel item in tier2) item.gameObject.SetActive(true);
        }

        if (currentPlanet.CurrentTier == ItemTierLevel.T3)
        {
            List<BuyItemInfoPanel> tier3 = buyItemInfoPanelInstance.Where(i => i.CurrentItem.tier == ItemTierLevel.T3)
                .ToList();

            foreach (BuyItemInfoPanel item in tier3) item.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 구매 패널의 아이템 맵을 보여준다. 구매 패널의 아이템맵을 가격 비교를 현재 행성에서 판매하는 가격을 다른 행성과 비교한다.
    /// </summary>
    /// <param name="selectedItem">선택한 아이템</param>
    public void ShowBuyItemMap(TradingItemData selectedItem)
    {
        itemMapPanel.gameObject.SetActive(true);
        itemMapPanel.transform.position = itemMapBuyPosition.position;
        itemMapPanel.SetIsPlanetItemMap(true);

        itemMapPanel.Initialize(selectedItem);
    }

    /// <summary>
    /// 구매 확인 창을 띄우는 함수
    /// </summary>
    /// <param name="selectedItem">선택한 아이템</param>
    public void ShowBuyCheckPanel(TradingItemData selectedItem)
    {
        buyCheckPanel.gameObject.SetActive(true);
        buyCheckPanel.Initialize(selectedItem);
    }

    /// <summary>
    /// 구매 승인 버튼을 누를 때 호출되는 함수
    /// </summary>
    /// <param name="selectedItem">선택한 아이템</param>
    public void OnBuyCheckBuyButtonClicked(TradingItemData selectedItem)
    {
        int price = selectedItem.amount * selectedItem.boughtCost;

        if (price > ResourceManager.Instance.COMA)
        {
            notEnoughCOMAText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughCOMAText, 1.5f));
            return;
        }

        // 새로운 TradingItemData 인스턴스 생성 및 복사
        TradingItemData copiedItemData = ScriptableObject.CreateInstance<TradingItemData>();

        // 모든 필드 복사
        copiedItemData.id = selectedItem.id;
        copiedItemData.planet = selectedItem.planet;
        copiedItemData.tier = selectedItem.tier;
        copiedItemData.itemState = selectedItem.itemState;
        copiedItemData.itemName = selectedItem.itemName;
        copiedItemData.debugName = selectedItem.debugName;
        copiedItemData.type = selectedItem.type;
        copiedItemData.temperatureMin = selectedItem.temperatureMin;
        copiedItemData.temperatureMax = selectedItem.temperatureMax;
        copiedItemData.shape = selectedItem.shape;
        copiedItemData.costBase = selectedItem.costBase;
        copiedItemData.capacity = selectedItem.capacity;
        copiedItemData.costMin = selectedItem.costMin;
        copiedItemData.costChangerate = selectedItem.costChangerate;
        copiedItemData.costMax = selectedItem.costMax;
        copiedItemData.description = selectedItem.description;
        copiedItemData.itemSprite = selectedItem.itemSprite;
        copiedItemData.amount = selectedItem.amount; // BuyCheckPanel에서 설정된 수량
        copiedItemData.boughtCost = selectedItem.boughtCost;

        buyCheckPanel.gameObject.SetActive(false);
        tradeShip.gameObject.SetActive(true);

        // 복사된 데이터를 사용하여 아이템 인스턴스 생성
        itemInstance = GameObjectFactory.Instance.CreateItemInstance(copiedItemData.id, copiedItemData.amount);

        // 아이템 인스턴스의 데이터를 복사된 것으로 교체
        itemInstance.GetComponent<TradingItem>()
            .Initialize(copiedItemData, copiedItemData.amount, copiedItemData.itemState);

        tradeStorage.AddItem(itemInstance, new Vector2Int(2, 1), Constants.Rotations.Rotation.Rotation0);

        buyConfirmButton.gameObject.SetActive(true);
        buyCancelButton.gameObject.SetActive(true);

        exitButton.gameObject.SetActive(false);
        panelBuy.SetActive(false);
        panelSell.SetActive(false);
    }

    /// <summary>
    /// 아이템을 임시 창고에서 꺼내 배치하고 구매를 최종적으로 확정하는 버튼을 눌렀을 때 호출되는 함수.
    /// 만약 임시 창고에 아이템이 남아있으면 경고 문구를 출력한다.
    /// 임시 창고에 아이템이 비어있다면 정상적으로 구매를 진행한다.
    /// </summary>
    public void OnBuyConfirmButtonClicked()
    {
        if (tradeStorage.storedItems.Count > 0)
        {
            notEmptyWarningText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEmptyWarningText, 1.5f));
            return;
        }

        tradeShip.gameObject.SetActive(false);
        buyConfirmButton.gameObject.SetActive(false);
        buyCancelButton.gameObject.SetActive(false);
        BuyItem(itemInstance.GetItemData());
        panelBuy.SetActive(true);
        panelSell.SetActive(true);
        exitButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 구매를 취소하는 버튼을 누르면 호출되는 함수.
    /// </summary>
    public void OnBuyCancelButtonClicked()
    {
        if (tradeStorage.storedItems.Any(i => i != itemInstance))
        {
            dontPlaceItemHereText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(dontPlaceItemHereText, 1.5f));
            return;
        }

        if (tradeStorage.storedItems.Contains(itemInstance))
        {
            tradeStorage.RemoveItem(itemInstance);
            Destroy(itemInstance.gameObject);
        }
        else
        {
            List<Room> allRooms = GameManager.Instance.playerShip.allRooms;
            foreach (Room room in allRooms)
                if (room is StorageRoomBase storageRoom)
                    if (storageRoom.storedItems.Contains(itemInstance))
                        storageRoom.DestroyItem(itemInstance);
        }


        tradeShip.gameObject.SetActive(false);
        buyConfirmButton.gameObject.SetActive(false);
        buyCancelButton.gameObject.SetActive(false);

        exitButton.gameObject.SetActive(true);

        panelBuy.SetActive(true);
        panelSell.SetActive(true);
    }

    /// <summary>
    /// 구매에 관련된 처리 함수.
    /// 구매한 아이템값을 제거하고, 행성의 총 구매액에 추가한다.
    /// </summary>
    /// <param name="selectedItem"></param>
    public void BuyItem(TradingItemData selectedItem)
    {
        int price = selectedItem.amount * selectedItem.boughtCost;

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -price);

        GameManager.Instance.WhereIAm().currentRevenue += price;

        UpdateBuyPanel();
    }

    /// <summary>
    /// 임시 창고용 배를 원하는 지점으로 옮긴다.
    /// </summary>
    public void MoveShipToPoint()
    {
        float zDistance = Mathf.Abs(mainCamera.transform.position.z - tradeShip.transform.position.z);
        Vector3 worldPos = mainCamera.ViewportToWorldPoint(new Vector3(normalizedTradeShipPosition.x,
            normalizedTradeShipPosition.y, zDistance));

        tradeShip.transform.position = worldPos;
    }

    #endregion

    #region 판매 패널 설정

    /// <summary>
    /// 판매 패널을 초기화한다.
    /// </summary>
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

    /// <summary>
    /// 판매 패널의 아이템 맵을 표시한다.
    /// </summary>
    /// <param name="selectedItem">선택한 아이템</param>
    public void ShowSellItemMap(TradingItemData selectedItem)
    {
        // 아이템 맵 활성화 및 초기화
        itemMapPanel.gameObject.SetActive(true);
        itemMapPanel.transform.position = itemMapSellPosition.position;
        itemMapPanel.SetIsPlanetItemMap(false);

        itemMapPanel.Initialize(selectedItem);
    }

    /// <summary>
    /// 아이템 맵을 숨기는 함수
    /// </summary>
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

    /// <summary>
    /// 판매 처리를 진행하는 함수.
    /// 현재 아이템의 시세에 맞게 아이템을 판매하고, 그만큼의 액수를 자금에 추가한다.
    /// </summary>
    /// <param name="selectedItem">선택한 아이템</param>
    public void SellItem(TradingItemData selectedItem)
    {
        TradingItem item = GameManager.Instance.playerShip.GetAllItems().Find(r => r.GetItemData() == selectedItem);

        if (item == null)
        {
            Debug.LogError("이 메시지 보이면 클난거임");
            return;
        }

        int price = GameManager.Instance.WhereIAm().GetItemPrice(selectedItem) * selectedItem.amount;

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, price);

        foreach (Room room in GameManager.Instance.playerShip.GetAllRooms())
            if (room is StorageRoomBase storageRoom && storageRoom.storedItems.Contains(item))
                storageRoom.DestroyItem(item);


        InitializeSellPanel();
        itemMapPanel.gameObject.SetActive(false);
    }

    #endregion

    #region 장비 패널 설정

    public void InitializeEquipmentPanel()
    {
        if (equipmentLeftInstance != null)
        {
            equipmentLeftInstance.transform.SetParent(null);
            Destroy(equipmentLeftInstance.gameObject);
        }

        if (equipmentMiddleInstance != null)
        {
            equipmentMiddleInstance.transform.SetParent(null);
            Destroy(equipmentMiddleInstance.gameObject);
        }

        if (equipmentRightInstance != null)
        {
            equipmentRightInstance.transform.SetParent(null);
            Destroy(equipmentRightInstance.gameObject);
        }


        PlanetData currentPlanet = GameManager.Instance.WhereIAm();

        EquipmentItem nextWeapon = EquipmentManager.Instance.GetNextLevelEquipment(EquipmentType.WeaponEquipment);

        if (nextWeapon != null)
        {
            EquipmentInfoPanel leftEquipmentInfoPanel =
                Instantiate(equipmentItemInfoPrefab, equipmentLeftContainer.transform)
                    .GetComponent<EquipmentInfoPanel>();

            leftEquipmentInfoPanel.Initialize(nextWeapon);
            leftEquipmentInfoPanel.panelButton.onClick.AddListener(OnClickWeaponEquipmentPanel);

            equipmentLeftInstance = leftEquipmentInfoPanel;
        }
        else
        {
            equipmentLeftSoldOutInstance.SetActive(true);
        }


        EquipmentItem nextShield = EquipmentManager.Instance.GetNextLevelEquipment(EquipmentType.ShieldEquipment);

        if (nextShield != null)
        {
            EquipmentInfoPanel middleEquipmentInfoPanel =
                Instantiate(equipmentItemInfoPrefab, equipmentMiddleContainer.transform)
                    .GetComponent<EquipmentInfoPanel>();

            middleEquipmentInfoPanel.Initialize(nextShield);
            middleEquipmentInfoPanel.panelButton.onClick.AddListener(OnClickShieldEquipmentPanel);

            equipmentMiddleInstance = middleEquipmentInfoPanel;
        }
        else
        {
            equipmentMiddleSoldOutInstance.SetActive(true);
        }

        if (!GameManager.Instance.isBoughtEquipment)
        {
            EquipmentItem randomItem = currentPlanet.currentRandomEquipmentItem;

            EquipmentInfoPanel rightEquipmentInfoPanel =
                Instantiate(equipmentItemInfoPrefab, equipmentRightContainer.transform)
                    .GetComponent<EquipmentInfoPanel>();
            rightEquipmentInfoPanel.Initialize(randomItem);

            rightEquipmentInfoPanel.panelButton.onClick.AddListener(OnClickRandomEquipmentPanel);

            equipmentRightInstance = rightEquipmentInfoPanel;
        }
        else
        {
            equipmentRightSoldOutInstance.SetActive(true);
        }
    }

    public void UpdateEquipmentPanel()
    {
        EquipmentItem nextWeapon = EquipmentManager.Instance.GetNextLevelEquipment(EquipmentType.WeaponEquipment);

        EquipmentInfoPanel targetPanel = null;
        if (nextWeapon == null)
        {
            targetPanel = equipmentLeftInstance;
            if (targetPanel != null)
            {
                targetPanel.gameObject.SetActive(false);

                targetPanel.HideTooltip();
            }

            equipmentLeftSoldOutInstance.SetActive(true);
        }
        else
        {
            if (equipmentLeftInstance != null)
            {
                targetPanel = equipmentLeftInstance;
                targetPanel.Initialize(nextWeapon);
                targetPanel.HideTooltip();
            }
        }

        EquipmentItem nextShield = EquipmentManager.Instance.GetNextLevelEquipment(EquipmentType.ShieldEquipment);

        if (nextShield == null)
        {
            equipmentMiddleSoldOutInstance.SetActive(true);
            if (equipmentMiddleInstance != null)
            {
                targetPanel = equipmentMiddleInstance;
                targetPanel.gameObject.SetActive(false);
                targetPanel.HideTooltip();
            }
        }
        else
        {
            targetPanel = equipmentMiddleInstance;
            targetPanel.Initialize(nextShield);
            targetPanel.HideTooltip();
        }

        if (GameManager.Instance.isBoughtEquipment)
        {
            equipmentRightSoldOutInstance.SetActive(true);
            if (equipmentRightInstance != null)
            {
                targetPanel = equipmentRightInstance;
                if (targetPanel != null)
                {
                    targetPanel.gameObject.SetActive(false);

                    targetPanel.HideTooltip();
                }
            }
        }
        else
        {
            targetPanel = equipmentRightInstance;
            targetPanel.Initialize(GameManager.Instance.WhereIAm().currentRandomEquipmentItem);
            targetPanel.HideTooltip();
        }
    }


    public void OnClickRandomEquipmentPanel()
    {
        BuyRandomEquipment(GameManager.Instance.WhereIAm().currentRandomEquipmentItem);
    }

    public void OnClickWeaponEquipmentPanel()
    {
        BuyEquipment(EquipmentManager.Instance.GetNextLevelEquipment(EquipmentType.WeaponEquipment));
    }

    public void OnClickShieldEquipmentPanel()
    {
        BuyEquipment(EquipmentManager.Instance.GetNextLevelEquipment(EquipmentType.ShieldEquipment));
    }

    public void BuyEquipment(EquipmentItem item)
    {
        int price = item.eqPrice;

        if (price > ResourceManager.Instance.COMA)
        {
            notEnoughCOMAText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughCOMAText, 1.5f));
            return;
        }

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -price);

        if (EquipmentManager.Instance.globalWeaponLevel2 == item)
        {
            GameManager.Instance.CurrentGlobalWeaponLevel = 2;
            GameManager.Instance.playerShip.unUsedItems.Add(EquipmentManager.Instance.globalWeaponLevel2);
        }
        else if (EquipmentManager.Instance.globalWeaponLevel3 == item)
        {
            GameManager.Instance.CurrentGlobalWeaponLevel = 3;
            GameManager.Instance.playerShip.unUsedItems.Add(EquipmentManager.Instance.globalWeaponLevel3);
        }

        else if (EquipmentManager.Instance.globalShieldLevel2 == item)
        {
            GameManager.Instance.CurrentGlobalShieldLevel = 2;
            GameManager.Instance.playerShip.unUsedItems.Add(EquipmentManager.Instance.globalShieldLevel2);
        }
        else if (EquipmentManager.Instance.globalShieldLevel3 == item)
        {
            GameManager.Instance.CurrentGlobalShieldLevel = 3;
            GameManager.Instance.playerShip.unUsedItems.Add(EquipmentManager.Instance.globalShieldLevel3);
        }

        equipmentGlobalApplyAllPanel.SetActive(true);
        recentBoughtEquipment = item;

        UpdateEquipmentPanel();
    }

    public void BuyRandomEquipment(EquipmentItem item)
    {
        int price = item.eqPrice;

        if (price > ResourceManager.Instance.COMA)
        {
            notEnoughCOMAText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughCOMAText, 1.5f));
            return;
        }

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -price);
        GameManager.Instance.playerShip.unUsedItems.Add(item);
        GameManager.Instance.isBoughtEquipment = true;

        UpdateEquipmentPanel();
    }

    public void OnClickApplyGlobalEquipmentConfirmButton()
    {
        equipmentGlobalApplyAllPanel.SetActive(false);
        if (recentBoughtEquipment == null)
            return;
        EquipmentManager.Instance.PurchaseAndEquipGlobal(recentBoughtEquipment);
        recentBoughtEquipment = null;
    }

    #endregion

    #region 자원 구매 패널 설정

    public void InitializeResourceBuyPanel()
    {
        PlanetData currentPlanet = GameManager.Instance.WhereIAm();
        fuelPrice.text = currentPlanet.currentFuelPrice.ToString();
        missilePrice.text = currentPlanet.currentMissilePrice.ToString();
        hypersonicPrice.text = currentPlanet.currentHypersonicPrice.ToString();
    }

    public void OnClickFuelBuyButton()
    {
        BuyFuel();
    }

    public void OnClickMissileBuyButton()
    {
        BuyMissile();
    }

    public void OnClickHypersonicBuyButton()
    {
        BuyHypersonic();
    }

    private void BuyFuel()
    {
        PlanetData currentPlanet = GameManager.Instance.WhereIAm();
        int price = currentPlanet.currentFuelPrice;

        if (price > ResourceManager.Instance.COMA)
        {
            notEnoughCOMAText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughCOMAText, 1.5f));
            return;
        }

        if (ResourceManager.Instance.Fuel == GameManager.Instance.playerShip.GetStat(ShipStat.FuelStoreCapacity))
        {
            notEnoughFuelCapacityText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughFuelCapacityText, 1.5f));
            return;
        }


        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -price);
        ResourceManager.Instance.ChangeResource(ResourceType.Fuel, 100);
    }

    private void BuyMissile()
    {
        PlanetData currentPlanet = GameManager.Instance.WhereIAm();
        int price = currentPlanet.currentMissilePrice;

        if (price > ResourceManager.Instance.COMA)
        {
            notEnoughCOMAText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughCOMAText, 1.5f));
            return;
        }

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -price);
        ResourceManager.Instance.ChangeResource(ResourceType.Missile, 1);
    }

    private void BuyHypersonic()
    {
        PlanetData currentPlanet = GameManager.Instance.WhereIAm();
        int price = currentPlanet.currentHypersonicPrice;

        if (price > ResourceManager.Instance.COMA)
        {
            notEnoughCOMAText.gameObject.SetActive(true);
            StartCoroutine(HideChangeText(notEnoughCOMAText, 1.5f));
            return;
        }

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -price);
        ResourceManager.Instance.ChangeResource(ResourceType.Hypersonic, 1);
    }

    #endregion
}
