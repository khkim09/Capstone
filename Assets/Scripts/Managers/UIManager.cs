using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 상태별 UI 컨트롤러 참조
    [SerializeField] private GameObject warpUIControllerPrefab;

    public WarpUIController activeWarpUIController;

    // 여기에다가 각 UI 만들 때마다 컨트롤러 등록

    // 상태와 무관한 공통 UI 요소들
    [SerializeField] private GameObject commonHUD;
    [SerializeField] private GameObject settingsCanvas; // 설정창 캔버스 참조
    private GameObject persistentSettingsCanvas; // DontDestroy된 설정창 참조

    // 언어 설정 UI 요소 목록
    private List<GameObject> languageSettingsUIElements = new();

    // 언어 설정 UI 태그
    [SerializeField] private string languageSettingsTag = "LanguageSettings";

    // 메인 메뉴 씬 이름
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    /// <summary>
    /// 인스턴스를 초기화하고 싱글톤을 설정합니다.
    /// </summary>
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 전환 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeSettingsCanvas();
    }

    // 씬이 로드될 때마다 호출
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 설정창이 초기화된 후에 실행해야 함
        if (persistentSettingsCanvas != null)
        {
            // 언어 설정 UI 요소 다시 찾기 (새로운 씬에 있을 수 있음)
            FindLanguageSettingsUIElements();

            // 현재 씬이 메인 메뉴인지 확인하고 언어 설정 UI 표시 여부 설정
            UpdateLanguageSettingsVisibility(scene.name == mainMenuSceneName);
        }
    }

    private void InitializeSettingsCanvas()
    {
        // 씬에서 설정창 찾기 (또는 프리팹에서 인스턴스화)
        GameObject foundCanvas = settingsCanvas;

        if (foundCanvas == null) foundCanvas = GameObject.FindWithTag("SettingPanel");

        if (foundCanvas != null)
        {
            // 찾은 설정창을 DontDestroyOnLoad로 설정
            persistentSettingsCanvas = foundCanvas;
            DontDestroyOnLoad(persistentSettingsCanvas);

            // 언어 설정 UI 요소 찾기
            FindLanguageSettingsUIElements();

            // 초기에는 숨김
            persistentSettingsCanvas.SetActive(false);

            // 현재 씬에 따라 언어 설정 UI 보이기/숨기기
            UpdateLanguageSettingsVisibility(SceneManager.GetActiveScene().name == mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("설정창을 찾을 수 없습니다!");
        }
    }

    // 언어 설정 UI 요소들 찾기
    private void FindLanguageSettingsUIElements()
    {
        // 기존 목록 초기화
        languageSettingsUIElements.Clear();

        // 태그로 모든 언어 설정 UI 요소 찾기
        GameObject[] languageUIObjects = GameObject.FindGameObjectsWithTag(languageSettingsTag);

        if (languageUIObjects.Length > 0)
        {
            // 찾은 모든 요소를 목록에 추가
            languageSettingsUIElements.AddRange(languageUIObjects);
            Debug.Log($"언어 설정 UI 요소 {languageUIObjects.Length}개 찾음");
        }
        else
        {
            Debug.LogWarning($"\"{languageSettingsTag}\" 태그를 가진 언어 설정 UI 요소를 찾을 수 없습니다.");
        }
    }

    // 메인 메뉴 여부에 따라 언어 설정 UI 표시/숨김
    private void UpdateLanguageSettingsVisibility(bool isMainMenu)
    {
        foreach (GameObject uiElement in languageSettingsUIElements)
            if (uiElement != null)
                uiElement.SetActive(isMainMenu);
    }

    /// <summary>
    /// 게임 상태 전환 시, 해당 상태에 맞는 UI를 활성화합니다.
    /// 이전 UI는 모두 비활성화됩니다.
    /// </summary>
    /// <param name="stateType">전환할 게임 상태.</param>
    public void SwitchToGameState(GameState stateType)
    {
        // 모든 UI 컨트롤러 비활성화
        DisalbeAllUI();

        // 해당 상태 UI 활성화
        switch (stateType)
        {
            case GameState.Warp:
                // true로 설정
                warpUIControllerPrefab.SetActive(true);
                break;
        }
    }

    /// <summary>
    /// 현재 활성화된 모든 UI 컨트롤러를 비활성화합니다.
    /// </summary>
    public void DisalbeAllUI()
    {
        warpUIControllerPrefab.SetActive(false);
        // 여기에다가 각 UI컨트롤러 만들 때마다 비활성화 코드 추가
    }

    /// <summary>
    /// 워프 UI 컨트롤러를 가져오거나, 없으면 새로 생성합니다.
    /// </summary>
    /// <returns>활성화된 WarpUIController 인스턴스.</returns>
    public WarpUIController GetOrCreateWarpUIController()
    {
        if (activeWarpUIController == null)
        {
            GameObject controllerObj = Instantiate(warpUIControllerPrefab);
            activeWarpUIController = controllerObj.GetComponent<WarpUIController>();
        }

        return activeWarpUIController;
    }

    // 설정창 토글
    public void ToggleSettingsPanel()
    {
        if (persistentSettingsCanvas != null)
        {
            bool isActive = !persistentSettingsCanvas.activeSelf;
            persistentSettingsCanvas.SetActive(isActive);

            // 활성화된 경우, 현재 씬이 메인 메뉴인지 확인하여 언어 설정 표시 여부 결정
            if (isActive)
                UpdateLanguageSettingsVisibility(SceneManager.GetActiveScene().name == mainMenuSceneName);
        }
    }

    // 설정창 활성화/비활성화
    public void SetSettingsPanelActive(bool active)
    {
        if (persistentSettingsCanvas != null)
        {
            persistentSettingsCanvas.SetActive(active);

            // 활성화된 경우, 현재 씬이 메인 메뉴인지 확인하여 언어 설정 표시 여부 결정
            if (active)
                UpdateLanguageSettingsVisibility(SceneManager.GetActiveScene().name == mainMenuSceneName);
        }
    }

    // 씬 전환 시 수동으로 언어 설정 요소 업데이트
    public void RefreshLanguageSettingsElements()
    {
        FindLanguageSettingsUIElements();
        UpdateLanguageSettingsVisibility(SceneManager.GetActiveScene().name == mainMenuSceneName);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
