using TMPro;
using UnityEngine;
using System.Collections;

/// <summary>
/// 엔딩 씬에서 플레이 정보를 화면에 출력하는 클래스.
/// </summary>
public class EndingInfoDisplay : MonoBehaviour
{
    /// <summary>
    /// 1번 캔버스
    /// </summary>
    [SerializeField] private GameObject firstEndingCanvas;

    /// <summary>
    /// 2번 캔버스
    /// </summary>
    [SerializeField] private GameObject secondEndingCanvas;

    /// <summary>
    /// 출력할 TextMeshProUGUI 컴포넌트
    /// </summary>
    [SerializeField] private TextMeshProUGUI endingText;

    /// <summary>
    /// 한 줄씩 출력되는 간격 (초)
    /// </summary>
    [SerializeField] private float lineDelay = 0.5f;

    /// <summary>
    /// 글자당 출력 속도 (초)
    /// </summary>
    [SerializeField] private float charDelay = 0.05f;

    /// <summary>
    /// 노 터치 패널
    /// </summary>
    [SerializeField] private GameObject noTouchPanel;

    /// <summary>
    /// 다음 캔버스 관련 코드
    /// </summary>
    [SerializeField] private EndingResultDisplay endingResultDisplay;

    private bool isFinished = false;
    private bool hasSceneSwitched = false;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f;

    private void Start()
    {
        StartCoroutine(ShowPlayerSummary());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (hasSceneSwitched)
                return;

            float timeSinceLastClick = Time.time - lastClickTime;
            lastClickTime = Time.time;

            if (isFinished || timeSinceLastClick < doubleClickThreshold)
            {
                GoToNextScene();
                hasSceneSwitched = true;
            }
        }
    }

    /// <summary>
    /// GameManager로부터 플레이어 데이터를 받아와 UI에 출력
    /// </summary>
    private IEnumerator ShowPlayerSummary()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null)
        {
            endingText.text = "Can't load play data";
            yield break;
        }

        var data = GameManager.Instance.playerData;
        endingText.text = "";

        yield return StartCoroutine(TypeLine("지금까지의 항해 기록\n\n"));
        yield return new WaitForSeconds(lineDelay);

        yield return StartCoroutine(TypeLine($"보유 재화: {data.COMA:N0} COMA\n"));
        yield return new WaitForSeconds(lineDelay);

        yield return StartCoroutine(TypeLine($"해적 처치 수: {data.pirateDefeated}명\n"));
        yield return new WaitForSeconds(lineDelay);

        yield return StartCoroutine(TypeLine($"퀘스트 완료 수: {data.questCleared}개\n"));
        yield return new WaitForSeconds(lineDelay);

        yield return StartCoroutine(TypeLine($"불가사의 조우 수: {data.mysteryFound}회\n\n"));
        yield return new WaitForSeconds(lineDelay);

        yield return StartCoroutine(TypeLine("성공적인 항해의 마무리를 축하드립니다.\n"));
        yield return new WaitForSeconds(lineDelay);

        yield return StartCoroutine(TypeLine("화면을 클릭하시면 엔딩을 확인하실 수 있습니다.\n"));
        yield return new WaitForSeconds(1f);

        if (noTouchPanel != null)
            noTouchPanel.SetActive(false);

        isFinished = true;
    }

    /// <summary>
    /// 한 줄의 텍스트를 한 글자씩 출력하는 코루틴
    /// </summary>
    private IEnumerator TypeLine(string line)
    {
        foreach (char c in line)
        {
            endingText.text += c;
            yield return new WaitForSeconds(charDelay);
        }
    }

    private void GoToNextScene()
    {
        SwitchCanvas();
    }
    /// <summary>
    /// 현재 캔버스를 끄고 다음 캔버스를 켭니다.
    /// </summary>
    private void SwitchCanvas()
    {
        if (firstEndingCanvas != null)
            firstEndingCanvas.SetActive(false);

        if (noTouchPanel != null)
            noTouchPanel.SetActive(true);

        if (secondEndingCanvas != null)
            secondEndingCanvas.SetActive(true);

        if (endingResultDisplay != null)
            endingResultDisplay.ShowEndingResult();
    }
}
