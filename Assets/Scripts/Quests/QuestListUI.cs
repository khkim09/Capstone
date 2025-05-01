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

    private List<GameObject> spawnedSlots = new();

    /// <summary>
    /// 퀘스트 목록 창을 열고 목록을 표시합니다.
    /// </summary>
    public void Open()
    {
        Clear();
        panel.SetActive(true);

        List<QuestManager.Quest> quests = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in quests)
        {
            GameObject slot = Instantiate(questSlotPrefab, contentParent);
            slot.GetComponentInChildren<TextMeshProUGUI>().text = $"{quest.title} ({quest.status})";
            spawnedSlots.Add(slot);
        }
    }

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
    /// 기존에 생성된 슬롯들을 제거합니다.
    /// </summary>
    private void Clear()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }
}
