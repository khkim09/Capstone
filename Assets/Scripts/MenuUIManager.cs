using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 모든 오브젝트 및 UI 중 가장 상위에서 관리되어 언제나 유저에게 보이는 MenuUI를 관리하는 Manager
/// </summary>
public class MenuUIManager : MonoBehaviour
{
    public RectTransform mainUI;  // MainUI 패널
    public Button menuButton;      // 메뉴 버튼
    public float slideSpeed = 0.3f; // 애니메이션 속도
    private bool isMenuOpen; // 현재 메뉴 상태

    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;
    private Coroutine slideCoroutine;

    public static MenuUIManager Instance { get; set; }

    /// <summary>
    /// 메뉴 UI의 초기 설정을 하고, 메뉴 버튼에 클릭 이벤트를 추가합니다.
    /// </summary>
    void Start()
    {
        // UI의 숨겨진 위치와 보이는 위치 설정
        hiddenPosition = new Vector2(Screen.width, mainUI.anchoredPosition.y); // 오른쪽 화면 밖
        visiblePosition = new Vector2(Screen.width - mainUI.rect.width, mainUI.anchoredPosition.y); // 화면 안으로 이동

        mainUI.anchoredPosition = hiddenPosition; // 기본적으로 숨김
        isMenuOpen = false;
        menuButton.onClick.AddListener(ToggleMenu); // 버튼 클릭 이벤트 추가
    }

    /// <summary>
    /// 열려있는 Menu UI를 강제로 닫습니다.
    /// </summary>
    public void ForceCloseMenu()
    {
        if (!isMenuOpen)
            return;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideMenu(hiddenPosition));
        isMenuOpen = false;
    }

    /// <summary>
    /// Menu UI를 토글형식으로 열었다가 닫을 수 있습니다.
    /// </summary>
    public void ToggleMenu()
    {
        StopAllCoroutines();
        StartCoroutine(SlideMenu(isMenuOpen ? hiddenPosition : visiblePosition));
        isMenuOpen = !isMenuOpen;
    }

    /// <summary>
    /// Menu UI 클릭 시 UI가 애니메이션 형태로 우에서 좌로 Slide되어 나타납니다.
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <returns></returns>
    private IEnumerator SlideMenu(Vector2 targetPosition)
    {
        float elapsedTime = 0f;
        Vector2 startPosition = mainUI.anchoredPosition;

        while (elapsedTime < slideSpeed)
        {
            mainUI.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, elapsedTime / slideSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainUI.anchoredPosition = targetPosition;
    }
}
