using System;
using UnityEngine;

/// <summary>
/// 투사체 움직임 타입
/// </summary>
public enum ProjectileMovementType
{
    Linear, // 직선 움직임
    Parabolic // 포물선 움직임
}

/// <summary>
/// 투사체 데이터 클래스
/// </summary>
[Serializable]
public class ProjectileData
{
    [Header("기본 정보")] [Tooltip("투사체의 고유 ID (무기에서 참조)")]
    public int projectileId;

    [Tooltip("투사체 이름 (관리용)")] public string projectileName;

    [Tooltip("투사체 프리팹")] public GameObject projectilePrefab;

    [Header("성능 설정")] public float speed;
    public float maxLifetime;

    [Header("오디오/비주얼")] public AudioClip fireSound;
    public GameObject impactEffect;

    [Header("이펙트 설정")] [Tooltip("임팩트 이펙트가 지속되는 시간 (초)")]
    public float impactEffectDuration = 2f;

    [Header("움직임 설정")] [Tooltip("투사체의 움직임 타입")]
    public ProjectileMovementType movementType = ProjectileMovementType.Linear;

    [Range(0f, 90f)] [Tooltip("포물선 각도 (0도 = 직선, 최대 90도 포물선)")]
    public float parabolicAngle = 0f;

    [Tooltip("아래쪽 포물선 허용 여부 (false면 위쪽만)")]
    public bool allowDownwardParabola = true;

    [Header("호환성 정보")] [Tooltip("이 투사체를 사용할 수 있는 무기 타입들")]
    public ShipWeaponType[] compatibleWeaponTypes;
}
