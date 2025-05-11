using UnityEngine;
using UnityEngine.Audio;

public class AudioTester : MonoBehaviour
{
    // BGM 관련
    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    // SFX 관련
    [Header("SFX")]
    [SerializeField] private AudioClip sfxClip;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Settings")]
    [SerializeField] private KeyCode playSfxKey = KeyCode.Space;

    private void Start()
    {
        // BGM 설정
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.outputAudioMixerGroup = bgmMixerGroup;

        // SFX 설정
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        sfxSource.clip = sfxClip;
        sfxSource.loop = false;
        sfxSource.outputAudioMixerGroup = sfxMixerGroup;

        // BGM 재생 시작
        bgmSource.Play();

        Debug.Log("오디오 테스터가 초기화되었습니다. 스페이스 바를 누르면 SFX가 재생됩니다.");
    }

    private void Update()
    {
        // 지정된 키를 누르면 SFX 재생
        if (Input.GetKeyDown(playSfxKey))
        {
            PlaySFX();
        }
    }

    // SFX 재생 메서드
    public void PlaySFX()
    {
        sfxSource.PlayOneShot(sfxClip);
        Debug.Log("SFX 재생");
    }
}
