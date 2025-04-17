// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// /// <summary>
// /// Ship 클래스에 직렬화 관련 확장 메서드를 제공하는 클래스
// /// </summary>
// public static class ShipSerializationExtensions
// {
//     /// <summary>
//     /// 함선을 JSON 문자열로 직렬화
//     /// </summary>
//     /// <param name="ship">대상 함선</param>
//     /// <returns>직렬화된 JSON 문자열</returns>
//     public static string ToJson(this Ship ship)
//     {
//         return ShipSerialization.SerializeShip(ship);
//     }
//
//     /// <summary>
//     /// JSON 문자열에서 함선을 역직렬화
//     /// </summary>
//     /// <param name="ship">대상 함선</param>
//     /// <param name="json">JSON 문자열</param>
//     /// <returns>성공 여부</returns>
//     public static bool FromJson(this Ship ship, string json)
//     {
//         return ShipSerialization.DeserializeShip(json, ship);
//     }
//
//     /// <summary>
//     /// 함선을 파일로 저장
//     /// </summary>
//     /// <param name="ship">대상 함선</param>
//     /// <param name="filePath">저장 경로</param>
//     /// <returns>성공 여부</returns>
//     public static bool SaveToFile(this Ship ship, string filePath)
//     {
//         return ShipSerialization.SaveShipToFile(ship, filePath);
//     }
//
//     /// <summary>
//     /// 파일에서 함선 불러오기
//     /// </summary>
//     /// <param name="ship">대상 함선</param>
//     /// <param name="filePath">파일 경로</param>
//     /// <returns>성공 여부</returns>
//     public static bool LoadFromFile(this Ship ship, string filePath)
//     {
//         return ShipSerialization.LoadShipFromFile(filePath, ship);
//     }
//
//     /// <summary>
//     /// 템플릿에서 함선 생성
//     /// </summary>
//     /// <param name="ship">대상 함선</param>
//     /// <param name="templateId">템플릿 ID</param>
//     /// <returns>성공 여부</returns>
//     public static bool LoadFromTemplate(this Ship ship, string templateId)
//     {
//         return ShipTemplateSystem.Instance.CreateShipFromTemplate(templateId, ship);
//     }
// }


