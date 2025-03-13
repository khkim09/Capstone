using System;
using UnityEngine;

public enum CrewRace
{
    None = 0,
    Human = 1,
    Amorphous = 2,
    MechanicTank = 3,
    MechanicSup = 4,
    Beast = 5,
    Insect = 6
}

[Serializable]
public class CrewMember : MonoBehaviour
{
    // 기본 정보
    public string crewName; // 이름
    public float health; // 현재 체력
    public CrewStatus status; // 현재 상태 (부상 등)
    public bool isAlive = true; // 생존 여부

    // 디테일 정보
    public CrewRace race; // 종족
    public float maxHealth; // 최대 체력
    public float attack; // 공격력
    public float defense; // 방어력

    /*
        // 장비
        public string workAssistant; // 작업 보조 장비
        public string weapon; // 무기 장비
        public string armor; // 방어구 장비
    */
}
