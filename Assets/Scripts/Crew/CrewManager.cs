using System.Collections.Generic;
using UnityEngine;

public class CrewManager : MonoBehaviour
{
    // instance
    public static CrewManager Instance { get; set; }

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
            Instance = this;
        // TODO : 임시로 작동되게 해놓음. 최종적으론 삭제 예정
        /*
         *
         *         if (GameManager.Instance.GetPlayerShip().GetCrewCount() == 0)
            AlertNeedCrew();
         */


        else
            Destroy(gameObject);
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

        // TODO : 임시로 작동되게 해놓음. 최종적으론 삭제 예정

        GameManager.Instance.GetPlayerShip().GetSystem<CrewSystem>().AddCrewMember(newCrew);

        // 확인용 로그
        Debug.Log($"새로운 선원 : {newCrew.crewName} {newCrew.race}");
    }

    // 현재 선원 수 가져오기
    public int GetCurrentCrewCount()
    {
        // TODO : 임시로 작동되게 해놓음. 최종적으론 삭제 예정
        return GameManager.Instance.GetPlayerShip().GetCrewCount();
    }

    // 현재 수용 가능 선원 수 가져오기 (총 고용 가능 수)
    public int GetMaxCrewCount()
    {
        return GameManager.Instance.GetPlayerShip().GetMaxCrew();
    }


    public CrewBase GetSelectedCrew()
    {
        // TODO : 임시로 작동되게 해놓음. 최종적으론 삭제 예정

        return GameManager.Instance.GetPlayerShip().GetCrewCount() > 0
            ? GameManager.Instance.GetPlayerShip().GetAllCrew()[0]
            : null;
        // crewList UI (discord 참조)에서 유저가 선택하도록 수정 필요
    }
}
