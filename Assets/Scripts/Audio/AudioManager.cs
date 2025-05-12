using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")] [SerializeField]
    private AudioMixer audioMixer;

    [SerializeField] private string bgmVolumeParameter = "BGMVolume";
    [SerializeField] private string sfxVolumeParameter = "SFXVolume";

    private void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 초기 오디오 설정 로드
            LoadVolumes();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 초기 볼륨 설정 로드 및 적용
    private void LoadVolumes()
    {
        // PlayerPrefs에서 볼륨 값 불러오기
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

        // AudioMixer에 적용
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);

        Debug.Log($"Audio Manager: 볼륨 설정 로드 - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }

    // BGM 볼륨 설정 (AudioMixer에만 적용)
    public void SetBGMVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat(bgmVolumeParameter, dB);
        }
    }

    // SFX 볼륨 설정 (AudioMixer에만 적용)
    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            audioMixer.SetFloat(sfxVolumeParameter, dB);
        }
    }

    // 볼륨 설정 저장 (PlayerPrefs에 저장)
    public void SaveVolumeSettings(float bgmVolume, float sfxVolume)
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();

        // 믹서에도 적용
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);

        Debug.Log($"Audio Manager: 볼륨 설정 저장 - BGM: {bgmVolume}, SFX: {sfxVolume}");
    }

    // 현재 BGM 볼륨 가져오기
    public float GetBGMVolume()
    {
        // PlayerPrefs에서 저장된 값 반환
        return PlayerPrefs.GetFloat("BGMVolume", 0.8f);
    }

    // 현재 SFX 볼륨 가져오기 (PlayerPrefs에서 직접 가져오기)
    public float GetSFXVolume()
    {
        // PlayerPrefs에서 저장된 값 반환
        return PlayerPrefs.GetFloat("SFXVolume", 0.8f);
    }
}
