using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 승무원 UI 참조
    [SerializeField] private Transform crewContainer;
    [SerializeField] private GameObject crewPrefab;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameplayPanel;

    // UI 패널 참조
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;

    // 리소스 UI 참조
    [SerializeField] private Transform resourceContainer;
    [SerializeField] private GameObject resourcePrefab;

    // 선박 시스템 UI 참조
    [SerializeField] private Transform systemContainer;
    [SerializeField] private GameObject systemPrefab;
    private readonly Dictionary<int, GameObject> crewObjects = new();

    private readonly Dictionary<ResourceType, TextMeshProUGUI> resourceTexts = new();
    private readonly Dictionary<string, GameObject> systemObjects = new();
    public static UIManager Instance { get; private set; }

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
        // 게임 상태 변경 이벤트 구독
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;

        // 자원 변경 이벤트 구독
        ResourceManager.Instance.OnResourceChanged += UpdateResourceUI;

        // 승무원 변경 이벤트 구독
        CrewManager.Instance.OnCrewChanged += UpdateCrewUI;

        // 선박 시스템 변경 이벤트 구독
        ShipManager.Instance.OnShipSystemChanged += UpdateSystemUI;

        // 초기 UI 설정
        InitializeResourceUI();
        InitializeCrewUI();
        InitializeSystemUI();

        // 첫 화면 설정
        SetActivePanel(GameState.MainMenu);
    }

    // 게임 상태에 따라 활성화할 패널 설정
    private void OnGameStateChanged(GameState newState)
    {
        SetActivePanel(newState);
    }

    private void SetActivePanel(GameState state)
    {
        mainMenuPanel.SetActive(state == GameState.MainMenu);
        gameplayPanel.SetActive(state == GameState.Gameplay || state == GameState.Event);
        pausePanel.SetActive(state == GameState.Paused);
        gameOverPanel.SetActive(state == GameState.GameOver);
    }

    // 자원 UI 초기화
    private void InitializeResourceUI()
    {
        // 기존 자원 UI 제거
        foreach (Transform child in resourceContainer) Destroy(child.gameObject);

        resourceTexts.Clear();

        // 모든 자원 유형에 대한 UI 생성
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            var resourceObj = Instantiate(resourcePrefab, resourceContainer);
            var nameText = resourceObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            var valueText = resourceObj.transform.Find("ValueText").GetComponent<TextMeshProUGUI>();

            nameText.text = type + ":";
            valueText.text = ResourceManager.Instance.GetResource(type).ToString();

            resourceTexts[type] = valueText;
        }
    }

    // 승무원 UI 초기화
    private void InitializeCrewUI()
    {
        // 기존 승무원 UI 제거
        foreach (Transform child in crewContainer) Destroy(child.gameObject);

        crewObjects.Clear();

        // 모든 승무원에 대한 UI 생성
        for (var i = 0; i < CrewManager.Instance.GetAliveCrewCount(); i++)
        {
            var crewMember = CrewManager.Instance.GetCrewMember(i);
            if (crewMember != null) CreateCrewUI(i, crewMember);
        }
    }

    // 선박 시스템 UI 초기화
    private void InitializeSystemUI()
    {
        // 기존 시스템 UI 제거
        foreach (Transform child in systemContainer) Destroy(child.gameObject);

        systemObjects.Clear();

        // 모든 선박 시스템에 대한 UI 생성
        foreach (var system in ShipManager.Instance.GetAllSystems()) CreateSystemUI(system);
    }

    // 자원 UI 업데이트
    private void UpdateResourceUI(ResourceType type, int newAmount)
    {
        if (resourceTexts.TryGetValue(type, out var valueText)) valueText.text = newAmount.ToString();
    }

    // 승무원 UI 업데이트
    private void UpdateCrewUI(int crewIndex, CrewManager.CrewMember crewMember)
    {
        if (crewObjects.TryGetValue(crewIndex, out var crewObj))
        {
            // 기존 UI 업데이트
            var nameText = crewObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            var statusText = crewObj.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
            var healthSlider = crewObj.transform.Find("HealthSlider").GetComponent<Slider>();

            nameText.text = crewMember.name;
            statusText.text = crewMember.status.ToString();
            healthSlider.value = crewMember.health / crewMember.maxHealth;

            // 죽은 승무원 시각적 표시
            if (!crewMember.isAlive)
            {
                crewObj.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                statusText.text = "Dead";
            }
            else
            {
                crewObj.GetComponent<Image>().color = Color.white;
            }
        }
        else if (crewMember.isAlive)
        {
            // 새 승무원 UI 생성
            CreateCrewUI(crewIndex, crewMember);
        }
    }

    private void CreateCrewUI(int index, CrewManager.CrewMember crewMember)
    {
        var crewObj = Instantiate(crewPrefab, crewContainer);
        var nameText = crewObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var statusText = crewObj.transform.Find("StatusText").GetComponent<TextMeshProUGUI>();
        var healthSlider = crewObj.transform.Find("HealthSlider").GetComponent<Slider>();

        nameText.text = crewMember.name;
        statusText.text = crewMember.status.ToString();
        healthSlider.value = crewMember.health / crewMember.maxHealth;

        crewObjects[index] = crewObj;
    }

    // 선박 시스템 UI 업데이트
    private void UpdateSystemUI(int systemIndex, ShipManager.ShipSystem system)
    {
        if (systemObjects.TryGetValue(system.name, out var systemObj))
        {
            // 기존 UI 업데이트
            var nameText = systemObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
            var levelText = systemObj.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
            var healthSlider = systemObj.transform.Find("HealthSlider").GetComponent<Slider>();

            nameText.text = system.name;
            levelText.text = "Lv. " + system.level;
            healthSlider.value = system.health / system.maxHealth;

            // 비활성 시스템 시각적 표시
            if (!system.isActive)
                systemObj.GetComponent<Image>().color = new Color(1f, 0.3f, 0.3f, 0.8f);
            else
                systemObj.GetComponent<Image>().color = Color.white;
        }
        else
        {
            // 새 시스템 UI 생성
            CreateSystemUI(system);
        }
    }

    private void CreateSystemUI(ShipManager.ShipSystem system)
    {
        var systemObj = Instantiate(systemPrefab, systemContainer);
        var nameText = systemObj.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        var levelText = systemObj.transform.Find("LevelText").GetComponent<TextMeshProUGUI>();
        var healthSlider = systemObj.transform.Find("HealthSlider").GetComponent<Slider>();
        var upgradeButton = systemObj.transform.Find("UpgradeButton").GetComponent<Button>();

        nameText.text = system.name;
        levelText.text = "Lv. " + system.level;
        healthSlider.value = system.health / system.maxHealth;

        // 업그레이드 버튼 이벤트 등록
        upgradeButton.onClick.AddListener(() => ShipManager.Instance.UpgradeSystem(system.name));

        systemObjects[system.name] = systemObj;
    }
}
