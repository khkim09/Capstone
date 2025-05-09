using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

// 게임 매니저와 통합할 수 있는 정적 클래스 버전의 LocalizationManager
public static class LocalizationManager
{
    // 현재 선택된 언어
    private static SystemLanguage _currentLanguage = SystemLanguage.English;

    public static SystemLanguage CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                OnLanguageChanged?.Invoke(_currentLanguage);
            }
        }
    }

    // 언어 변경 이벤트
    public delegate void LanguageChangedHandler(SystemLanguage newLanguage);

    public static event LanguageChangedHandler OnLanguageChanged;

    // 로컬라이제이션 데이터
    private static Dictionary<string, Dictionary<string, string>> localizedTexts = new();

    // 초기화 완료 여부
    private static bool isInitialized = false;

    // 지원하는 언어 목록
    public static List<SystemLanguage> SupportedLanguages { get; } = new()
    {
        SystemLanguage.English, SystemLanguage.Korean
    };

    // 매니저 초기화 (게임 매니저 Start에서 호출)
    public static void Initialize(MonoBehaviour coroutineRunner)
    {
        if (isInitialized)
            return;

        // 시스템 언어 감지 또는 저장된 설정 불러오기
        DetectLanguage();

        // 언어 파일 로딩
        string filePath = Path.Combine(Application.streamingAssetsPath, "Language.json");

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            coroutineRunner.StartCoroutine(LoadLocalizedTextWebGL(filePath));
        else
            LoadLocalizedText(filePath);

        isInitialized = true;
    }

    // 시스템 언어 감지 또는 설정 불러오기
    private static void DetectLanguage()
    {
        // PlayerPrefs에서 저장된 언어 설정 확인
        if (PlayerPrefs.HasKey("Language"))
        {
            string savedLanguage = PlayerPrefs.GetString("Language");
            if (Enum.TryParse(savedLanguage, out SystemLanguage language))
            {
                _currentLanguage = language;
                return;
            }
        }

        // 저장된 설정이 없으면 시스템 언어 감지
        SystemLanguage deviceLanguage = Application.systemLanguage;

        // 지원하는 언어인지 확인
        if (SupportedLanguages.Contains(deviceLanguage))
            _currentLanguage = deviceLanguage;
        else
            // 기본 언어로 영어 설정
            _currentLanguage = SystemLanguage.English;

        // 설정 저장
        PlayerPrefs.SetString("Language", _currentLanguage.ToString());
        PlayerPrefs.Save();
    }

    // 언어 파일 로딩
    private static void LoadLocalizedText(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonText = File.ReadAllText(filePath);
            ProcessJsonData(jsonText);
        }
        else
        {
            Debug.LogError($"Cannot find localization file at path: {filePath}");
        }
    }

    // JSON 데이터 처리
    private static void ProcessJsonData(string jsonText)
    {
        try
        {
            // Newtonsoft.Json 사용하여 파싱
            Dictionary<string, LocalizationEntry> jsonData =
                JsonConvert.DeserializeObject<Dictionary<string, LocalizationEntry>>(jsonText);

            if (jsonData == null)
            {
                Debug.LogError("Failed to parse JSON data");
                return;
            }

            // JSON 데이터를 순회하며 로컬라이제이션 데이터 구축
            foreach (KeyValuePair<string, LocalizationEntry> entry in jsonData)
                if (!string.IsNullOrEmpty(entry.Value.key))
                {
                    Dictionary<string, string> translations = new();

                    // "ko", "eng" 등의 언어 코드 추출
                    if (!string.IsNullOrEmpty(entry.Value.ko))
                        translations["ko"] = entry.Value.ko;

                    if (!string.IsNullOrEmpty(entry.Value.eng))
                        translations["eng"] = entry.Value.eng;

                    localizedTexts[entry.Value.key] = translations;
                }

            Debug.Log($"Loaded {localizedTexts.Count} localized texts");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing JSON data: {e.Message}");
        }
    }

    // WebGL 플랫폼을 위한 비동기 로딩
    private static IEnumerator LoadLocalizedTextWebGL(string filePath)
    {
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
        yield return www.SendWebRequest();

        if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            string jsonText = www.downloadHandler.text;
            ProcessJsonData(jsonText);
        }
        else
        {
            Debug.LogError($"Error loading localization file: {www.error}");
        }
    }

    // 언어 코드 얻기
    private static string GetLanguageCode(SystemLanguage language)
    {
        switch (language)
        {
            case SystemLanguage.English:
                return "eng"; // JSON에서 사용된 키가 "eng"
            case SystemLanguage.Korean:
                return "ko";
            default:
                return "eng"; // 기본값
        }
    }

    // 키를 사용하여 현재 언어로 텍스트 얻기
    public static string GetLocalizedText(string key)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("LocalizationManager not initialized. Call Initialize() first.");
            return key;
        }

        string languageCode = GetLanguageCode(_currentLanguage);

        // 키가 없는 경우 키 자체를 반환 (디버깅 용이)
        if (!localizedTexts.ContainsKey(key) || !localizedTexts[key].ContainsKey(languageCode))
        {
            Debug.LogWarning($"Localization key not found: {key} for language {languageCode}");

            // 영어로 폴백
            if (languageCode != "eng" && localizedTexts.ContainsKey(key) &&
                localizedTexts[key].ContainsKey("eng"))
                return localizedTexts[key]["eng"];

            return key;
        }

        return localizedTexts[key][languageCode];
    }

    // 포맷팅을 지원하는 버전 (예: "Hello, {0}!" -> "Hello, Player!")
    public static string GetLocalizedText(string key, params object[] args)
    {
        string value = GetLocalizedText(key);

        if (args != null && args.Length > 0)
            try
            {
                return string.Format(value, args);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"Format error for key '{key}': {ex.Message}");
                return value;
            }

        return value;
    }

    // 언어 변경 메서드
    public static void ChangeLanguage(SystemLanguage language)
    {
        if (SupportedLanguages.Contains(language))
        {
            CurrentLanguage = language;
            PlayerPrefs.SetString("Language", language.ToString());
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogWarning($"Language {language} is not supported");
        }
    }

    // 로컬라이제이션 데이터 구조
    [Serializable]
    private class LocalizationEntry
    {
        public string key;
        public string ko;
        public string eng;
    }
}

// 확장 메소드
public static class LocalizationExtensions
{
    // string 확장 메소드: 직접 키를 문자열로 변환
    public static string Localize(this string key)
    {
        return LocalizationManager.GetLocalizedText(key);
    }

    // string 확장 메소드: 매개변수를 포함하는 로컬라이제이션
    public static string Localize(this string key, params object[] args)
    {
        return LocalizationManager.GetLocalizedText(key, args);
    }
}
