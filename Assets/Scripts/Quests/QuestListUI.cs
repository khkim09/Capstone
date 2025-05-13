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
        panel.SetActive(false);
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

        List<RandomQuest> quests = QuestManager.Instance.GetActiveQuests();

        foreach (RandomQuest quest in quests)
        {
            GameObject slot = Instantiate(questSlotPrefab, contentParent);
            slot.SetActive(true);

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
    /// 외부 버튼에서 호출될 때 사용되는 열기/닫기 함수입니다.
    /// 단순히 SetActive만 제어합니다.
    /// </summary>
    public void ToggleFromButton()
    {
        if (panel.activeSelf)
            Close();
        else
            Open();
    }

    /// <summary>
    /// 퀘스트 목록 창을 닫고 기존 슬롯들을 제거합니다.
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
        foreach (GameObject slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();
    }

    /// <summary>
    /// 퀘스트 완료 시 호출되어 리스트를 자동 갱신합니다.
    /// </summary>
    private void OnQuestCompleted(RandomQuest quest)
    {
        if (IsOpen())
            Open();
    }

    /// <summary>
    /// 현재 패널이 열려 있는지 여부를 반환합니다.
    /// </summary>
    public bool IsOpen()
    {
        return panel.activeSelf;
    }
}
