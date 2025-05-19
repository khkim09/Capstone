// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using UnityEngine.UI;
//
// /// <summary>
// /// 수락한 퀘스트들을 보여주는 UI 패널을 관리합니다.
// /// 완료 가능한 퀘스트는 리스트 상단으로 올리고, 패널 이미지를 변경합니다.
// /// </summary>
// public class QuestListUI : MonoBehaviour
// {
//     /// <summary>퀘스트 패널 루트</summary>
//     public GameObject panel;
//
//     /// <summary>퀘스트 슬롯 프리팹</summary>
//     public GameObject questSlotPrefab;
//
//     /// <summary>퀘스트 슬롯이 들어갈 부모 오브젝트</summary>
//     public Transform contentParent;
//
//     /// <summary>현재 표시 중인 퀘스트 슬롯 목록</summary>
//     private List<GameObject> spawnedSlots = new();
//
//     /// <summary>실버 패널 Sprite (기본)</summary>
//     [SerializeField] private Sprite silverPanelSprite;
//
//     /// <summary>골드 패널 Sprite (완료 가능 시)</summary>
//     [SerializeField] private Sprite goldPanelSprite;
//
//     /// <summary>
//     /// 시작 시 QuestCompleted 이벤트를 연결하고 패널을 꺼둡니다.
//     /// </summary>
//     private void Start()
//     {
//         if (panel == null)
//         {
//             Debug.LogError("QuestListUI: panel이 할당되지 않았습니다.");
//             return;
//         }
//
//         panel.SetActive(false);
//
//         if (QuestManager.Instance != null)
//             QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
//     }
//
//     /// <summary>
//     /// 이벤트 리스너 해제
//     /// </summary>
//     private void OnDestroy()
//     {
//         if (QuestManager.Instance != null)
//             QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
//     }
//
//     /// <summary>
//     /// 퀘스트 목록 창을 열고 목록을 표시합니다.
//     /// 완료 가능한 퀘스트를 먼저 추가한 뒤 진행 중 퀘스트를 추가합니다.
//     /// </summary>
//     public void OpenQuestListForPlanet(Planet currentPlanet = null)
//     {
//         Clear();
//         panel.SetActive(true);
//
//         string currentPlanetId = currentPlanet?.PlanetData?.planetName;
//
//         if (QuestManager.Instance == null)
//         {
//             Debug.LogWarning("QuestManager.Instance가 존재하지 않습니다.");
//             return;
//         }
//
//         List<RandomQuest> quests = QuestManager.Instance.GetActiveQuests();
//
//         // 완료 가능한 퀘스트 먼저
//         foreach (RandomQuest quest in quests)
//         {
//             bool isCompleteReady =
//                 !string.IsNullOrEmpty(currentPlanetId) && quest.IsQuestCompleteReady(currentPlanetId);
//             if (isCompleteReady)
//                 AddToQuestListPanel(quest, true);
//         }
//
//         // 그 외 퀘스트
//         foreach (RandomQuest quest in quests)
//         {
//             bool isCompleteReady =
//                 !string.IsNullOrEmpty(currentPlanetId) && quest.IsQuestCompleteReady(currentPlanetId);
//             if (!isCompleteReady)
//                 AddToQuestListPanel(quest, false);
//         }
//     }
//
//
//     /// <summary>
//     /// 외부 버튼에서 호출될 때 사용되는 열기/닫기 함수입니다.
//     /// </summary>
//     public void ToggleFromButton()
//     {
//         if (panel == null) return;
//
//         if (panel.activeSelf)
//         {
//             Close();
//         }
//         else
//         {
//             Planet planet = QuestUIManager.Instance?.GetCurrentPlanet();
//             OpenQuestListForPlanet(planet);
//         }
//     }
//
//     /// <summary>
//     /// 퀘스트 목록 창을 닫고 기존 슬롯들을 제거합니다.
//     /// </summary>
//     public void Close()
//     {
//         if (panel != null)
//             panel.SetActive(false);
//
//         Clear();
//     }
//
//     /// <summary>
//     /// 기존에 생성된 모든 퀘스트 슬롯들을 제거합니다.
//     /// </summary>
//     private void Clear()
//     {
//         foreach (GameObject slot in spawnedSlots)
//             Destroy(slot);
//         spawnedSlots.Clear();
//     }
//
//     /// <summary>
//     /// 퀘스트 완료 시 호출되어 리스트를 자동 갱신합니다.
//     /// </summary>
//     private void OnQuestCompleted(RandomQuest quest)
//     {
//         if (!IsOpen()) return;
//
//         Planet planet = QuestUIManager.Instance?.GetCurrentPlanet();
//         OpenQuestListForPlanet(planet);
//     }
//
//     /// <summary>
//     /// 현재 패널이 열려 있는지 여부를 반환합니다.
//     /// </summary>
//     public bool IsOpen()
//     {
//         return panel != null && panel.activeSelf;
//     }
//
//     /// <summary>
//     /// 퀘스트 슬롯을 생성하여 리스트에 추가합니다.
//     /// 완료 가능한 경우 Sprite를 골드로 변경합니다.
//     /// </summary>
//     private void AddToQuestListPanel(RandomQuest quest, bool isCompleteReady = false)
//     {
//         if (quest == null || contentParent == null || questSlotPrefab == null)
//         {
//             Debug.LogWarning("AddToQuestListPanel: 필요한 오브젝트가 null입니다.");
//             return;
//         }
//
//         GameObject slot = Instantiate(questSlotPrefab, contentParent);
//         if (slot == null) return;
//
//         slot.SetActive(true);
//
//         // 캐싱을 통한 최적화
//         Transform slotTransform = slot.transform;
//         TextMeshProUGUI titleText = slotTransform.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
//         TextMeshProUGUI descText = slotTransform.Find("QuestDescription")?.GetComponent<TextMeshProUGUI>();
//         TextMeshProUGUI statusText = slotTransform.Find("QuestStatus")?.GetComponent<TextMeshProUGUI>();
//         TextMeshProUGUI deadlineText = slotTransform.Find("QuestDeadline")?.GetComponent<TextMeshProUGUI>();
//         TextMeshProUGUI rewardText = slotTransform.Find("QuestReward")?.GetComponent<TextMeshProUGUI>();
//         TextMeshProUGUI planetText = slotTransform.Find("QuestPlanet")?.GetComponent<TextMeshProUGUI>();
//
//         if (titleText != null)
//             titleText.text = quest.title;
//
//         if (descText != null)
//             descText.text = quest.description;
//
//         if (statusText != null)
//             statusText.text = quest.status.ToString();
//
//         if (deadlineText != null && GameManager.Instance != null)
//         {
//             int currentYear = GameManager.Instance.CurrentYear;
//             int yearsPassed = currentYear - quest.questAcceptedYear;
//             int yearsLeft = Mathf.Max(0, 20 - yearsPassed);
//             deadlineText.text = $"남은연도: {yearsLeft}";
//         }
//
//         if (rewardText != null && quest.rewards != null && quest.rewards.Count > 0)
//         {
//             QuestReward reward = quest.rewards[0];
//             rewardText.text = $"{reward.amount} {reward.questRewardType}";
//         }
//
//         if (planetText != null)
//         {
//             // objective 중 destinationPlanetId를 가진 첫 목표 가져오기
//             string planetId = "-";
//             foreach (QuestObjective obj in quest.objectives)
//             {
//             }
//
//             planetText.text = planetId;
//         }
//
//         // 패널 이미지 변경 (Silver / Gold)
//         Image background = slot.GetComponent<Image>();
//         if (background != null)
//             background.sprite = isCompleteReady ? goldPanelSprite : silverPanelSprite;
//
//         // 슬롯 클릭 시 완료 UI 호출
//         Button btn = slot.GetComponent<Button>();
//         if (btn != null && isCompleteReady)
//             btn.onClick.AddListener(() =>
//             {
//                 QuestUIManager ui = QuestUIManager.Instance;
//                 if (ui != null)
//                     ui.ShowCompletion(quest);
//             });
//
//         // 취소 버튼 연결
//         Button failButton = slotTransform.Find("FailButton")?.GetComponent<Button>();
//         if (failButton != null)
//             failButton.onClick.AddListener(() =>
//             {
//                 if (QuestManager.Instance == null) return;
//
//                 QuestManager.Instance.RequestFailQuest(quest);
//                 QuestUIManager ui = QuestUIManager.Instance;
//                 if (ui != null)
//                 {
//                     Planet planet = ui.GetCurrentPlanet();
//                     if (planet != null)
//                         OpenQuestListForPlanet(planet);
//                 }
//             });
//
//         spawnedSlots.Add(slot);
//     }
// }


