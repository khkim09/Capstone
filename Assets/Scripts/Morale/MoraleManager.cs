using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 사기 관련 이벤트 생성에 따른 사기 효과 적용 관리 Manager.
/// </summary>
public class MoraleManager : MonoBehaviour
{
    public static MoraleManager Instance { get; private set; }

    [SerializeField] private GameObject moraleIconPrefab;
    [SerializeField] private RectTransform moraleStatusPanel;

    // 종족별 아이콘 참조 (None은 전체 선원용)
    private Dictionary<CrewRace, MoraleIcon> raceIcons = new();

    /// <summary>
    /// 현재 적용 중인 사기 효과 데이터
    /// </summary>
    private List<MoraleEffectData> activeEffects = new();

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

    private void Start()
    {
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged += CheckEventExpirations;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged -= CheckEventExpirations;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu")
        {
            if (moraleStatusPanel == null)
            {
                GameObject panelObj = GameObject.Find("Morale Status Layout");
                if (panelObj != null)
                {
                    moraleStatusPanel = panelObj.GetComponent<RectTransform>();
                }
                else
                {
                    Debug.LogError("Morale Status Layout을 찾을 수 없습니다!");
                    return;
                }
            }

            // Horizontal Layout Group 확인 및 추가
            HorizontalLayoutGroup layoutGroup = moraleStatusPanel.GetComponent<HorizontalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = moraleStatusPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.spacing = 10f;
                layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = false;
            }

            // 종족별 아이콘 미리 생성
            CreateRaceIcons();
        }
    }

    /// <summary>
    /// 종족별 아이콘 생성
    /// </summary>
    private void CreateRaceIcons()
    {
        // 먼저 모든 종족 아이콘 생성 준비
        CrewRace[] races =
        {
            CrewRace.None, // 전체
            CrewRace.Human, CrewRace.Amorphous, CrewRace.MechanicTank, CrewRace.MechanicSup, CrewRace.Beast,
            CrewRace.Insect
        };

        foreach (CrewRace race in races)
        {
            // UI 요소로 아이콘 생성 (부모는 레이아웃 그룹이 있는 패널)
            GameObject iconObj = Instantiate(moraleIconPrefab, moraleStatusPanel);

            // RectTransform 설정
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            if (rectTransform == null) rectTransform = iconObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(40f, 40f); // 기본 크기 설정

            // LayoutElement 추가 (레이아웃 그룹 내에서 크기 설정)
            LayoutElement layoutElement = iconObj.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = iconObj.AddComponent<LayoutElement>();
            layoutElement.minWidth = 40f;
            layoutElement.minHeight = 40f;
            layoutElement.preferredWidth = 40f;
            layoutElement.preferredHeight = 40f;

            // MoraleIcon 초기화
            MoraleIcon icon = iconObj.GetComponent<MoraleIcon>();
            if (icon == null) icon = iconObj.AddComponent<MoraleIcon>();
            icon.Initialize(race);

            // 딕셔너리에 저장
            raceIcons[race] = icon;

            // 초기에는 비활성화 (효과가 없을 때)
            iconObj.SetActive(false);
        }
    }

    /// <summary>
    /// 종족 별 사기 반환
    /// </summary>
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
    public float GetTotalMoraleBonus(CrewMember crew)
    {
        float raceMorale = GetRaceMorale(crew.race);
        return raceMorale + globalMorale;
    }

    /// <summary>
    /// 종족별 사기 설정
    /// </summary>
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

        // 모든 효과 제거
        activeEffects.Clear();

        // 모든 아이콘 비활성화
        foreach (MoraleIcon icon in raceIcons.Values)
        {
            icon.ClearEffectData();
            icon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 새로운 사기 효과 등록
    /// </summary>
    public void RegisterMoraleEffect(MoraleEffectData effectData)
    {
        // 효과 리스트에 추가
        activeEffects.Add(effectData);

        // 사기 값 적용
        ApplyMoraleEffectValue(effectData);

        // 해당 종족의 아이콘 업데이트
        UpdateRaceIcon(effectData.targetRace);
    }

    /// <summary>
    /// CrewEffect에서 MoraleEffectData 생성 및 등록
    /// </summary>
    public void RegisterMoraleEffect(CrewEffect crewEffect)
    {
        if (crewEffect.effectType != CrewEffectType.ChangeMorale)
        {
            Debug.LogError("사기 이벤트가 아닙니다");
            return;
        }

        int value = (int)crewEffect.changeAmount;
        int duration = (int)crewEffect.conditionAmount;

        if (duration <= 0)
        {
            Debug.LogError("지속 시간은 0보다 커야 합니다");
            return;
        }

        // 효과 데이터 생성
        MoraleEffectData effectData = new(
            crewEffect.raceType,
            value,
            GameManager.Instance.CurrentYear,
            duration
        );

        // 효과 등록
        RegisterMoraleEffect(effectData);
    }

    /// <summary>
    /// 사기 효과 값 적용
    /// </summary>
    private void ApplyMoraleEffectValue(MoraleEffectData effect)
    {
        if (effect.targetRace == CrewRace.None)
        {
            float newMorale = globalMorale + effect.value;
            SetAllCrewMorale(newMorale);
        }
        else
        {
            float current = GetRaceMorale(effect.targetRace);
            SetRaceMorale(effect.targetRace, current + effect.value);
        }
    }

    /// <summary>
    /// 사기 효과 값 제거
    /// </summary>
    private void RemoveMoraleEffectValue(MoraleEffectData effect)
    {
        if (effect.targetRace == CrewRace.None)
        {
            float newMorale = globalMorale - effect.value;
            SetAllCrewMorale(newMorale);
        }
        else
        {
            float current = GetRaceMorale(effect.targetRace);
            SetRaceMorale(effect.targetRace, current - effect.value);
        }
    }

    /// <summary>
    /// 종족별 아이콘 업데이트
    /// </summary>
    private void UpdateRaceIcon(CrewRace race)
    {
        if (!raceIcons.TryGetValue(race, out MoraleIcon icon))
        {
            Debug.LogError($"아이콘을 찾을 수 없습니다: {race}");
            return;
        }

        // 이 종족에 대한 모든 효과 찾기
        List<MoraleEffectData> raceEffects = activeEffects.FindAll(e => e.targetRace == race);

        // 아이콘의 효과 데이터 초기화
        icon.ClearEffectData();

        // 적용 중인 효과가 있으면 아이콘 활성화 및 데이터 추가
        if (raceEffects.Count > 0)
        {
            icon.gameObject.SetActive(true);

            foreach (MoraleEffectData effectData in raceEffects) icon.AddEffectData(effectData);
        }
        else
        {
            // 적용 중인 효과가 없으면 아이콘 비활성화
            icon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 모든 종족 아이콘 업데이트
    /// </summary>
    private void UpdateAllRaceIcons()
    {
        // 각 종족별로 아이콘 업데이트
        foreach (CrewRace race in raceIcons.Keys) UpdateRaceIcon(race);
    }

    /// <summary>
    /// 현재 이벤트 상태를 확인하고 만료된 효과를 제거합니다
    /// </summary>
    public void CheckEventExpirations(int currentYear)
    {
        // 만료된 효과 찾기
        List<CrewRace> racesToUpdate = new();

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            MoraleEffectData effect = activeEffects[i];

            if (currentYear >= effect.EndYear)
            {
                // 만료된 효과의 종족 기록
                if (!racesToUpdate.Contains(effect.targetRace)) racesToUpdate.Add(effect.targetRace);

                // 효과 값 제거
                RemoveMoraleEffectValue(effect);

                // 효과 목록에서 제거
                activeEffects.RemoveAt(i);
            }
        }

        // 변경된 종족의 아이콘만 업데이트
        foreach (CrewRace race in racesToUpdate) UpdateRaceIcon(race);
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

        Debug.Log("=== 적용 중인 효과 ===");
        foreach (MoraleEffectData effect in activeEffects)
            Debug.Log($"종족: {effect.targetRace}, 값: {effect.value}, 종료 년도: {effect.EndYear}");
    }
}
