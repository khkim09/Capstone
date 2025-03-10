using System;
using System.Collections.Generic;

// 승무원 종족
public enum CrewRace
{
    Human, // 인간형
    NonCorporeal, // 부정형
    Machine, // 기계형
    Beast, // 짐승형
    Insect // 곤충형
}

// 장비 (보조 장비, 무기, 방어구)
[Serializable]
public class Equipment
{
    public string workAssistant; // 작업 보조 장비
    public string weapon; // 무기
    public string armor; // 방어구
}

// 승무원 field
[Serializable]
public class CrewMember
{
    public CrewRace race;

    public string name;
    public int attack;
    public int defense;
    
    // 숙련도
    public float facilityProficiency; // 시설
    public float combatProficiency; // 전투
    public float tradeProficiency; // 거래
    public float maxProficiency; // 최대 숙련도
    public float learningSpeed; // 학습 속도

    public Equipment equipment; // 장비

    // 기본 field
    public float health = 100.0f;
    public float maxHealth = 100.0f;
    public CrewStatus status = CrewStatus.Normal;
    public bool isAlive = true;
    public List<String> skills = new List<String>();
}

// crew member data
[Serializable]
public class CrewMemberData
{
    public string name;
    // public 
}