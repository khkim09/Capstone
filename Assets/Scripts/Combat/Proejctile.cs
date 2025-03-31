using System;
using UnityEngine;

/// <summary>
/// 전투 중 발사된 투사체의 이동 및 충돌을 처리하는 클래스입니다.
/// 목표 지점까지 이동하며, 충돌 시 지정된 콜백을 실행합니다.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// 투사체의 목표 위치입니다.
    /// </summary>
    private Vector2 targetPosition;


    /// <summary>
    /// 목표에 도달했을 때 실행할 콜백 함수입니다.
    /// </summary>
    private Action onHitCallback;

    /// <summary>
    /// 투사체의 이동 속도입니다.
    /// </summary>
    private float speed = 10f;


    /// <summary>
    /// 투사체를 초기화합니다.
    /// 목표 위치와 충돌 콜백을 지정합니다.
    /// </summary>
    /// <param name="target">목표 위치 (월드 좌표).</param>
    /// <param name="onHit">충돌 시 실행할 콜백 함수.</param>
    public void Initialize(Vector2 target, Action onHit)
    {
        targetPosition = target;
        onHitCallback = onHit;
    }

    /// <summary>
    /// 매 프레임마다 호출되어 투사체를 이동시킵니다.
    /// 목표에 도달하면 충돌 처리를 수행합니다.
    /// </summary>
    private void Update()
    {
        // 2D 게임에 맞게 Vector2로 계산
        Vector2 currentPos = new(transform.position.x, transform.position.y);
        Vector2 targetPos = new(targetPosition.x, targetPosition.y);
        Vector2 direction = (targetPos - currentPos).normalized;

        transform.position = new Vector3(
            currentPos.x + direction.x * speed * Time.deltaTime,
            currentPos.y + direction.y * speed * Time.deltaTime,
            transform.position.z
        );

        // 목표 근처에 도달하면 콜백 호출 및 소멸
        if (Vector2.Distance(currentPos, targetPos) < 0.5f) OnHitTarget();
    }

    /// <summary>
    /// 목표에 도달했을 때 호출됩니다.
    /// 효과를 재생하고 콜백을 실행한 뒤 투사체를 파괴합니다.
    /// </summary>
    private void OnHitTarget()
    {
        // 충돌 효과 재생
        PlayHitEffect();

        // 콜백 호출
        onHitCallback?.Invoke();

        // 투사체 소멸
        Destroy(gameObject);
    }

    /// <summary>
    /// 충돌 시 효과(이펙트)를 재생합니다.
    /// </summary>
    private void PlayHitEffect()
    {
        // 충돌 효과 재생 (파티클 등)
        // 예: Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }
}
