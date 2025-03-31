using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사기 관련 이벤트 생성에 따른 사기 효과 적용 관리 Manager
/// </summary>
public class MoraleManager : MonoBehaviour
{
    public static MoraleManager Instance { get; private set; }

    /// <summary>
    /// 종족별 사기
    /// </summary>
    public float humanMorale = 0f;
    public float amorphousMorale = 0f;
    public float mechanicTankMorale = 0f;
    public float mechanicSupMorale = 0f;
    public float beastMorale = 0f;
    public float insectMorale = 0f;

    /// <summary>
    /// 전체 선원 적용 사기
    /// </summary>
    public float globalMorale = 0f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 종족 별 사기 반환
    /// </summary>
    /// <param name="race"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 선원의 총 사기 반환
    /// </summary>
    /// <param name="crew"></param>
    /// <returns></returns>
    public float GetTotalMoraleBonus(CrewMember crew)
    {
        float raceMorale = GetRaceMorale(crew.race);
        return raceMorale + globalMorale;
    }

    /// <summary>
    /// 종족별 사기 설정
    /// </summary>
    /// <param name="race"></param>
    /// <param name="value"></param>
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

    /// <summary>
    /// 전체 선원 사기 한 번에 할당
    /// </summary>
    /// <param name="value"></param>
    public void SetAllCrewMorale(float value)
    {
        globalMorale = value;
    }

    /// <summary>
    /// 전체 사기 초기화
    /// </summary>
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

    /// <summary>
    /// 전체 사기 상태 출력 (디버깅용)
    /// </summary>
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
