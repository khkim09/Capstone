using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 수락한 퀘스트들을 보여주는 UI 패널을 관리합니다.
/// </summary>
public class QuestListUI : MonoBehaviour
{
    /// <summary>퀘스트 패널 루트</summary>
    public GameObject panel;

    /// <summary>퀘스트 슬롯 프리팹</summary>
    public GameObject questSlotPrefab;

    /// <summary>퀘스트 슬롯이 들어갈 부모 오브젝트</summary>
    public Transform contentParent;

    /// <summary>현재 표시 중인 퀘스트 슬롯 목록</summary>
    private List<GameObject> spawnedSlots = new();

    /// <summary>
    /// 시작할 때 실행되는 함수입니다.
    /// 처음에는 패널을 꺼둡니다.
    /// </summary>
    private void Start()
    {
        panel.SetActive(false);  // 처음에 꺼두기

        /// <summary>퀘스트 완료 시 자동으로 리스트를 새로고침합니다.</summary>
        QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
    }

    /// <summary>
    /// 퀘스트 목록 창을 열고 목록을 표시합니다.
    /// 이미 생성된 슬롯은 제거하고, 현재 수락된 퀘스트들을 표시합니다.
    /// </summary>
    public void Open()
    {
        Clear();
        panel.SetActive(true);

        List<QuestManager.Quest> quests = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in quests)
        {
            GameObject slot = Instantiate(questSlotPrefab, contentParent);

            TextMeshProUGUI titleText = slot.transform.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = slot.transform.Find("QuestDescription")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI statusText = slot.transform.Find("QuestStatus")?.GetComponent<TextMeshProUGUI>();

            if (titleText != null)
                titleText.text = quest.title;

            if (descText != null)
                descText.text = quest.description;

            if (statusText != null)
                statusText.text = quest.status.ToString();

            spawnedSlots.Add(slot);
        }
    }

    /// <summary>
    /// 외부 버튼에서 호출될 때 사용되는 열기 함수입니다.
    /// 이미 열려 있으면 아무 동작도 하지 않습니다.
    /// </summary>
    public void OpenFromButton()
    {
        if (!panel.activeSelf)
        {
            Open();
        }
    }

    /// <summary>
    /// 창을 닫고 기존 슬롯들을 제거합니다.
    /// </summary>
    public void Close()
    {
        panel.SetActive(false);
        Clear();
    }

    /// <summary>
    /// 기존에 생성된 퀘스트 슬롯들을 모두 제거합니다.
    /// </summary>
    private void Clear()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }

    /// <summary>
    /// 퀘스트 완료 시 호출되어 리스트를 자동 갱신합니다.
    /// </summary>
    private void OnQuestCompleted(QuestManager.Quest quest)
    {
        if (panel.activeSelf)
        {
            Open(); // 다시 그려주기
        }
    }
}
