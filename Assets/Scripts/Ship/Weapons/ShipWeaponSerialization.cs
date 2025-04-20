using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 함선 무기의 직렬화 및 역직렬화를 담당하는 유틸리티 클래스
/// </summary>
public static class ShipWeaponSerialization
{
    /// <summary>
    /// 무기 직렬화 데이터 클래스
    /// </summary>
    [Serializable]
    public class ShipWeaponSerializationData
    {
        // 무기 데이터 ID (스크립터블 오브젝트 참조용)
        public int weaponId;

        // 위치 및 방향 정보
        public Vector2Int gridPosition;
        public ShipWeaponAttachedDirection attachedDirection;

        // 상태 정보
        public bool isEnabled = true;
        public float currentCooldown = 0f;

        // 통계 정보
        public int hits = 0;
        public float totalDamageDealt = 0f;
    }

    /// <summary>
    /// 무기를 직렬화하여 데이터 객체로 변환
    /// </summary>
    /// <param name="weapon">직렬화할 무기</param>
    /// <returns>직렬화된 무기 데이터</returns>
    public static ShipWeaponSerializationData SerializeWeapon(ShipWeapon weapon)
    {
        if (weapon == null || weapon.weaponData == null)
            return null;

        return new ShipWeaponSerializationData
        {
            weaponId = weapon.weaponData.id,
            gridPosition = weapon.GetGridPosition(),
            attachedDirection = weapon.GetAttachedDirection(),
            isEnabled = weapon.IsEnabled(),
            currentCooldown = 0f, // 현재 시스템에서는 쿨다운 값을 0-100 사이로 관리
            hits = weapon.GetHits(),
            totalDamageDealt = weapon.GetTotalDamageDealt()
        };
    }

    /// <summary>
    /// 함선의 모든 무기를 직렬화
    /// </summary>
    /// <param name="ship">대상 함선</param>
    /// <returns>직렬화된 무기 데이터 목록</returns>
    public static List<ShipWeaponSerializationData> SerializeAllWeapons(Ship ship)
    {
        List<ShipWeaponSerializationData> result = new();

        if (ship == null)
            return result;

        List<ShipWeapon> weapons = ship.GetAllWeapons();
        foreach (ShipWeapon weapon in weapons)
        {
            ShipWeaponSerializationData data = SerializeWeapon(weapon);
            if (data != null)
                result.Add(data);
        }

        return result;
    }

    /// <summary>
    /// 직렬화된 무기 데이터로 무기 객체 생성
    /// </summary>
    /// <param name="data">직렬화된 무기 데이터</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>생성된 무기 객체</returns>
    public static ShipWeapon DeserializeWeapon(ShipWeaponSerializationData data, Ship ship)
    {
        if (data == null || ship == null)
            return null;

        // 무기 데이터 로드
        ShipWeaponData weaponData = GameObjectFactory.Instance.ShipWeaponFactory.GetWeaponData(data.weaponId);
        if (weaponData == null)
        {
            Debug.LogWarning($"무기 데이터를 찾을 수 없음: ID {data.weaponId}");
            return null;
        }

        // 무기 생성
        ShipWeapon weapon = ship.AddWeapon(data.weaponId, data.gridPosition, data.attachedDirection);

        if (weapon != null)
        {
            // 상태 복원
            if (!data.isEnabled)
                weapon.SetEnabled(false);

            // 통계 데이터 복원
            weapon.SetHits(data.hits);
            weapon.SetTotalDamageDealt(data.totalDamageDealt);

            // 쿨다운 복원 (현재 시스템에 맞게 조정)
            weapon.ResetCooldown(data.currentCooldown);
        }

        return weapon;
    }

    /// <summary>
    /// 함선에 모든 무기 복원
    /// </summary>
    /// <param name="weaponDataList">직렬화된 무기 데이터 목록</param>
    /// <param name="ship">대상 함선</param>
    /// <returns>복원된 무기 수</returns>
    public static int DeserializeAllWeapons(List<ShipWeaponSerializationData> weaponDataList, Ship ship)
    {
        if (weaponDataList == null || ship == null)
            return 0;

        int restoredCount = 0;

        // 기존 무기 제거 (Ship 클래스의 메서드 사용)
        List<ShipWeapon> existingWeapons = new(ship.GetAllWeapons());
        foreach (ShipWeapon weapon in existingWeapons) ship.RemoveWeapon(weapon);

        // 무기 복원
        foreach (ShipWeaponSerializationData data in weaponDataList)
        {
            ShipWeapon weapon = DeserializeWeapon(data, ship);
            if (weapon != null)
                restoredCount++;
        }

        return restoredCount;
    }

    /// <summary>
    /// 무기 데이터를 JSON 문자열로 변환
    /// </summary>
    /// <param name="data">직렬화할 무기 데이터</param>
    /// <returns>JSON 문자열</returns>
    public static string ToJson(ShipWeaponSerializationData data)
    {
        if (data == null)
            return "{}";

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    /// <summary>
    /// 무기 데이터 목록을 JSON 문자열로 변환
    /// </summary>
    /// <param name="dataList">직렬화할 무기 데이터 목록</param>
    /// <returns>JSON 문자열</returns>
    public static string ToJson(List<ShipWeaponSerializationData> dataList)
    {
        if (dataList == null)
            return "[]";

        return JsonConvert.SerializeObject(dataList, Formatting.Indented);
    }

    /// <summary>
    /// JSON 문자열에서 무기 데이터 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>복원된 무기 데이터</returns>
    public static ShipWeaponSerializationData FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return JsonConvert.DeserializeObject<ShipWeaponSerializationData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"무기 데이터 역직렬화 오류: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// JSON 문자열에서 무기 데이터 목록 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>복원된 무기 데이터 목록</returns>
    public static List<ShipWeaponSerializationData> FromJsonList(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new List<ShipWeaponSerializationData>();

        try
        {
            return JsonConvert.DeserializeObject<List<ShipWeaponSerializationData>>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"무기 데이터 목록 역직렬화 오류: {e.Message}");
            return new List<ShipWeaponSerializationData>();
        }
    }
}
