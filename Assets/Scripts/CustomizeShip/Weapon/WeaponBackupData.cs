using System;
using UnityEngine;

[Serializable]
public struct WeaponBackupData
{
    public ShipWeaponData weaponData;
    public Vector2Int position;
    public ShipWeaponAttachedDirection direction;
}
