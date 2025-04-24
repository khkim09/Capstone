using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Easy Save 3를 사용하여 선원(CrewBase) 객체의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// </summary>
public static class CrewSerialization
{
    /// <summary>
    /// 모든 선원을 저장합니다.
    /// </summary>
    /// <param name="crews">저장할 선원 목록</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveAllCrews(List<CrewBase> crews, string filename)
    {
        // 파일이 이미 존재한다면 해당 파일 삭제 (덮어쓰기)
        if (ES3.FileExists(filename))
            ES3.DeleteFile(filename);

        // 모든 선원 저장
        ES3.Save("crewCount", crews.Count, filename);

        for (int i = 0; i < crews.Count; i++) ES3.Save($"crew_{i}", crews[i], filename);
    }

    /// <summary>
    /// 함선에 탑승 중인 모든 선원을 저장합니다.
    /// </summary>
    /// <param name="ship">선원이 탑승 중인 함선</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveShipCrews(Ship ship, string filename)
    {
        if (ship == null)
            return;

        SaveAllCrews(ship.GetAllCrew(), filename);
    }

    /// <summary>
    /// 저장된 모든 선원을 불러옵니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <returns>불러온 선원 목록</returns>
    public static List<CrewBase> LoadAllCrews(string filename)
    {
        List<CrewBase> crews = new();

        if (!ES3.FileExists(filename))
            return crews;

        // 선원 수 불러오기
        int crewCount = ES3.Load<int>("crewCount", filename);

        // 각 선원 불러오기
        for (int i = 0; i < crewCount; i++)
            if (ES3.KeyExists($"crew_{i}", filename))
            {
                CrewBase crew = ES3.Load<CrewBase>($"crew_{i}", filename);
                if (crew != null)
                    crews.Add(crew);
            }

        return crews;
    }

    /// <summary>
    /// 함선에 모든 선원을 복원합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>복원된 선원 수</returns>
    public static int RestoreAllCrewsToShip(string filename, Ship ship)
    {
        if (!ES3.FileExists(filename) || ship == null)
            return 0;

        ship.RemoveAllCrews();
        List<CrewBase> crews = LoadAllCrews(filename);


        foreach (CrewBase crew in crews)
        {
            CrewBase newCrew = GameObjectFactory.Instance.CrewFactory.CreateCrewObject(crew);
            ship.AddCrew(newCrew);
        }

        return crews.Count;
    }
}
