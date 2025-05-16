using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

/// <summary>
/// 템플릿을 기반으로 해적선을 생성하는 유틸리티 클래스
/// </summary>
public class EnemyShipFactory : MonoBehaviour
{
    [SerializeField] private EnemyShipDatabase shipDatabase;

    /// <summary>
    /// 제공된 템플릿 이름을 기반으로 해적선을 생성합니다
    /// </summary>
    /// <param name="templateName">사용할 해적선 템플릿 이름</param>
    /// <param name="position">배를 생성할 월드 위치</param>
    /// <returns>생성된 배 인스턴스</returns>
    public Ship SpawnPirateShip(string templateName, Vector2Int position = new())
    {
        // 데이터베이스에서 템플릿 가져오기
        EnemyShipTemplate template = shipDatabase.GetShipTemplate(templateName);

        if (template == null)
        {
            Debug.LogError($"템플릿을 찾을 수 없음: {templateName}, 기본 템플릿으로 대체합니다.");
            template = shipDatabase.GetShipTemplate("pirate_default");

            // 기본 템플릿도 없으면 실패
            if (template == null)
            {
                Debug.LogError("기본 해적선 템플릿도 찾을 수 없음. 생성 실패.");
                return null;
            }
        }

        // 템플릿 로드 시 생성될 배의 예상 이름
        string expectedShipName = "EnemyShip"; // ES3가 생성하는 이름 규칙에 맞게 조정 필요

        try
        {
            // 템플릿에서 배 로드
            string templatePath = template.GetTemplatePath();
            ShipTemplateLoader.LoadShipFromResources(templatePath);

            // ES3가 생성한 배 찾기
            Ship loadedShip = FindShipByName(expectedShipName).GetComponent<Ship>();

            if (loadedShip != null)
            {
                Ship playerShip = GameManager.Instance.GetPlayerShip();

                // TODO: MoveTo 함수 완성하기
                loadedShip.MoveShipToFacingTargetShip(playerShip);

                GeneratePirateCrew(loadedShip, template);

                return loadedShip;
            }
            else
            {
                Debug.LogError($"ES3로 로드된 배를 찾을 수 없습니다: {expectedShipName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"해적선 템플릿 로드 중 오류 발생: {e.Message}");

            // 기본 템플릿 시도
            if (template.templateName != "pirate_default")
            {
                Debug.Log("기본 해적 템플릿 로드 시도 중");
                try
                {
                    ShipTemplateLoader.LoadShipFromResources("ShipTemplates/template_pirate_default");

                    // ES3가 생성한 기본 배 찾기
                    Ship defaultShip = FindShipByName("Ship_pirate_default");

                    if (defaultShip != null)
                    {
                        defaultShip.gameObject.name = $"Pirate_Default";

                        // 선원 생성 및 추가
                        GeneratePirateCrew(defaultShip, template);

                        return defaultShip;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"기본 해적선 템플릿 로드 실패: {ex.Message}");
                }
            }
        }

        // 여기까지 오면 로드 실패, 수동으로 생성
        Debug.LogWarning("템플릿 로드 실패. 기본 배를 수동으로 생성합니다.");
        return CreateDefaultShip(template, position);
    }

    /// <summary>
    /// 이름으로 Ship 컴포넌트가 있는 게임오브젝트를 찾습니다
    /// </summary>
    private Ship FindShipByName(string shipName)
    {
        // 씬에서 모든 배 찾기
        Ship[] allShips = FindObjectsOfType<Ship>();

        // 이름이 일치하는 배 찾기
        foreach (Ship ship in allShips)
            if (ship.gameObject.name == shipName)
                return ship;

        return null;
    }

    /// <summary>
    /// 기본 배를 수동으로 생성합니다
    /// </summary>
    private Ship CreateDefaultShip(EnemyShipTemplate template, Vector2Int position)
    {
        // 새 게임 오브젝트 생성
        GameObject shipObject = new($"Pirate_{template.templateName}_Manual");

        // Ship 컴포넌트 추가
        Ship ship = shipObject.AddComponent<Ship>();

        shipObject.transform.position = ship.GetWorldPositionFromGrid(position);

        // 배 초기화
        ship.Initialize();

        // 기본 설정 적용
        ApplyDefaultShipSettings(ship);

        // 선원 생성 및 추가
        GeneratePirateCrew(ship, template);

        return ship;
    }

    /// <summary>
    /// 기본 배 설정을 적용합니다 (템플릿 로드 실패 시)
    /// </summary>
    private void ApplyDefaultShipSettings(Ship ship)
    {
        // 기본 설정 적용 - 필요에 따라 구현
        // 예: ship.SetHealth(100);
        Debug.Log("배에 기본 설정을 적용합니다.");
    }

    /// <summary>
    /// 템플릿 설정을 기반으로 해적선에 대한 무작위 선원을 생성합니다
    /// </summary>
    private static void GeneratePirateCrew(Ship ship, EnemyShipTemplate template)
    {
        // 기존 선원 제거 (중복 방지)
        if (ship.GetCrewCount() > 0)
        {
            Debug.Log("기존 선원을 제거하고 새 선원을 생성합니다.");
            ship.RemoveAllCrews();
        }

        // 선원 수 결정
        int crewCount = Random.Range(template.minCrewCount, template.maxCrewCount + 1);

        // 다양성을 위해 사용된 종족 추적
        List<CrewRace> usedRaces = new();

        // 가중치 종족 선택 시스템 생성
        List<CrewRaceOption> availableRaces = template.possibleRaces;

        // 지정된 종족이 없는 경우 기본값 사용
        if (availableRaces.Count == 0)
            availableRaces = new List<CrewRaceOption>
            {
                new() { race = CrewRace.Human, spawnWeight = 1.0f },
                new() { race = CrewRace.Beast, spawnWeight = 0.7f },
                new() { race = CrewRace.Insect, spawnWeight = 0.5f }
            };

        // 각 선원 생성
        for (int i = 0; i < crewCount; i++)
        {
            // 종족 선택
            CrewRace selectedRace = SelectRace(availableRaces, usedRaces);
            usedRaces.Add(selectedRace);

            // 종족에 따른 이름 생성
            string crewName = GeneratePirateName(selectedRace);

            // 선원 생성
            CrewBase newCrew = GameObjectFactory.Instance.CreateCrewInstance(selectedRace, crewName, false);

            // 배에 추가
            ship.AddCrew(newCrew);

            // enemyController (적 AI) 컴포넌트 부착
            newCrew.AddComponent<EnemyController>();
        }
    }

    /// <summary>
    /// 가중치 및 다양성 설정에 따라 선원 종족 선택
    /// </summary>
    private static CrewRace SelectRace(List<CrewRaceOption> options, List<CrewRace> usedRaces)
    {
        // 다양성을 강제하고 이미 사용된 종족이 있는 경우
        if (usedRaces.Count > 0 && usedRaces.Count < options.Count)
        {
            // 이미 사용된 종족 필터링
            List<CrewRaceOption> unusedOptions = options.Where(o => !usedRaces.Contains(o.race)).ToList();
            return WeightedRandomRace(unusedOptions);
        }

        // 표준 가중치 선택
        return WeightedRandomRace(options);
    }

    /// <summary>
    /// 가중치에 따라 무작위 종족 선택
    /// </summary>
    private static CrewRace WeightedRandomRace(List<CrewRaceOption> options)
    {
        float totalWeight = options.Sum(o => o.spawnWeight);
        float randomValue = Random.Range(0f, totalWeight);

        float currentWeight = 0f;
        foreach (CrewRaceOption option in options)
        {
            currentWeight += option.spawnWeight;
            if (randomValue <= currentWeight)
                return option.race;
        }

        // 대체 방안
        return options[0].race;
    }

    /// <summary>
    /// 종족에 따른 해적 이름 생성
    /// </summary>
    private static string GeneratePirateName(CrewRace race)
    {
        string[] piratePrefix = { "선장", "일등항해사", "갑판장", "포수", "병참장교", "장비원", "항해사" };
        string[] humanNames = { "검은수염", "플린트", "스패로우", "모건", "키드", "보니", "후크", "라캄" };
        string[] beastNames = { "송곳니", "발톱", "으르렁", "배회자", "야만인", "사냥꾼" };
        string[] insectNames = { "침", "턱", "갑각", "군체" };
        string[] amorphousNames = { "흐름", "변형", "공허", "아지랑이" };
        string[] mechNames = { "봇", "서보", "유닛", "기어" };

        string prefix = piratePrefix[Random.Range(0, piratePrefix.Length)];
        string name;

        // 종족에 따른 이름 선택
        switch (race)
        {
            case CrewRace.Human:
                name = humanNames[Random.Range(0, humanNames.Length)];
                break;
            case CrewRace.Beast:
                name = beastNames[Random.Range(0, beastNames.Length)];
                break;
            case CrewRace.Insect:
                name = insectNames[Random.Range(0, insectNames.Length)];
                break;
            case CrewRace.Amorphous:
                name = amorphousNames[Random.Range(0, amorphousNames.Length)];
                break;
            case CrewRace.MechanicSup:
            case CrewRace.MechanicTank:
                name = mechNames[Random.Range(0, mechNames.Length)];
                break;
            default:
                name = "해적";
                break;
        }

        // 해적 접미사를 추가할 확률
        if (Random.value < 0.3f)
        {
            string[] suffixes = { "잔인한", "외눈", "목발다리", "공포의", "피에굶주린", "사나운" };
            name += " " + suffixes[Random.Range(0, suffixes.Length)];
        }

        return $"{prefix} {name}";
    }
}
