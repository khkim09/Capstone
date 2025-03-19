using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

public class RaceTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI statsText;
    CrewInfoWrapper crewInfoWrapper;

    // 예시: 각 종족의 능력치를 설정하는 함수
    public void SetupTooltip(CrewRace raceType)
    {
        // json data 가져오기
        crewInfoWrapper = CrewInfoLoader.crewInfo;

        if (crewInfoWrapper == null)
        {
            statsText.text = "No data available";
            Debug.LogError("CrewInfo 데이터가 로드되지 않았습니다!");
            return;
        }

        raceNameText.text = raceType.ToString();

        // 선택한 종족에 해당하는 CrewData 가져오기
        CrewData crewData = null;
        switch (raceType)
        {
            case CrewRace.Human:
                crewData = crewInfoWrapper.인간형;
                break;
            case CrewRace.Amorphous:
                crewData = crewInfoWrapper.부정형;
                break;
            case CrewRace.MechanicTank:
                crewData = crewInfoWrapper.돌격기계형;
                break;
            case CrewRace.MechanicSup:
                crewData = crewInfoWrapper.지원기계형;
                break;
            case CrewRace.Beast:
                crewData = crewInfoWrapper.짐승형;
                break;
            case CrewRace.Insect:
                crewData = crewInfoWrapper.곤충형;
                break;
        }

        if (crewData == null)
        {
            statsText.text = "해당 종족의 데이터가 없습니다.";
            return;
        }

        // Reflection을 사용하여 CrewData의 모든 필드를 순회합니다.
        FieldInfo[] fields = typeof(CrewData).GetFields();
        StringBuilder sb = new StringBuilder();
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(crewData);
            // 각 필드의 이름과 값을 "필드명 : 값" 형태로 추가합니다.
            sb.AppendLine($"{field.Name} : {value}");
        }

        // 완성된 문자열을 statsText.text에 할당합니다.
        statsText.text = sb.ToString();
    }
}
