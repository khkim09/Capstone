using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Easy Save 3를 사용하여 함선 무기의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// </summary>
public static class ShipWeaponSerialization
{
    /// <summary>
    /// 모든 무기를 저장합니다.
    /// </summary>
    /// <param name="weapons">저장할 무기 목록</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveAllWeapons(List<ShipWeapon> weapons, string filename)
    {
        // 파일이 이미 존재한다면 해당 파일 삭제 (덮어쓰기)
        if (ES3.FileExists(filename))
            ES3.DeleteFile(filename);

        // 모든 무기 저장
        ES3.Save("weaponCount", weapons.Count, filename);

        for (int i = 0; i < weapons.Count; i++)
            ES3.Save($"weapon_{i}", weapons[i], filename);
    }

    /// <summary>
    /// 함선의 모든 무기를 저장합니다.
    /// </summary>
    /// <param name="ship">무기가 설치된 함선</param>
    /// <param name="filename">저장 파일명</param>
    public static void SaveShipWeapons(Ship ship, string filename)
    {
        if (ship == null)
            return;

        SaveAllWeapons(ship.GetAllWeapons(), filename);
    }

    /// <summary>
    /// 저장된 모든 무기를 불러옵니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <returns>불러온 무기 목록</returns>
    public static List<ShipWeapon> LoadAllWeapons(string filename)
    {
        List<ShipWeapon> weapons = new();

        if (!ES3.FileExists(filename))
            return weapons;

        // 무기 수 불러오기
        int weaponCount = ES3.Load<int>("weaponCount", filename);

        // 각 무기 불러오기
        for (int i = 0; i < weaponCount; i++)
            if (ES3.KeyExists($"weapon_{i}", filename))
            {
                ShipWeapon weapon = ES3.Load<ShipWeapon>($"weapon_{i}", filename);
                if (weapon != null)
                    weapons.Add(weapon);
            }

        return weapons;
    }

    /// <summary>
    /// 함선에 모든 무기를 복원합니다.
    /// </summary>
    /// <param name="filename">불러올 파일명</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>복원된 무기 수</returns>
    public static int RestoreAllWeaponsToShip(string filename, Ship ship)
    {
        if (!ES3.FileExists(filename) || ship == null)
            return 0;

        // 기존 무기 제거
        List<ShipWeapon> existingWeapons = new(ship.GetAllWeapons());
        foreach (ShipWeapon weapon in existingWeapons)
            ship.RemoveWeapon(weapon);

        List<ShipWeapon> weapons = LoadAllWeapons(filename);

        foreach (ShipWeapon weapon in weapons)
            // ShipWeapon은 이미 로드된 상태이므로, 직접 함선에 추가
            ship.AddWeapon(weapon);

        return weapons.Count;
    }
}
