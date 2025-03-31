using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 targetPosition;
    private Action onHitCallback;
    private float speed = 10f;

    public void Initialize(Vector2 target, Action onHit)
    {
        targetPosition = target;
        onHitCallback = onHit;
    }

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

    private void OnHitTarget()
    {
        // 충돌 효과 재생
        PlayHitEffect();

        // 콜백 호출
        onHitCallback?.Invoke();

        // 투사체 소멸
        Destroy(gameObject);
    }

    private void PlayHitEffect()
    {
        // 충돌 효과 재생 (파티클 등)
        // 예: Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
    }
}
