using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Analytics;
using UnityEngine.Serialization;

public class SlidePanelController : MonoBehaviour
{
    [Header("탭 버튼")] [SerializeField] private Button buttonShip;
    [SerializeField] private Button buttonCrew;
    [SerializeField] private Button buttonQuest;
    [SerializeField] private Button buttonStorage;

    [Header("전체 탭")] [SerializeField] private GameObject Tabs;

    [Header("패널 목록")] [SerializeField] private GameObject panelShip;
    [SerializeField] private GameObject panelCrew;
    [SerializeField] private GameObject panelQuest;
    [SerializeField] private GameObject panelStorage;
    [SerializeField] private GameObject panelUnselected;

    [Header("함선 패널 설정")] [SerializeField] private GameObject shipPanelContent;
    [SerializeField] private GameObject roomInfoPanelPrefab;
    private Dictionary<RoomType, ShipInfoPanel> shipInfoPanelDictionary = new();


    [Header("승무원 패널 설정")] [SerializeField] private GameObject crewPanelContent;
    [SerializeField] private GameObject crewInfoPanelPrefab;
    [SerializeField] private GameObject equipmentDetailPanel;
    private List<CrewInfoPanel> crewInfoPanelList = new();


    [FormerlySerializedAs("qeustPanelContent")] [Header("퀘스트 패널 설정")] [SerializeField]
    private GameObject questPanelContent;

    [SerializeField] private GameObject questInfoPanelPrefab;
    [SerializeField] private GameObject checkQuestUI;
    private List<QuestInfoPanel> questInfoPanelList = new();
    private RandomQuest questToCancel;

