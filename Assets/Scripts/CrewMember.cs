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
    public float maxSkillValue; // 최대 숙련도
    public float learningSpeed; // 학습 속도

    // 숙련도 디테일 (수정 필요)
    public float facilitySkill; // 시설 숙련도

    // combat skill
    public float meleeSkill; // 근접
    public float rangeSkill; // 원거리
    public float shieldSkill; // 방어

    // etc skill
    public float healSkill; // 치유
    public float tradeSkill; // 거래

    /*
        // 장비
        public string workAssistant; // 작업 보조 장비
        public string weapon; // 무기 장비
        public string armor; // 방어구 장비
    */
}
