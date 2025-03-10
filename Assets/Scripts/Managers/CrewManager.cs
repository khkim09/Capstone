using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class CrewManager : MonoBehaviour
{
    // 승무원 상태 변경 이벤트
    public delegate void CrewChangedHandler(int crewIndex, CrewMember crewMember);

    [SerializeField] private List<CrewMember> crew = new List<CrewMember>();
    public static CrewManager Instance { get; private set; }
    public event CrewChangedHandler OnCrewChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 초기 승무원 설정
            if (crew.Count == 0)
                InitializeDefaultCrew();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDefaultCrew()
    {
        /*
        // 기본 승무원 생성 예시
        AddCrewMember("Captain", new List<string> { "Leadership", "Piloting" });
        AddCrewMember("Engineer", new List<string> { "Repairs", "Technical" });
        AddCrewMember("Doctor", new List<string> { "Medical", "Science" });
        */
    }

    public void AddCrewMember(string inputName, CrewRace inputRace, CrewMember newCrew)
    {
        // CrewMember newCrew = new CrewMember();

        switch (inputRace)
        {
            case CrewRace.Human:
                newCrew.maxHealth = 100.0f;
                newCrew.attack = 8.0f;
                newCrew.defense = 8.0f;
                newCrew.maxSkillValue = 100.0f;
                newCrew.learningSpeed = 1.5f;
                break;
            case CrewRace.Amorphous:
                newCrew.maxHealth = 100.0f;
                newCrew.attack = 9.0f;
                newCrew.defense = 6.0f;
                newCrew.maxSkillValue = 120.0f;
                newCrew.learningSpeed = 0.8f;
                break;
            case CrewRace.MechanicTank:
                newCrew.maxHealth = 120.0f;
                newCrew.attack = 5.0f;
                newCrew.defense = 5.0f;
                newCrew.maxSkillValue = 100.0f;
                newCrew.learningSpeed = 0.0f;
                break;
            case CrewRace.MechanicSup:
                newCrew.maxHealth = 70.0f;
                newCrew.attack = 5.0f;
                newCrew.defense = 5.0f;
                newCrew.maxSkillValue = 100.0f;
                newCrew.learningSpeed = 0.0f;
                break;
            case CrewRace.Beast:
                newCrew.maxHealth = 120.0f;
                newCrew.attack = 12.0f;
                newCrew.defense = 12.0f;
                newCrew.maxSkillValue = 100.0f;
                newCrew.learningSpeed = 0.8f;
                // 짐승형 제작 시 전투 능력 만 learningSpeed = 1.5f
                break;
            case CrewRace.Insect:
                newCrew.maxHealth = 120.0f;
                newCrew.attack = 10.0f;
                newCrew.defense = 15.0f;
                newCrew.maxSkillValue = 120.0f;
                newCrew.learningSpeed = 1.0f;
                break;
        }

        // 기본 정보 초기화
        newCrew.crewName = inputName;
        newCrew.health = newCrew.maxHealth; // 초기 현재 체력 = maxHealth
        newCrew.race = inputRace;
        newCrew.isAlive = true;
        newCrew.status = CrewStatus.Normal;

        // 숙련도 초기화
        newCrew.facilitySkill = 0.0f;
        newCrew.combatSkill = 0.0f;
        newCrew.tradeSkill = 0.0f;

/*
        // 기본 장비 설정 (수정 필요)
        newCrew.equipment.workAssistant = "Basic workAssistant";
        newCrew.equipment.weapon = "Basic weapon";
        newCrew.equipment.armor = "Basic armor";
*/
        // 크루 추가
        crew.Add(newCrew);
        OnCrewChanged?.Invoke(crew.Count - 1, newCrew);
    }

    // 특정 승무원에게 효과 적용
    public void ApplyCrewEffect(CrewEffect effect)
    {
        var targetIndex = effect.targetCrewIndex;

        // 랜덤 대상 선택
        if (targetIndex == -1)
        {
            var aliveIndices = new List<int>();
            for (var i = 0; i < crew.Count; i++)
                if (crew[i].isAlive)
                    aliveIndices.Add(i);

            if (aliveIndices.Count > 0)
                targetIndex = aliveIndices[Random.Range(0, aliveIndices.Count)];
            else
                // 생존한 승무원이 없음
                return;
        }

        // 유효한 대상인지 확인
        if (targetIndex >= 0 && targetIndex < crew.Count && crew[targetIndex].isAlive)
        {
            var targetCrew = crew[targetIndex];

            // 효과 유형에 따른 처리
            switch (effect.effectType)
            {
                case CrewEffectType.Damage:
                    targetCrew.health = Mathf.Max(0, targetCrew.health + effect.healthChange);
                    if (targetCrew.health <= 0) KillCrewMember(targetIndex);
                    break;

                case CrewEffectType.Heal:
                    targetCrew.health = Mathf.Min(targetCrew.maxHealth,
                        targetCrew.health + Mathf.Abs(effect.healthChange));
                    break;

                case CrewEffectType.StatusChange:
                    targetCrew.status = effect.statusEffect;
                    break;

                case CrewEffectType.Kill:
                    KillCrewMember(targetIndex);
                    break;
            }

            OnCrewChanged?.Invoke(targetIndex, targetCrew);
        }
    }

    // 모든 승무원에게 효과 적용
    public void ApplyEffectToAllCrew(CrewEffect effect)
    {
        for (var i = 0; i < crew.Count; i++)
            if (crew[i].isAlive)
            {
                var individualEffect = new CrewEffect
                {
                    effectType = effect.effectType,
                    targetCrewIndex = i,
                    healthChange = effect.healthChange,
                    statusEffect = effect.statusEffect
                };

                ApplyCrewEffect(individualEffect);
            }
    }

    private void KillCrewMember(int index)
    {
        if (index >= 0 && index < crew.Count)
        {
            crew[index].isAlive = false;
            crew[index].health = 0;
            crew[index].status = CrewStatus.Normal; // 사망 시 상태효과 제거

            Debug.Log($"Crew member {crew[index].crewName} has died!");
            OnCrewChanged?.Invoke(index, crew[index]);

            // 모든 승무원이 사망했는지 확인
            if (GetAliveCrewCount() <= 0)
                // 게임 오버 조건
                GameManager.Instance.ChangeGameState(GameState.GameOver);
        }
    }

    public int GetAliveCrewCount()
    {
        var count = 0;
        foreach (var member in crew)
            if (member.isAlive)
                count++;

        return count;
    }

    public CrewMember GetCrewMember(int index)
    {
        if (index >= 0 && index < crew.Count) return crew[index];
        return null;
    }

    // 일별 승무원 상태 업데이트
    public void UpdateCrewForNewDay()
    {
        for (var i = 0; i < crew.Count; i++)
            if (crew[i].isAlive)
            {
                // 상태에 따른 영향
                switch (crew[i].status)
                {
                    case CrewStatus.Sick:
                        // 아픈 상태는 지속적인 체력 손실
                        crew[i].health -= 5f;
                        if (crew[i].health <= 0) KillCrewMember(i);
                        break;

                    case CrewStatus.Injured:
                        // 부상은 약간 회복될 수 있음
                        if (Random.value < 0.3f) crew[i].status = CrewStatus.Normal;
                        break;

                    case CrewStatus.Normal:
                        // 정상 상태는 체력 회복
                        crew[i].health = Mathf.Min(crew[i].maxHealth, crew[i].health + 5f);
                        break;
                }

                OnCrewChanged?.Invoke(i, crew[i]);
            }
    }
}
