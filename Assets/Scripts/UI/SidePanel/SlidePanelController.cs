using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.Analytics;

public class SlidePanelController : MonoBehaviour
{
    [Header("탭 버튼")][SerializeField] private Button buttonShip;
    [SerializeField] private Button buttonCrew;
    [SerializeField] private Button buttonQuest;
    [SerializeField] private Button buttonStorage;

    [Header("전체 탭")][SerializeField] private GameObject Tabs;

    [Header("패널 목록")][SerializeField] private GameObject panelShip;
    [SerializeField] private GameObject panelCrew;
    [SerializeField] private GameObject panelQuest;
    [SerializeField] private GameObject panelStorage;
    [SerializeField] private GameObject panelUnselected;

    [Header("함선 패널 설정")][SerializeField] private GameObject shipPanelContent;
    [SerializeField] private GameObject roomInfoPanelPrefab;

    [Header("승무원 패널 설정")][SerializeField] private GameObject crewPanelContent;
    [SerializeField] private GameObject crewInfoPanelPrefab;
    [SerializeField] private GameObject equipmentDetailPanel;


    [Header("열려야 되는 위치")][SerializeField] private Transform openedPosition;
    [Header("열리는 속도")][SerializeField] private float slideSpeed = 0.1f;

    private Vector3 closedPosition;
    private Coroutine slideCoroutine;
    private bool isOpen = false;
    private GameObject currentPanel = null;

    private List<CrewInfoPanel> crewInfoPanelList = new();
    private Dictionary<RoomType, ShipInfoPanel> shipInfoPanelDictionary = new();

    private void Start()
    {
        closedPosition = Tabs.transform.position; // 시작 위치 저장
        AddButtonListeners();
    }

    private void Update()
    {
        if (isOpen && Input.GetMouseButtonDown(0))
        {
            if (equipmentDetailPanel.activeInHierarchy)
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
                Debug.Log(room.roomType + "더함!");
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
            CrewInfoPanel crewInfoPanel = Instantiate(crewInfoPanelPrefab, crewPanelContent.transform).GetComponent<CrewInfoPanel>();
            crewInfoPanel.Initialize(crew);
            crewInfoPanelList.Add(crewInfoPanel);
        }
    }

    #endregion

    public void OnCrewPanelEquipmentButtonClicked(CrewMember selectedCrew)
    {
        EquipmentApplyHandler handler = equipmentDetailPanel.GetComponent<EquipmentApplyHandler>();
        handler.Initialize();
        handler.OnCrewIconSelected(selectedCrew);

        equipmentDetailPanel.SetActive(true);
    }
}
