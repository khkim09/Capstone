using System;
using System.IO;
using UnityEngine;

public class ShipTemplateLoader : MonoBehaviour
{
    public Ship enemyShip;

    private void Start()
    {
        //enemyShip.Initialize();
        //LoadShipFromResources("ShipTemplates/template_ggqe");
    }

    /// <summary>
    /// Resources 폴더에서 함선 템플릿을 로드하여 새 인스턴스를 생성합니다.
    /// </summary>
    /// <param name="resourcePath">Resources 폴더 내 경로 (확장자 제외)</param>
    /// <returns>생성된 함선 객체, 실패 시 null</returns>
    public static void LoadShipFromResources(string resourcePath)
    {
        try
        {
            // Resources 폴더에서 파일 로드
            TextAsset shipData = Resources.Load<TextAsset>(resourcePath);

            if (shipData == null) Debug.LogWarning($"Ship template not found in Resources: {resourcePath}");

            // 임시 파일로 저장
            string tempPath = Path.Combine(Application.temporaryCachePath,
                $"temp_ship_{Path.GetFileName(resourcePath)}.es3");
            File.WriteAllBytes(tempPath, shipData.bytes);

            // 함선 로드
            ShipSerialization.LoadShip(tempPath);
            ShipSerialization.LoadShip(tempPath);
            // 임시 파일 삭제
            File.Delete(tempPath);

            ;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading ship from Resources: {e.Message} - {e.StackTrace}");
        }
    }

    /// <summary>
    /// Resources 폴더에서 함선 템플릿을 로드하여 기존 함선에 복원합니다.
    /// </summary>
    /// <param name="resourcePath">Resources 폴더 내 경로 (확장자 제외)</param>
    /// <param name="targetShip">복원할 대상 함선</param>
    /// <returns>복원 성공 여부</returns>
    public static bool RestoreShipFromResources(string resourcePath, Ship targetShip)
    {
        if (targetShip == null)
            return false;

        try
        {
            // Resources 폴더에서 파일 로드
            TextAsset shipData = Resources.Load<TextAsset>(resourcePath);

            if (shipData == null)
            {
                Debug.LogWarning($"Ship template not found in Resources: {resourcePath}");
                return false;
            }

            // 임시 파일로 저장
            string tempPath = Path.Combine(Application.temporaryCachePath,
                $"temp_ship_{Path.GetFileName(resourcePath)}.es3");
            File.WriteAllBytes(tempPath, shipData.bytes);

            // 함선 복원
            bool result = ShipSerialization.RestoreShip(tempPath, targetShip);

            // 임시 파일 삭제
            File.Delete(tempPath);

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error restoring ship from Resources: {e.Message} - {e.StackTrace}");
            return false;
        }
    }
}
