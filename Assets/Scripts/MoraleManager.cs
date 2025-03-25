using System.Collections.Generic;
using UnityEngine;

public class MoraleManager : MonoBehaviour
{
    public static MoraleManager Instance { get; private set; }

    // 종족별 사기
    public float humanMorale = 0f;
    public float amorphousMorale = 0f;
    public float mechanicTankMorale = 0f;
    public float mechanicSupMorale = 0f;
    public float beastMorale = 0f;
    public float insectMorale = 0f;

    // 전체 선원 적용 사기
    public float globalMorale = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 종족별 사기 반환
    public float GetRaceMorale(CrewRace race)
    {
        return race switch
        {
            CrewRace.Human => humanMorale,
            CrewRace.Amorphous => amorphousMorale,
            CrewRace.MechanicTank => mechanicTankMorale,
            CrewRace.MechanicSup => mechanicSupMorale,
            CrewRace.Beast => beastMorale,
            CrewRace.Insect => insectMorale,
            _ => 0f
        };
    }

    // 선원의 총 사기 반환
    public float GetTotalMoraleBonus(CrewMember crew)
    {
        float raceMorale = GetRaceMorale(crew.race);
        return raceMorale + globalMorale;
    }

    // 종족별 사기 설정
    public void SetRaceMorale(CrewRace race, float value)
    {
        switch (race)
        {
            case CrewRace.Human: humanMorale = value; break;
            case CrewRace.Amorphous: amorphousMorale = value; break;
            case CrewRace.MechanicTank: mechanicTankMorale = value; break;
            case CrewRace.MechanicSup: mechanicSupMorale = value; break;
            case CrewRace.Beast: beastMorale = value; break;
            case CrewRace.Insect: insectMorale = value; break;
        }
    }

    // 전체 선원 사기 한 번에 할당
    public void SetAllCrewMorale(float value)
    {
        globalMorale = value;
    }

    // 전체 사기 초기화
    public void ResetAllMorale()
    {
        humanMorale = 0f;
        amorphousMorale = 0f;
        mechanicTankMorale = 0f;
        mechanicSupMorale = 0f;
        beastMorale = 0f;
        insectMorale = 0f;
        globalMorale = 0f;
    }

    // 전체 사기 상태 출력 (디버깅용)
    public void PrintAllMorale()
    {
        Debug.Log("=== Morale 상태 ===");
        Debug.Log($"Human: {humanMorale}");
        Debug.Log($"Amorphous: {amorphousMorale}");
        Debug.Log($"MechanicTank: {mechanicTankMorale}");
        Debug.Log($"MechanicSup: {mechanicSupMorale}");
        Debug.Log($"Beast: {beastMorale}");
        Debug.Log($"Insect: {insectMorale}");
        Debug.Log($"Global Ship Morale: {globalMorale}");
    }
}
