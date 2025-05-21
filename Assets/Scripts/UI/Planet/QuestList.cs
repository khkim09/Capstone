using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestList : MonoBehaviour
{
    public GameObject questListItem;
    private PlanetData planetData;
    public GameObject content;

    private List<RandomQuest> quests;

    public Sprite availableSprite;
    public Sprite activeSprite;
    public Sprite completableSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        QuestListItem questItem = Instantiate(questListItem, content.transform,false).GetComponent<QuestListItem>();
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
