using System;
using UnityEngine;

/// <summary>
/// 개별 투사체 클래스 (포물선 움직임 지원)
/// </summary>
public class MyProjectile : MonoBehaviour
{
    private ProjectileData data;
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private Action onHitCallback;
    private float lifetime;
    private float speed;

    // 포물선 움직임 관련
    private bool isParabolic;
    private Vector3 middleControlPoint; // 베지어 곡선의 중간 제어점
    private float totalDistance;
    private float journeyProgress; // 0 to 1

    // 직선 -> 포물선 전환
    private const float LINEAR_DURATION = 2f; // 1초 동안 직선으로 이동
    private bool hasStartedParabolic = false;
    private Vector3 parabolicStartPosition; // 포물선 시작 지점

    public void Initialize(ProjectileData projectileData)
    {
        data = projectileData;
        speed = data.speed;
    }

    public void Fire(Vector3 target, Action callback)
    {
        targetPosition = target;
        startPosition = transform.position;
        onHitCallback = callback;
        lifetime = 0f;
        journeyProgress = 0f;

        // 움직임 타입에 따른 초기화
        SetupMovement();

        // 방향 설정 (초기에만)
        Vector3 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void SetupMovement()
    {
        isParabolic = data.movementType == ProjectileMovementType.Parabolic && data.parabolicAngle > 0f;
        hasStartedParabolic = false;

        // 포물선 투사체도 처음에는 직선으로 시작하므로 여기서는 베지어 곡선을 설정하지 않음
    }

    /// <summary>
    /// 1초 후 포물선 시작 시 호출되는 메서드
    /// </summary>
    private void SetupParabolicMovement()
    {
        if (!isParabolic) return;

        // 현재 위치를 포물선 시작점으로 설정
        parabolicStartPosition = transform.position;

        // 포물선을 위한 베지어 곡선 설정
        Vector3 midPoint = (parabolicStartPosition + targetPosition) * 0.5f;

        // 현재 위치에서 목표점까지의 거리
        totalDistance = Vector3.Distance(parabolicStartPosition, targetPosition);

        if (totalDistance > 0)
        {
            // 포물선의 높이 계산 (각도를 기반으로)
            float heightOffset = totalDistance * Mathf.Tan(data.parabolicAngle * Mathf.Deg2Rad) * 0.5f;

            // 위아래 방향 랜덤 선택
            bool isUpward = data.allowDownwardParabola ? UnityEngine.Random.Range(0, 2) == 0 : true;
            heightOffset *= isUpward ? 1f : -1f;

            // 중간 제어점 설정 (베지어 곡선용)
            middleControlPoint = midPoint + Vector3.up * heightOffset;

            // 진행도 초기화
            journeyProgress = 0f;
            hasStartedParabolic = true;
        }
    }

    public void UpdateProjectile(float deltaTime)
    {
        lifetime += deltaTime;

        // 수명 체크
        if (lifetime > data.maxLifetime)
        {
            ProjectileManager.Instance.ReturnProjectileToPool(this);
            return;
        }

        // 포물선 타입이면서 1초 후 아직 포물선을 시작하지 않았다면
        if (isParabolic && !hasStartedParabolic && lifetime >= LINEAR_DURATION) SetupParabolicMovement();

        // 움직임 업데이트
        if (isParabolic && hasStartedParabolic)
            UpdateParabolicMovement(deltaTime);
        else
            UpdateLinearMovement(deltaTime);

        // 목표 도달 체크
        if ((isParabolic && hasStartedParabolic && journeyProgress >= 1f) ||
            Vector3.Distance(transform.position, targetPosition) < 0.1f)
            OnHitTarget();
    }

    private void UpdateLinearMovement(float deltaTime)
    {
        // 직선 움직임 (기존 로직)
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 previousPosition = transform.position;
        transform.position += direction * speed * deltaTime;

        // 회전 업데이트
        if (Vector3.Distance(previousPosition, transform.position) > 0.001f)
        {
            Vector3 moveDirection = (transform.position - previousPosition).normalized;
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 진행도 계산
        journeyProgress = 1f - Vector3.Distance(transform.position, targetPosition) / totalDistance;
    }

    private void UpdateParabolicMovement(float deltaTime)
    {
        // 거리 기반 진행도 계산
        float distanceToTravel = speed * deltaTime;

        if (totalDistance > 0)
        {
            // 전체 거리에 대한 진행도 업데이트
            journeyProgress += distanceToTravel / totalDistance;
            journeyProgress = Mathf.Clamp01(journeyProgress);

            // 베지어 곡선을 따라 위치 계산 (포물선 시작점부터)
            Vector3 previousPosition = transform.position;
            transform.position = CalculateBezierPoint(journeyProgress, parabolicStartPosition, middleControlPoint,
                targetPosition);

            // 움직임 방향에 따른 회전 업데이트
            if (Vector3.Distance(previousPosition, transform.position) > 0.001f)
            {
                Vector3 moveDirection = (transform.position - previousPosition).normalized;
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
    }

    /// <summary>
    /// 베지어 곡선 상의 점 계산
    /// </summary>
    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // 2차 베지어 곡선: B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = uu * p0; // (1-t)² * P0
        point += 2 * u * t * p1; // 2(1-t)t * P1
        point += tt * p2; // t² * P2

        return point;
    }

    private void OnHitTarget()
    {
        // 목표 위치로 정확히 이동
        transform.position = targetPosition;

        // 이펙트 생성
        CreateImpactEffect();

        // 콜백 호출
        onHitCallback?.Invoke();

        // 투사체 반환
        ProjectileManager.Instance.ReturnProjectileToPool(this);
    }

    private void CreateImpactEffect()
    {
        // ProjectileManager의 중앙 이펙트 시스템 사용
        if (ProjectileManager.Instance != null)
            ProjectileManager.Instance.CreateImpactEffect(
                data.impactEffect,
                targetPosition,
                data.impactEffectDuration
            );
    }

    public int GetProjectileId()
    {
        return data.projectileId;
    }

    public ShipWeaponType GetWeaponType()
    {
        // 호환성을 위해 첫 번째 호환 무기 타입 반환
        if (data.compatibleWeaponTypes != null && data.compatibleWeaponTypes.Length > 0)
            return data.compatibleWeaponTypes[0];

        // 기본값
        return ShipWeaponType.Laser;
    }
}
