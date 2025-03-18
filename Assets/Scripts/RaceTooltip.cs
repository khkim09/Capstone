using UnityEngine;
using TMPro;

public class RaceTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI statsText;

    // 예시: 각 종족의 능력치를 설정하는 함수
    public void SetupTooltip(CrewRace raceType)
    {
        raceNameText.text = raceType.ToString();
        // 각 종족에 맞는 능력치를 여기서 설정 (데이터베이스나 ScriptableObject 등에서 불러올 수도 있음)
        switch (raceType)
        {
            case CrewRace.Human:
                statsText.text = "Max Health : 100\nAttack : 10\nDefense : 8\n";
                break;
            case CrewRace.Amorphous:
                statsText.text = "Max Health : 100\nAttack : 6\nDefense : 6\n";
                break;
            case CrewRace.MechanicTank:
                statsText.text = "Max Health : 120\nAttack : 5\nDefense : 5\n";
                break;
            case CrewRace.MechanicSup:
                statsText.text = "Max Health : 70\nAttack : 5\nDefense : 5\n";
                break;
            case CrewRace.Beast:
                statsText.text = "Max Health : 120\nAttack : 12\nDefense : 12\n";
                break;
            case CrewRace.Insect:
                statsText.text = "Max Health : 120\nAttack : 10\nDefense : 15\n";
                break;
            // 기타 종족 케이스 추가
            default:
                statsText.text = "기본 능력치";
                break;
        }
    }
}
