using System;
using UnityEngine;

/// <summary>
/// 백업할 선원 데이터 구조체
/// </summary>
[Serializable]
public struct BackupCrewData
{
    public CrewRace race;
    public string crewName;
    public bool needsOxygen;
    public Vector2Int position;
    public Vector2Int roomPos;
}
