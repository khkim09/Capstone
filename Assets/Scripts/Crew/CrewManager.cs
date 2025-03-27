using System.Collections.Generic;
using UnityEngine;

public class CrewManager : MonoBehaviour
{
    // instance
    public static CrewManager Instance { get; set; }

    // 승무원 리스트
    [Header("Crew List")] public int currentCrewCount = 0; // 현재 선원 수
    public int maxCrewCount = 1; // 총 고용 가능 선원 수
    public List<CrewBase> crewList = new();

    [Header("Crew UI")] [SerializeField] private GameObject alertAddCrewUI;
    [SerializeField] private GameObject mainUI;

    [Header("Crew Prefabs")] [SerializeField]
    private GameObject[] crewPrefabs;

    [Header("Crew Race Settings")] [SerializeField]
    private CrewRaceStat[] raceSettings;

    [Header("Default Equipments")] public EquipmentItem defaultWeapon;
    public EquipmentItem defaultShield;
    public EquipmentItem defaultAssistant;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (crewList.Count == 0)
                AlertNeedCrew();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (alertAddCrewUI.activeInHierarchy)
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                alertAddCrewUI.SetActive(false);
                mainUI.SetActive(true);
            }
    }

    private void AlertNeedCrew()
    {
        alertAddCrewUI.SetActive(true);
        mainUI.SetActive(false);
    }


    public void AddCrewMember(string inputName, CrewRace selectedRace)
    {
        Vector3 startPos = new(-8.0f, 0.0f, 0.0f);

        // 새 crew 생성
        CrewMember newCrew = Instantiate(crewPrefabs[(int)selectedRace - 1], startPos, Quaternion.identity, null)
            .GetComponent<CrewMember>();
        newCrew.name = $"Name: {inputName}, Race: {selectedRace}";
        newCrew.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

        // 기본 정보 초기화
        newCrew.crewName = inputName;
        newCrew.isPlayerControlled = true;
        newCrew.race = selectedRace;

        // 크루 추가
        crewList.Add(newCrew);
        RefreshCrewList(crewList.Count, maxCrewCount);

        // 확인용 로그
        Debug.Log($"새로운 선원 : {newCrew.crewName} {newCrew.race}");
    }

    // 현재 선원 수 가져오기
    public int GetCurrentCrewCount()
    {
        return currentCrewCount;
    }

    // 현재 수용 가능 선원 수 가져오기 (총 고용 가능 수)
    public int GetMaxCrewCount()
    {
        return maxCrewCount;
    }

    // 선원 수 갱신
    public void RefreshCrewList(int currentCnt, int maxCnt)
    {
        currentCrewCount = currentCnt;
        maxCrewCount = maxCnt;
    }

    public CrewBase GetSelectedCrew()
    {
        // crewList UI (discord 참조)에서 유저가 선택하도록 수정 필요
        return crewList.Count > 0 ? crewList[0] : null; // 예시: 첫 번째 선원
    }
}
