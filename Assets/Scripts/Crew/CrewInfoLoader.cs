using UnityEngine;
using System.IO;

public class CrewInfoLoader : MonoBehaviour
{
    public static CrewInfoWrapper crewInfo;
    void Awake()
    {
        // Application.dataPath는 프로젝트의 Assets 폴더 경로를 반환합니다.
        string filePath = Path.Combine(Application.dataPath, "Data/CrewInfo.json");

        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);

            // JsonUtility는 C# 필드명과 JSON 키가 정확히 일치해야 합니다.
            // JSON 내 공백을 제거한 키 이름으로 변경합니다.
            jsonString = jsonString.Replace("\"산소 호흡\"", "\"산소호흡\"");
            jsonString = jsonString.Replace("\"조종실 숙련도\"", "\"조종실숙련도\"");
            jsonString = jsonString.Replace("\"엔진실 숙련도\"", "\"엔진실숙련도\"");
            jsonString = jsonString.Replace("\"전력실 숙련도\"", "\"전력실숙련도\"");
            jsonString = jsonString.Replace("\"배리어실 숙련도\"", "\"배리어실숙련도\"");
            jsonString = jsonString.Replace("\"조준석 숙련도\"", "\"조준석숙련도\"");
            jsonString = jsonString.Replace("\"탄약고 숙련도\"", "\"탄약고숙련도\"");
            jsonString = jsonString.Replace("\"의무실 숙련도\"", "\"의무실숙련도\"");
            jsonString = jsonString.Replace("\"수리 숙련도\"", "\"수리숙련도\"");

            // JsonUtility를 사용해 JSON 문자열을 CrewInfoWrapper 객체로 변환
            crewInfo = JsonUtility.FromJson<CrewInfoWrapper>(jsonString);
            Debug.Log("CrewInfo 데이터 로드 성공");

            // 사용 데이터 로드 방법 - crewInfo.인간형.공격력
        }
        else
        {
            Debug.LogError("파일을 찾을 수 없습니다: " + filePath);
        }
    }
}
