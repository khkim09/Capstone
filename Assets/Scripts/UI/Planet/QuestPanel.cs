using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    private PlanetData planet;
    private bool isActive;
    private Animator animator;

    private Image panel;

    private TextMeshProUGUI tmp;

    private List<RandomQuest> completableQuests=new List<RandomQuest>();
    private List<RandomQuest> availableQuests=new List<RandomQuest>();
    private List<RandomQuest> activeQuests=new List<RandomQuest>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public Sprite completablePanel;
    public Sprite availablePanel;
    public Sprite activePanel;
    void Start()
    {
        planet = GameManager.Instance.WhereIAm();
        animator = GetComponent<Animator>();
        panel = GetComponent<Image>();
        tmp = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void UpdatePanel()
    {
        //퀘스트가 있는 경우 패널이 들어온다.
        if (planet.questList.Count > 0 && !isActive)
        {
            animator.SetTrigger("In");
            isActive = true;

            //먼저 퀘스트 리스트를 다 비우고
            completableQuests.Clear();
            availableQuests.Clear();
            activeQuests.Clear();

            foreach (RandomQuest quest in planet.questList)
            {
                if (quest.GetCanComplete())
                {
                    completableQuests.Add(quest);
                }
                else if(quest.status==QuestStatus.NotStarted)
                    availableQuests.Add(quest);
                else if(quest.status==QuestStatus.Active)
                    activeQuests.Add(quest);
            }

            //완료 가능한 퀘스트가 있는 경우
            if (completableQuests.Count > 0)
            {
                tmp.text = $"{"ui.planet.quest.completable".Localize()}";
                panel.sprite = completablePanel;
            }
            //수락 가능한 퀘스트가 있는 경우
            else if (availableQuests.Count > 0)
            {
                tmp.text = $"{"ui.planet.quest.available".Localize()}";
                panel.sprite = availablePanel;
            }
            //진행 중인 퀘스트가 있는 경우
            else if (activeQuests.Count > 0)
            {
                tmp.text = $"{"ui.planet.quest.active".Localize()}";
                panel.sprite = activePanel;
            }
        }
        //퀘스트가 없는 경우
        else if (planet.questList.Count == 0 && isActive)
        {
            animator.SetTrigger("Out");
            isActive = false;
        }
    }
}
