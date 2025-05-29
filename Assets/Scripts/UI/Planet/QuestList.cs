using System.Collections.Generic;
using UnityEngine;

public class QuestList : MonoBehaviour
{
    /// <summary>
    /// 퀘스트 아이템 prefab
    /// </summary>
    public GameObject questListItem;

    /// <summary>
    /// 행성 정보
    /// </summary>
    private PlanetData planetData;

    /// <summary>
    /// 퀘스트 아이템 루트 content
    /// </summary>
    public GameObject content;

    private List<RandomQuest> quests;

    public Sprite availableSprite;
    public Sprite activeSprite;
    public Sprite completableSprite;

    private void Start()
    {
        planetData = GameManager.Instance.WhereIAm();
        quests = planetData.questList;
        foreach (RandomQuest quest in quests)
        {
            AddQuestToListUI(quest);
        }
    }

    private void AddQuestToListUI(RandomQuest quest)
    {
        QuestListItem questItem = Instantiate(questListItem, content.transform, false).GetComponent<QuestListItem>();
        questItem.questInfo = quest;
        questItem.questList = this;

        questItem.UpdateItem();
    }

    public void UpdateList()
    {
        foreach (RandomQuest quest in quests)
        {
            AddQuestToListUI(quest);
        }
    }

    private void SetQuestStatus(RandomQuest quest, QuestStatus status)
    {
        quest.status = status;
        UpdateList();
    }


    public void CloseQuestList()
    {
        gameObject.SetActive(false);
    }

    public void OpenQuestList()
    {
        gameObject.SetActive(true);
    }
}
