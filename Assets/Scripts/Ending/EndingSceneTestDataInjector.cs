using UnityEngine;
using System.Collections;

/// <summary>
/// EndingScene 단독 테스트 시 필요한 데이터를 주입하는 클래스.
/// GameManager, ResourceManager는 그대로 두고 별도 주입 처리.
/// </summary>
public class EndingSceneTestDataInjector : MonoBehaviour
{
    [Header("테스트용 데이터")]
    [SerializeField] private int pirateDefeated = 420;
    [SerializeField] private int questCleared = 180;
    [SerializeField] private int mysteryFound = 3;
    [SerializeField] private int comaAmount = 520000;

    private void Start()
    {
        // 오직 EndingScene에서만 동작
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "EndingScene") return;

        Debug.Log("[테스트] EndingSceneTestDataInjector 실행됨");

        StartCoroutine(InjectDataWhenReady());
    }

    private IEnumerator InjectDataWhenReady()
    {
        // GameManager, ResourceManager 초기화 대기
        yield return new WaitUntil(() =>
            GameManager.Instance != null &&
            ResourceManager.Instance != null);

        // ▶ 플레이어 데이터 강제 설정
        GameManager.Instance.playerData = new PlayerData
        {
            pirateDefeated = pirateDefeated,
            questCleared = questCleared,
            mysteryFound = mysteryFound
        };
        Debug.Log("[테스트] GameManager.playerData 주입 완료");

        // ▶ COMA 강제 설정
        ResourceManager.Instance.SetResource(ResourceType.COMA, comaAmount);
        Debug.Log($"[테스트] ResourceManager.COMA = {comaAmount}");
    }
}
