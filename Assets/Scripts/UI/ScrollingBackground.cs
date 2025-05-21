using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollingBackground : MonoBehaviour
{
    /// <summary>
    /// 흐르는 속도
    /// </summary>
    [SerializeField] private float scrollSpeed = 0.5f;

    /// <summary>
    /// 평행 방향으로 흐르는지 여부
    /// </summary>
    [SerializeField] private bool scrollHorizontal = true;

    /// <summary>
    /// 수직 방향으로 흐르는지 여부
    /// </summary>
    [SerializeField] private bool scrollVertical = false;

    /// <summary>
    /// 흐르는 방향 전환
    /// </summary>
    [SerializeField] private bool reverseDirection = false;

    /// <summary>
    /// 흐르는 방향 랜덤 여부
    /// </summary>
    [SerializeField] private bool randomDirection = false;

    /// <summary>
    /// 배경 이미지 랜덤으로 정할지 여부
    /// </summary>
    [SerializeField] private bool randomImage = false;

    /// <summary>
    /// 타일링 정도
    /// </summary>
    [SerializeField] private Vector2 tiling = new(0.5f, 1);

    [Header("배경 이미지들")]
    /// <summary>
    /// 메인 배경 이미지
    /// </summary>
    [SerializeField]
    private RawImage mainRawImage;

    /// <summary>
    /// 서브 배경 이미지 (전환용)
    /// </summary>
    [SerializeField] private RawImage subRawImage;

    /// <summary>
    /// 랜덤일 경우 사용할 이미지 목록
    /// </summary>
    [SerializeField] private Texture2D[] backgroundTextures;

    [Header("전환 효과 설정")]
    /// <summary>
    /// 배경 전환 시간
    /// </summary>
    [SerializeField]
    private readonly float transitionDuration = Constants.WarpNodes.WarpingDuration;

    /// <summary>
    /// 전환 중 최대 스크롤 속도
    /// </summary>
    [SerializeField] private float maxScrollSpeed = 10.0f;

    [SerializeField] private GameObject noTouchPanel;

    /// <summary>
    /// 현재 활성화된 배경 (true: main, false: sub)
    /// </summary>
    private bool isMainActive = true;

    /// <summary>
    /// 원본 스크롤 속도
    /// </summary>
    private float originalScrollSpeed;

    /// <summary>
    /// 현재 전환 중인지 여부
    /// </summary>
    private bool isTransitioning = false;

    /// <summary>
    /// 현재 활성화된 RawImage
    /// </summary>
    private RawImage CurrentRawImage => isMainActive ? mainRawImage : subRawImage;

    /// <summary>
    /// 현재 비활성화된 RawImage
    /// </summary>
    private RawImage InactiveRawImage => isMainActive ? subRawImage : mainRawImage;

    private void Awake()
    {
        originalScrollSpeed = scrollSpeed;

        // 서브 이미지는 처음에 비활성화
        if (subRawImage != null) subRawImage.gameObject.SetActive(false);

        if (randomImage) SetRandomBackgroundTexture(CurrentRawImage);

        if (randomDirection) SetRandomDirection();

        // 텍스처 타일링 설정
        SetupTextureSettings(mainRawImage);
        SetupTextureSettings(subRawImage);
    }

    private void Update()
    {
        if (!isTransitioning)
        {
            UpdateScrolling(CurrentRawImage);
        }
        else
        {
            // 전환 중에는 두 이미지 모두 스크롤
            UpdateScrolling(mainRawImage);
            UpdateScrolling(subRawImage);
        }
    }

    private void UpdateScrolling(RawImage rawImage)
    {
        if (rawImage == null || !rawImage.gameObject.activeInHierarchy) return;

        Rect uvRect = rawImage.uvRect;
        Vector2 uvPosition = uvRect.position;

        float directionH = reverseDirection ? -1 : 1;
        float directionV = reverseDirection ? 1 : -1;

        if (scrollHorizontal) uvPosition.x += directionH * scrollSpeed * Time.deltaTime;
        if (scrollVertical) uvPosition.y += directionV * scrollSpeed * Time.deltaTime;

        rawImage.uvRect = new Rect(uvPosition, uvRect.size);
    }

    private void SetRandomDirection()
    {
        int directionIndex = Random.Range(0, 8);

        switch (directionIndex)
        {
            case 0: // ➡️ 오른쪽
                scrollHorizontal = true;
                scrollVertical = false;
                reverseDirection = false;
                break;
            case 1: // ⬅️ 왼쪽
                scrollHorizontal = true;
                scrollVertical = false;
                reverseDirection = true;
                break;
            case 2: // ⬇️ 아래
                scrollHorizontal = false;
                scrollVertical = true;
                reverseDirection = false;
                break;
            case 3: // ⬆️ 위
                scrollHorizontal = false;
                scrollVertical = true;
                reverseDirection = true;
                break;
            case 4: // ↘️ 오른아래
                scrollHorizontal = true;
                scrollVertical = true;
                reverseDirection = false;
                break;
            case 5: // ↙️ 왼쪽아래
                scrollHorizontal = true;
                scrollVertical = true;
                reverseDirection = true;
                break;
            case 6: // ↖️ 왼쪽위
                scrollHorizontal = true;
                scrollVertical = true;
                reverseDirection = true;
                break;
            case 7: // ↗️ 오른쪽위
                scrollHorizontal = true;
                scrollVertical = true;
                reverseDirection = false;
                break;
        }
    }

    private void SetupTextureSettings(RawImage rawImage)
    {
        if (rawImage != null && rawImage.texture != null)
        {
            rawImage.texture.wrapMode = TextureWrapMode.Repeat;
            rawImage.uvRect = new Rect(0, 0, tiling.x, tiling.y);
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged += OnYearChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged -= OnYearChanged;
    }

    private void OnYearChanged(int year)
    {
        if (!isTransitioning)
        {
            StartCoroutine(ShowNoTouchPanelForSeconds(transitionDuration));
            StartCoroutine(TransitionToNewBackground());
        }
    }

    /// <summary>
    /// 지정된 RawImage에 랜덤 배경 텍스처 설정
    /// </summary>
    /// <param name="targetImage">텍스처를 설정할 RawImage</param>
    /// <returns>성공 여부</returns>
    private bool SetRandomBackgroundTexture(RawImage targetImage)
    {
        if (backgroundTextures == null || backgroundTextures.Length == 0)
        {
            Debug.LogWarning("배경 텍스처 배열이 비어있습니다!");
            return false;
        }

        Texture2D randomTexture = backgroundTextures[Random.Range(0, backgroundTextures.Length)];

        if (randomTexture != null)
        {
            targetImage.texture = randomTexture;
            SetupTextureSettings(targetImage);
            return true;
        }
        else
        {
            Debug.LogWarning("선택된 배경 텍스처가 null입니다!");
            return false;
        }
    }

    /// <summary>
    /// 새로운 배경으로 전환하는 코루틴
    /// </summary>
    private IEnumerator TransitionToNewBackground()
    {
        isTransitioning = true;

        // 1. 새로운 배경을 비활성화된 RawImage에 설정
        RawImage newBackgroundImage = InactiveRawImage;
        RawImage oldBackgroundImage = CurrentRawImage;

        // 새로운 랜덤 배경 로드
        if (!SetRandomBackgroundTexture(newBackgroundImage))
        {
            isTransitioning = false;
            yield break;
        }

        // 2. 새로운 배경을 활성화하고 현재 배경과 동일한 상태로 완전 동기화
        newBackgroundImage.gameObject.SetActive(true);

        // 텍스처 설정 먼저 적용
        SetupTextureSettings(newBackgroundImage);

        // 현재 배경의 모든 상태를 새 배경에 완전 복사
        newBackgroundImage.uvRect = oldBackgroundImage.uvRect;

        // 알파값만 0으로 설정 (나머지는 모두 동일하게)
        Color newColor = newBackgroundImage.color;
        newColor.a = 0f;
        newBackgroundImage.color = newColor;

        // 새로운 스크롤 방향 랜덤 설정 (UV 복사 후에 설정)

        // 새로운 스크롤 방향 랜덤 설정 (UV 복사 후에 설정)
        SetRandomDirection();

        // 3. 전환 효과 시작
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;

            // 알파값 그라데이션
            Color oldColor = oldBackgroundImage.color;
            oldColor.a = Mathf.Lerp(1f, 0f, progress);
            oldBackgroundImage.color = oldColor;

            newColor = newBackgroundImage.color;
            newColor.a = Mathf.Lerp(0f, 1f, progress);
            newBackgroundImage.color = newColor;

            // 스크롤 속도 변화 (0 -> 최대 -> 원본)
            float speedProgress = progress * 2f; // 0 ~ 2 범위로 확장
            if (speedProgress <= 1f)
                // 0 ~ 1: 원본속도 -> 최대속도
                scrollSpeed = Mathf.Lerp(originalScrollSpeed, maxScrollSpeed, speedProgress);
            else
                // 1 ~ 2: 최대속도 -> 원본속도
                scrollSpeed = Mathf.Lerp(maxScrollSpeed, originalScrollSpeed, speedProgress - 1f);

            yield return null;
        }

        // 4. 전환 완료 처리
        // 이전 배경 비활성화
        oldBackgroundImage.gameObject.SetActive(false);

        // 새로운 배경을 완전히 불투명하게
        newColor = newBackgroundImage.color;
        newColor.a = 1f;
        newBackgroundImage.color = newColor;

        // 이전 배경의 알파값 리셋 (다음 전환을 위해)
        Color resetColor = oldBackgroundImage.color;
        resetColor.a = 1f;
        oldBackgroundImage.color = resetColor;

        // 활성 상태 전환
        isMainActive = !isMainActive;

        // 스크롤 속도 원복
        scrollSpeed = originalScrollSpeed;

        isTransitioning = false;
    }

    private IEnumerator ShowNoTouchPanelForSeconds(float seconds)
    {
        Debug.Log("패널 활성화 시도");
        noTouchPanel.SetActive(true);
        yield return new WaitForSeconds(seconds);
        noTouchPanel.SetActive(false);
    }

    /// <summary>
    /// 수동으로 배경 전환 테스트용 (디버그)
    /// </summary>
    [ContextMenu("Test Background Transition")]
    public void TestBackgroundTransition()
    {
        OnYearChanged(0);
    }
}
