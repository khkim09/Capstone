using System;

/// <summary>
/// 함선에서 사용하는 자원의 종류를 나타냅니다.
/// </summary>
[Serializable]
public enum ResourceType
{
    /// <summary>연료(Fuel) – 워프 이동 및 엔진 작동에 사용됩니다.</summary>
    Fuel,

    /// <summary>COMA - 게임의 기초 재화입니다.</summary>
    COMA,

    Missile,

    Hypersonic
}
