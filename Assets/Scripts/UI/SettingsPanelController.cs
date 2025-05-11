using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Volume Controls")] [SerializeField]
    private Slider bgmSlider;

    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI bgmValueText;
    [SerializeField] private TextMeshProUGUI sfxValueText;
    [SerializeField] private int volumeSteps = 10; // 10단계로 설정

    [Header("Language Controls")] [SerializeField]
    private TMP_Dropdown languageDropdown;

    [Header("Buttons")] [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    // 현재 설정값 (임시 저장)
    private float currentBgmVolume;
    private float currentSfxVolume;
    private SystemLanguage currentLanguage;

    // 원래 설정값 (취소 시 복원용)
    private float originalBgmVolume;
    private float originalSfxVolume;
    private SystemLanguage originalLanguage;

    private void Start()
    {
        // 버튼 이벤트 설정
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmSettings);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelSettings);

        // 언어 드롭다운 초기화
        if (languageDropdown != null)
            InitializeLanguageDropdown();
    }

    private void OnEnable()
    {
        // 설정창이 활성화될 때마다 초기화
        InitializeSettings();
    }

    // 설정창 초기화
    private void InitializeSettings()
    {
        // 볼륨 설정 초기화
        InitializeVolumeSettings();

        // 언어 설정 초기화
        InitializeLanguageSettings();
    }

    #region Volume Settings

    // 볼륨 설정 초기화
    private void InitializeVolumeSettings()
    {
        // AudioManager에서 현재 볼륨 값 가져오기
        if (AudioManager.Instance != null)
        {
            originalBgmVolume = AudioManager.Instance.GetBGMVolume();
            originalSfxVolume = AudioManager.Instance.GetSFXVolume();
        }
        else
        {
            // AudioManager가 없을 경우 기본값 사용
            originalBgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            originalSfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }

        // 단계에 맞게 조정
        originalBgmVolume = RoundToStep(originalBgmVolume);
        originalSfxVolume = RoundToStep(originalSfxVolume);

        // 현재 값 초기화
        currentBgmVolume = originalBgmVolume;
        currentSfxVolume = originalSfxVolume;

        // 슬라이더 설정
        SetupVolumeSliders();
    }

    // 볼륨 슬라이더 초기화 및 이벤트 설정
    private void SetupVolumeSliders()
    {
        // BGM 슬라이더 설정
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners();
            bgmSlider.value = currentBgmVolume;
            bgmSlider.onValueChanged.AddListener(OnBgmSliderChanged);
            UpdateBGMValueText(currentBgmVolume);
        }

        // SFX 슬라이더 설정
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = currentSfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            UpdateSFXValueText(currentSfxVolume);
        }
    }

    // BGM 슬라이더 변경 이벤트
    private void OnBgmSliderChanged(float value)
    {
        // 단계별 값으로 변환
        float steppedValue = RoundToStep(value);

        // 슬라이더 값 설정 (중복 호출 방지)
        if (bgmSlider.value != steppedValue)
            bgmSlider.value = steppedValue;

        // 현재 임시 값 업데이트
        currentBgmVolume = steppedValue;

        // AudioManager를 통해 오디오 믹서 업데이트 (즉시 들리도록)
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(steppedValue);

        // 텍스트 업데이트
        UpdateBGMValueText(steppedValue);
    }

    // SFX 슬라이더 변경 이벤트
    private void OnSfxSliderChanged(float value)
    {
        // 단계별 값으로 변환
        float steppedValue = RoundToStep(value);

        // 슬라이더 값 설정
        if (sfxSlider.value != steppedValue)
            sfxSlider.value = steppedValue;

        // 현재 임시 값 업데이트
        currentSfxVolume = steppedValue;

        // AudioManager를 통해 오디오 믹서 업데이트 (즉시 들리도록)
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(steppedValue);

        // 텍스트 업데이트
        UpdateSFXValueText(steppedValue);
    }

    // 값을 단계별로 변환하는 함수
    private float RoundToStep(float value)
    {
        // 0-1 사이의 값을 0-volumeSteps 사이의 단계로 변환 후 다시 0-1로 정규화
        int step = Mathf.RoundToInt(value * volumeSteps);
        return (float)step / volumeSteps;
    }

    // BGM 값 텍스트 업데이트
    private void UpdateBGMValueText(float value)
    {
        if (bgmValueText != null)
        {
            // 0-10 단계로 표시
            int volumeLevel = Mathf.RoundToInt(value * volumeSteps);
            bgmValueText.text = volumeLevel.ToString();
        }
    }

    // SFX 값 텍스트 업데이트
    private void UpdateSFXValueText(float value)
    {
        if (sfxValueText != null)
        {
            // 0-10 단계로 표시
            int volumeLevel = Mathf.RoundToInt(value * volumeSteps);
            sfxValueText.text = volumeLevel.ToString();
        }
    }

    #endregion

    #region Language Settings

    // 언어 드롭다운 초기화
    private void InitializeLanguageDropdown()
    {
        if (languageDropdown == null || !LocalizationManager.IsInitialized)
            return;

        // 드롭다운 옵션 초기화
        languageDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();

        // 지원하는 언어별로 옵션 추가
        foreach (SystemLanguage language in LocalizationManager.SupportedLanguages)
        {
            // 언어 이름을 표시용 텍스트로 변환
            string languageName = GetLanguageDisplayName(language);
            options.Add(new TMP_Dropdown.OptionData(languageName));
        }

        languageDropdown.AddOptions(options);

        // 값 변경 이벤트는 InitializeLanguageSettings에서 설정
    }

    // 언어 설정 초기화
    private void InitializeLanguageSettings()
    {
        if (languageDropdown == null || !LocalizationManager.IsInitialized)
            return;

        // 현재 언어 저장
        originalLanguage = LocalizationManager.CurrentLanguage;
        currentLanguage = originalLanguage;

        // 이벤트 리스너 설정 (중복 방지)
        languageDropdown.onValueChanged.RemoveAllListeners();

        // 드롭다운 값 설정
        int index = LocalizationManager.SupportedLanguages.IndexOf(currentLanguage);
        if (index >= 0)
            languageDropdown.value = index;

        // 이벤트 리스너 추가
        languageDropdown.onValueChanged.AddListener(OnLanguageSelected);
    }

    // 드롭다운에서 언어 선택 시 호출
    private void OnLanguageSelected(int index)
    {
        if (index >= 0 && index < LocalizationManager.SupportedLanguages.Count)
        {
            // 임시 언어 설정 변경 (저장은 확인 버튼 클릭 시)
            currentLanguage = LocalizationManager.SupportedLanguages[index];

            // 즉시 언어 변경하여 미리보기 제공
            LocalizationManager.ChangeLanguage(currentLanguage);
        }
    }

    // 언어 enum을 표시용 이름으로 변환
    private string GetLanguageDisplayName(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.Korean:
                return "한국어";
            case SystemLanguage.English:
                return "English";
            default:
                return language.ToString();
        }
    }

    #endregion

    #region Confirm/Cancel Buttons

    // 확인 버튼 이벤트 - 설정 저장
    public void ConfirmSettings()
    {
        // 볼륨 설정 저장
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SaveVolumeSettings(currentBgmVolume, currentSfxVolume);
        }
        else
        {
            // AudioManager가 없을 경우 직접 저장
            PlayerPrefs.SetFloat("BGMVolume", currentBgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", currentSfxVolume);
            PlayerPrefs.Save();
        }

        // 언어 설정 저장
        if (LocalizationManager.IsInitialized)
            // 현재 언어를 LocalizationManager에 저장 (내부적으로 PlayerPrefs에 저장됨)
            LocalizationManager.ChangeLanguage(currentLanguage);

        // 원래 값 업데이트 (다음 취소를 위해)
        originalBgmVolume = currentBgmVolume;
        originalSfxVolume = currentSfxVolume;
        originalLanguage = currentLanguage;

        Debug.Log("설정이 저장되었습니다!");

        // 설정창 닫기
        CloseSettingsPanel();
    }

    // 취소 버튼 이벤트 - 원래 설정으로 복원
    public void CancelSettings()
    {
        // 볼륨 설정 복원
        currentBgmVolume = originalBgmVolume;
        currentSfxVolume = originalSfxVolume;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(originalBgmVolume);
            AudioManager.Instance.SetSFXVolume(originalSfxVolume);
        }

        // 볼륨 UI 업데이트
        if (bgmSlider != null)
            bgmSlider.value = originalBgmVolume;

        if (sfxSlider != null)
            sfxSlider.value = originalSfxVolume;

        // 언어 설정 복원
        currentLanguage = originalLanguage;

        if (LocalizationManager.IsInitialized)
        {
            // 원래 언어로 다시 변경
            LocalizationManager.ChangeLanguage(originalLanguage);

            // 드롭다운 값 업데이트
            int index = LocalizationManager.SupportedLanguages.IndexOf(originalLanguage);
            if (index >= 0 && languageDropdown != null)
                languageDropdown.value = index;
        }

        Debug.Log("설정이 취소되었습니다!");

        // 설정창 닫기
        CloseSettingsPanel();
    }

    // 설정창 닫기
    private void CloseSettingsPanel()
    {
        // UI Manager를 통해 설정창 닫기
        if (UIManager.Instance != null)
            UIManager.Instance.SetSettingsPanelActive(false);
        else
            // UI Manager가 없으면 직접 비활성화
            gameObject.SetActive(false);
    }

    #endregion
}
