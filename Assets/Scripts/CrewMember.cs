using System;
using UnityEngine;

// 종족
public enum CrewRace
{
    Human, // 인간형
    Amorphous, // 무정형 (신체 X)
    MechanicTank, // 기계형 (돌격)
    MechanicSup, // 기계형 (지원)
    Beast, // 짐승형
    Insect // 곤충형
}

// 장비 (보조 장비, 무기, 방어구)
[Serializable]
public class Equipment
{
    public string workAssistant; // 작업 보조 장비
    public string weapon; // 무기 장비
    public string armor; // 방어구 장비
}

[Serializable]
public class CrewMember
{
    // 기본 정보
    public string name; // 이름
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

    // 숙련도 디테일 (0 ~ maxSkillValue)
    public float facilitySkill; // 시설 숙련도
    public float combatSkill; // 전투 숙련도
    public float tradeSkill; // 거래 숙련도

    // 장비
    public Equipment equipment; // 착용 장비
    public string weapon; // 무기
    public string armor; // 방어구
}