    [Header("창고 패널 설정")] [SerializeField] private GameObject storagePanelContent;
    [SerializeField] private GameObject storageInfoPanelPrefab;
    [SerializeField] private ItemMapController itemMapPanel;
    private List<StorageInfoPanel> storageInfoPanelInstance = new();
    private StorageInfoPanel selectedStoragePanel = null;


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
        {
            if (equipmentDetailPanel.activeInHierarchy || checkQuestUI.activeInHierarchy ||
                itemMapPanel.gameObject.activeInHierarchy)
                return;

            // 클릭된 UI 요소가 현재 패널이 아닌지 체크
            if (!IsClickingOnSelf())
                SlideClose();
        }
    }

    private void OnEnable()
    {
    }

    private void AddButtonListeners()
    {
        buttonShip.onClick.AddListener(() => OnPanelButtonClicked(panelShip));
        buttonCrew.onClick.AddListener(() => OnPanelButtonClicked(panelCrew));
        buttonQuest.onClick.AddListener(() => OnPanelButtonClicked(panelQuest));
        buttonStorage.onClick.AddListener(() => OnPanelButtonClicked(panelStorage));
    }

    public void OnPanelButtonClicked(GameObject targetPanel)
    {
        if (currentPanel == targetPanel && isOpen)
        {
            SlideClose();
            return;
        }

        if (targetPanel == panelShip) InitializeShipPanel();
        if (targetPanel == panelCrew) InitializeCrewPanel();
        if (targetPanel == panelQuest) InitializeQuestPanel();
        if (targetPanel == panelStorage) InitializeStoragePanel();

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

        SetSelectedStoragePanel(null);
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
        panelShip.SetActive(false);
        panelCrew.SetActive(false);
        panelQuest.SetActive(false);
        panelStorage.SetActive(false);
        panelToShow.SetActive(true);
        currentPanel = panelToShow;
    }

    #region 함선 패널 설정

    private void InitializeShipPanel()
    {
        foreach (ShipInfoPanel panel in shipInfoPanelDictionary.Values) Destroy(panel.gameObject);

        shipInfoPanelDictionary.Clear();


        foreach (Room room in GameManager.Instance.playerShip.allRooms)
        {
            if (room.roomType == RoomType.Corridor || room.roomType == RoomType.Storage)
                continue;

            if (!shipInfoPanelDictionary.ContainsKey(room.roomType))
            {
                ShipInfoPanel shipInfoPanel = Instantiate(roomInfoPanelPrefab, shipPanelContent.transform)
                    .GetComponent<ShipInfoPanel>();
                shipInfoPanel.Initialize(room);
                shipInfoPanelDictionary.Add(room.roomType, shipInfoPanel);
            }
            else
            {
                shipInfoPanelDictionary[room.roomType].AddInfo(room);
            }
        }

        List<Transform> shipPanelTransforms = new();

        foreach (Transform child in shipPanelContent.transform)
            shipPanelTransforms.Add(child);

        shipPanelTransforms = shipPanelTransforms
            .OrderByDescending(d => d.GetComponent<ShipInfoPanel>().currentDamageLevel).ToList();

        for (int i = 0; i < shipPanelTransforms.Count; i++) shipPanelTransforms[i].SetSiblingIndex(i);
    }

    #endregion


    #region 승무원 패널 설정

    public void InitializeCrewPanel()
    {
        /*
        for (int index = crewInfoPanelList.Count - 1; index >= 0; index--)
        {
            CrewInfoPanel infoPanel = crewInfoPanelList[index];
            crewInfoPanelList.Remove(infoPanel);
            Destroy(infoPanel.gameObject);
        }
        */
        foreach (CrewInfoPanel cip in crewInfoPanelList)
            Destroy(cip.gameObject);
        crewInfoPanelList.Clear();

        foreach (CrewMember crew in GameManager.Instance.playerShip.allCrews)
        {
            CrewInfoPanel crewInfoPanel = Instantiate(crewInfoPanelPrefab, crewPanelContent.transform)
                .GetComponent<CrewInfoPanel>();
            crewInfoPanel.Initialize(crew);
            crewInfoPanelList.Add(crewInfoPanel);
        }
    }

    public void OnCrewPanelEquipmentButtonClicked(CrewMember selectedCrew)
    {
        EquipmentApplyHandler handler = equipmentDetailPanel.GetComponent<EquipmentApplyHandler>();
        handler.Initialize();
        handler.OnCrewIconSelected(selectedCrew);

        equipmentDetailPanel.SetActive(true);
    }

    #endregion

    #region 퀘스트 패널 설정

    private void InitializeQuestPanel()
    {
        foreach (QuestInfoPanel panel in questInfoPanelList) Destroy(panel.gameObject);

        questInfoPanelList.Clear();


        foreach (PlanetData planet in GameManager.Instance.PlanetDataList)
        foreach (RandomQuest quest in planet.questList)
        {
            QuestInfoPanel questInfoPanel = Instantiate(questInfoPanelPrefab, questPanelContent.transform)
                .GetComponent<QuestInfoPanel>();

            questInfoPanel.Initialize(quest);
            questInfoPanelList.Add(questInfoPanel);
        }

        List<Transform> questPanelTransforms = new();

        foreach (Transform child in questPanelContent.transform) questPanelTransforms.Add(child);


        questPanelTransforms = questPanelTransforms
            .OrderBy(q => q.GetComponent<QuestInfoPanel>().currentQuest.GetCanComplete()).ToList();

        for (int i = 0; i < questPanelTransforms.Count; i++) questPanelTransforms[i].SetSiblingIndex(i);
    }

    public void RequestQuestCancel(RandomQuest quest)
    {
        checkQuestUI.SetActive(true);
        questToCancel = quest;
    }

    public void OnQuestCancelConfirmButtonClicked()
    {
        PlanetData planetData =
            GameManager.Instance.PlanetDataList.Find(d => d.questList.Contains(questToCancel));

        planetData.FailQuest(questToCancel);
        questToCancel = null;
        InitializeQuestPanel();
    }

    #endregion

    #region 창고 패널 설정

    public void InitializeStoragePanel()
    {
        // Clear storage panel selection when changing tabs
        SetSelectedStoragePanel(null);

        foreach (StorageInfoPanel panel in storageInfoPanelInstance) Destroy(panel.gameObject);

        storageInfoPanelInstance.Clear();

        List<TradingItem> allItems = GameManager.Instance.playerShip.GetAllItems();

        foreach (TradingItem item in allItems)
        {
            StorageInfoPanel storageInfoPanel = Instantiate(storageInfoPanelPrefab, storagePanelContent.transform)
                .GetComponent<StorageInfoPanel>();

            storageInfoPanel.Initialize(item);
            storageInfoPanelInstance.Add(storageInfoPanel);
        }
    }

    public void OnStorageInfoPanelButtonClicked(TradingItemData selectedItem)
    {
        // 이미 선택된 패널의 아이템과 동일한지 확인
        if (itemMapPanel.gameObject.activeInHierarchy &&
            selectedStoragePanel != null &&
            selectedStoragePanel.CurrentItem == selectedItem)
            // 이미 같은 아이템 맵이 열려있으면 아무 작업도 하지 않음
            return;

        // 새 아이템이거나 패널이 닫혀있으면 맵 활성화 및 초기화
        itemMapPanel.gameObject.SetActive(true);
        itemMapPanel.Initialize(selectedItem);
    }

    public void SetSelectedStoragePanel(StorageInfoPanel panel)
    {
        if (selectedStoragePanel != null) selectedStoragePanel.SetSelected(false);

        selectedStoragePanel = panel;
        if (selectedStoragePanel != null) selectedStoragePanel.SetSelected(true);
    }

    #endregion
}
