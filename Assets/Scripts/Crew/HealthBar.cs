using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Components")] [SerializeField]
    private Image healthBarFill;

    [SerializeField] private Canvas healthBarCanvas;

    [Header("Settings")] [SerializeField] private Vector3 offset = new(0, 0.8f, 0); // 선원 머리 위 위치

    // 현재 체력과 최대 체력은 크루에서 참조
    private float currentHealth;
    private float maxHealth;

    // 따라다닐 타겟 (선원)
    private Transform target;
    private Camera mainCamera;

    // 선원 참조
    private CrewBase crewBase;

    private void Start()
    {
        // 타겟 설정 (직접 부모로 설정 - 선원 오브젝트)
        target = transform.parent;

        // 선원 컴포넌트 찾기
        if (target != null)
        {
            crewBase = target.GetComponent<CrewBase>();
            if (crewBase == null)
            {
                Debug.LogError($"HealthBar: {target.name}에서 CrewBase 컴포넌트를 찾을 수 없습니다!");
                return;
            }

            // 선원의 체력 정보 초기화
            InitializeHealthFromCrew();
        }

        // 메인 카메라 찾기
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindObjectOfType<Camera>();

        // Canvas 설정
        SetupCanvas();

        // 초기 체력바 업데이트
        UpdateHealthBarFromCrew();
    }

    private void InitializeHealthFromCrew()
    {
        if (crewBase != null)
        {
            maxHealth = crewBase.maxHealth;
            currentHealth = crewBase.health;
        }
    }

    private void LateUpdate()
    {
        // 체력바를 선원 머리 위에 위치시키기
        if (target != null)
        {
            // Canvas 자체의 위치를 선원 위치 + offset으로 설정
            Vector3 worldPosition = target.position + offset;
            healthBarCanvas.transform.position = worldPosition;

            // 카메라가 있다면 카메라를 바라보도록 회전
            if (mainCamera != null)
            {
                Vector3 lookDirection = mainCamera.transform.position - healthBarCanvas.transform.position;
                healthBarCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        // 선원의 체력 정보와 동기화
        UpdateHealthBarFromCrew();
    }

    private void SetupCanvas()
    {
        if (healthBarCanvas == null)
        {
            healthBarCanvas = GetComponent<Canvas>();
            if (healthBarCanvas == null) healthBarCanvas = gameObject.AddComponent<Canvas>();
        }

        // World Space Canvas 설정
        healthBarCanvas.renderMode = RenderMode.WorldSpace;

        // Canvas의 로컬 위치를 (0,0,0)으로 재설정
        healthBarCanvas.transform.localPosition = Vector3.zero;

        // RectTransform 설정
        RectTransform canvasRect = healthBarCanvas.GetComponent<RectTransform>();
        if (canvasRect != null)
        {
            // Canvas 크기 설정 (픽셀 단위)
            canvasRect.sizeDelta = new Vector2(150, 30);

            // 스케일 설정 - 이 값을 조정해서 크기를 맞추세요
            canvasRect.localScale = Vector3.one * 0.01f; // 크기 조정

            // 앵커를 중앙으로 설정
            canvasRect.anchorMin = Vector2.one * 0.5f;
            canvasRect.anchorMax = Vector2.one * 0.5f;
            canvasRect.anchoredPosition = Vector2.zero;
        }

        // Sorting Order 설정
        healthBarCanvas.sortingOrder = 200;

        // CanvasScaler 제거 또는 비활성화 (World Space에서는 필요 없음)
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null) Destroy(scaler);
    }

    // 선원의 현재 체력 정보를 체력바에 반영
    private void UpdateHealthBarFromCrew()
    {
        if (crewBase == null) return;

        // 최대 체력이 변경된 경우 업데이트
        if (maxHealth != crewBase.maxHealth) maxHealth = crewBase.maxHealth;

        // 현재 체력이 변경된 경우 업데이트
        if (currentHealth != crewBase.health)
        {
            currentHealth = crewBase.health;
            UpdateHealthBarVisual();
        }
    }

    // 외부에서 직접 호출할 수도 있는 메서드 (즉시 업데이트용)
    public void ForceUpdateFromCrew()
    {
        if (crewBase != null)
        {
            maxHealth = crewBase.maxHealth;
            currentHealth = crewBase.health;
            UpdateHealthBarVisual();
        }
    }

    // 외부에서 체력 업데이트할 때 호출 (하위 호환성을 위해 유지)
    public void UpdateHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHealthBarVisual();
    }

    // 최대 체력 변경 (장비 변경 등) (하위 호환성을 위해 유지)
    public void UpdateMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthBarVisual();
    }

    // 체력 감소 (하위 호환성을 위해 유지, 실제로는 CrewBase에서 처리)
    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBarVisual();
    }

    // 체력바 UI 업데이트
    private void UpdateHealthBarVisual()
    {
        if (healthBarFill != null && maxHealth > 0)
        {
            float healthPercentage = currentHealth / maxHealth;
            healthBarFill.fillAmount = healthPercentage;

            // 체력에 따른 색상 변경
            UpdateHealthBarColor(healthPercentage);
        }
    }

    // 체력에 따른 색상 변경
    private void UpdateHealthBarColor(float healthPercentage)
    {
    }
}
