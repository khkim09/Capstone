using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 엔딩 씬에서 플레이 정보를 화면에 출력하는 클래스.
/// </summary>
public class EndingInfoDisplay : MonoBehaviour
{
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


    private bool isFinished = false;
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
            float timeSinceLastClick = Time.time - lastClickTime;
            lastClickTime = Time.time;

            if (isFinished)
            {
                GoToNextScene();
            }
            else if (timeSinceLastClick < doubleClickThreshold)
            {
                GoToNextScene(); // 출력 중이더라도 더블클릭 시 넘어감
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
        // 원하는 방식으로 씬 전환 가능
        SceneManager.LoadScene("EndingScene2");
    }
}
