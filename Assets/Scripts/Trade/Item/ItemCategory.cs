using System;

/// <summary>
/// 아이템의 분류를 나타내는 열거형입니다.
/// </summary>
[Serializable]
public enum ItemCategory
{
    /// <summary>광물</summary>
    Mineral,

    /// <summary>동물</summary>
    Livestock,

    /// <summary>무기</summary>
    Weapon,

    /// <summary>보석</summary>
    Gem,

    /// <summary>사치품</summary>
    Luxury,

    /// <summary>소재</summary>
    Material,

    /// <summary>식량</summary>
    Food,

    /// <summary>유물</summary>
    Artifact,

    /// <summary>향신료</summary>
    Spice,

    /// <summary>의약품</summary>
    Medicine,

    /// <summary>기본</summary>
    Default
}
