using TMPro;
using UnityEngine;
using System.Collections;

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

    private void Start()
    {
        ShowPlayerSummary();
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
        endingText.text = "<b>지금까지의 항해 기록</b>\n\n";

        yield return new WaitForSeconds(lineDelay);
        endingText.text += $"보유 재화: <b>{data.COMA:N0} COMA</b>\n";

        yield return new WaitForSeconds(lineDelay);
        endingText.text += $"해적 처치 수: <b>{data.pirateDefeated}</b>명\n";

        yield return new WaitForSeconds(lineDelay);
        endingText.text += $"퀘스트 완료 수: <b>{data.questCleared}</b>개\n";

        yield return new WaitForSeconds(lineDelay);
        endingText.text += $"불가사의 조우 수: <b>{data.mysteryFound}</b>회\n";
    }
}
