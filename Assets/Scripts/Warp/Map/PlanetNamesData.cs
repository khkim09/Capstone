using System;

/// <summary>
/// 행성 이름 생성에 사용되는 접두사(prefix) 배열을 담는 데이터 컨테이너 클래스입니다.
/// 직렬화가 가능하여 JSON 등 외부 파일로부터 데이터를 불러오는 데 사용됩니다.
/// </summary>
[Serializable]
public class PlanetNamesData
{
    /// <summary>
    /// 행성 이름 접두사 리스트입니다. 예: "RCE", "SIS" 등
    /// </summary>
    public string[] prefixes;
}
