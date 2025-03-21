using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuUIManager : MonoBehaviour
{
    public RectTransform mainUI;  // MainUI 패널
    public Button menuButton;      // 메뉴 버튼
    public float slideSpeed = 0.5f; // 애니메이션 속도
    private bool isMenuOpen = false; // 현재 메뉴 상태

    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;

    void Start()
    {
        // UI의 숨겨진 위치와 보이는 위치 설정
        hiddenPosition = new Vector2(Screen.width, mainUI.anchoredPosition.y); // 오른쪽 화면 밖
        visiblePosition = new Vector2(Screen.width - mainUI.rect.width, mainUI.anchoredPosition.y); // 화면 안으로 이동

        mainUI.anchoredPosition = hiddenPosition; // 기본적으로 숨김
        menuButton.onClick.AddListener(ToggleMenu); // 버튼 클릭 이벤트 추가
    }

    public void ToggleMenu()
    {
        StopAllCoroutines();
        StartCoroutine(SlideMenu(isMenuOpen ? hiddenPosition : visiblePosition));
        isMenuOpen = !isMenuOpen;
    }

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
