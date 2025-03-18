using System.Collections.Generic;
using UnityEngine;

public class CrewManager : MonoBehaviour
{
    // instance
    public static CrewManager Instance { get; set; }

    // CrewInfoWrapper
    private CrewInfoWrapper crewInfoWrapper;

    // 승무원 리스트
    [SerializeField] private List<CrewMember> crewList = new List<CrewMember>();
    [SerializeField] private GameObject alertAddCrewUI;
    [SerializeField] private GameObject mainUI;

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

        // CrewInfoLoader의 LoadedCrewInfo를 가져옵니다.
        crewInfoWrapper = CrewInfoLoader.crewInfo;
        if (crewInfoWrapper != null)
        {
            Debug.Log("CrewInfo data load 성공");
        }
        else
        {
            Debug.LogError("CrewInfo data load 실패");
        }
    }

    private void Update()
    {
        if (alertAddCrewUI.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                alertAddCrewUI.SetActive(false);
                mainUI.SetActive(true);
            }
        }
    }

    private void AlertNeedCrew()
    {
        alertAddCrewUI.SetActive(true);
        mainUI.SetActive(false);
    }

    public void AddCrewMember(string inputName, CrewRace inputRace, CrewMember newCrew)
    {

        switch (inputRace)
        {
            case CrewRace.Human:
                newCrew.maxHealth = 100.0f;
                newCrew.attack = 8.0f;
                newCrew.defense = 8.0f;

                break;
            case CrewRace.Amorphous:
                newCrew.maxHealth = 100.0f;
                newCrew.attack = 9.0f;
                newCrew.defense = 6.0f;
                break;
            case CrewRace.MechanicTank:
                newCrew.maxHealth = 120.0f;
                newCrew.attack = 5.0f;
                newCrew.defense = 5.0f;
                break;
            case CrewRace.MechanicSup:
                newCrew.maxHealth = 70.0f;
                newCrew.attack = 5.0f;
                newCrew.defense = 5.0f;
                break;
            case CrewRace.Beast:
                newCrew.maxHealth = 120.0f;
                newCrew.attack = 12.0f;
                newCrew.defense = 12.0f;
                break;
            case CrewRace.Insect:
                newCrew.maxHealth = 120.0f;
                newCrew.attack = 10.0f;
                newCrew.defense = 15.0f;
                break;
        }

        // 기본 정보 초기화
        newCrew.crewName = inputName;
        newCrew.health = newCrew.maxHealth; // 초기 현재 체력 = maxHealth
        newCrew.status = CrewStatus.Normal;
        newCrew.isAlive = true;
        newCrew.race = inputRace;

        /*
                // 기본 장비 설정 (수정 필요)
                newCrew.equipment.workAssistant = "Basic workAssistant";
                newCrew.equipment.weapon = "Basic weapon";
                newCrew.equipment.armor = "Basic armor";
        */

        // 크루 추가
        crewList.Add(newCrew);
    }


}
