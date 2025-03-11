using System;
using System.Collections.Generic;
using UnityEngine;

public class ShipManager : MonoBehaviour
{
    // 선박 변경 이벤트
    public delegate void ShipSystemChangedHandler(int systemIndex, ShipSystem system);

    [SerializeField] private string currentShipId = "default";

    [SerializeField] private List<ShipSystem> shipSystems = new();
    [SerializeField] private List<string> unlockedShips = new();
    public static ShipManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 기본 시스템 초기화
            if (shipSystems.Count == 0) InitializeDefaultSystems();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event ShipSystemChangedHandler OnShipSystemChanged;

    private void InitializeDefaultSystems()
    {
        // 기본 선박 시스템 추가
        shipSystems.Add(new ShipSystem { name = "Engines", health = 100f });
        shipSystems.Add(new ShipSystem { name = "Shields", health = 100f });
        shipSystems.Add(new ShipSystem { name = "Weapons", health = 100f });
        shipSystems.Add(new ShipSystem { name = "Life Support", health = 100f });
        shipSystems.Add(new ShipSystem { name = "Sensors", health = 100f });
    }

    public void DamageSystem(string systemName, float amount)
    {
        var index = shipSystems.FindIndex(s => s.name == systemName);
        if (index >= 0)
        {
            var system = shipSystems[index];
            system.health = Mathf.Max(0, system.health - amount);

            // 시스템이 완전히 손상되면 비활성화
            if (system.health <= 0)
            {
                system.isActive = false;
                system.efficiency = 0f;

                // 치명적인 시스템 손상에 따른 효과
                HandleSystemFailure(systemName);
            }
            else
            {
                // 손상 정도에 따른 효율성 계산
                system.efficiency = system.health / system.maxHealth;
            }

            OnShipSystemChanged?.Invoke(index, system);
        }
    }

    public void RepairSystem(string systemName, float amount)
    {
        var index = shipSystems.FindIndex(s => s.name == systemName);
        if (index >= 0)
        {
            var system = shipSystems[index];

            var prevHealth = system.health;
            system.health = Mathf.Min(system.maxHealth, system.health + amount);

            // 시스템이 복구되면 재활성화
            if (prevHealth <= 0 && system.health > 0) system.isActive = true;

            // 효율성 업데이트
            system.efficiency = system.health / system.maxHealth;

            OnShipSystemChanged?.Invoke(index, system);
        }
    }

    private void HandleSystemFailure(string systemName)
    {
        // 특정 시스템 손상에 따른 특별 효과
        switch (systemName)
        {
            case "Life Support":
                // 생명 유지 장치 손상 시 승무원 건강 영향
                Debug.Log("Life Support system failure! Crew is in danger.");
                CrewManager.Instance.ApplyEffectToAllCrew(new CrewEffect
                {
                    effectType = CrewEffectType.Damage, healthChange = -10f
                });
                break;

            case "Engines":
                // 엔진 손상 시 이동 불가
                Debug.Log("Engine failure! Ship cannot move until repaired.");
                break;

            case "Shields":
                // 방어막 손상 시 손상 증가
                Debug.Log("Shield failure! Ship is vulnerable to damage.");
                break;

            case "Sensors":
                // 센서 손상 시 이벤트 선택지 제한
                Debug.Log("Sensor failure! Limited information available.");
                break;
        }
    }

    public void UpgradeSystem(string systemName)
    {
        var index = shipSystems.FindIndex(s => s.name == systemName);
        if (index >= 0)
        {
            var system = shipSystems[index];

            // 업그레이드에 필요한 자원 확인
            var scrapCost = system.level * 10;

            if (ResourceManager.Instance.GetResource(ResourceType.Scrap) >= scrapCost)
            {
                // 자원 소비
                ResourceManager.Instance.ChangeResource(ResourceType.Scrap, -scrapCost);

                // 시스템 업그레이드
                system.level++;
                system.maxHealth += 20f;
                system.health = system.maxHealth; // 업그레이드 시 완전 수리
                system.efficiency = 1.0f;

                OnShipSystemChanged?.Invoke(index, system);

                Debug.Log($"{systemName} upgraded to level {system.level}!");
            }
            else
            {
                Debug.Log($"Not enough scrap for upgrade! Need {scrapCost}.");
            }
        }
    }

    public void UnlockShip(string shipId)
    {
        if (!unlockedShips.Contains(shipId))
        {
            unlockedShips.Add(shipId);
            Debug.Log($"New ship unlocked: {shipId}");
        }
    }

    public void SwitchShip(string shipId)
    {
        if (unlockedShips.Contains(shipId))
        {
            currentShipId = shipId;

            // 선박 변경에 따른 시스템 재설정
            ResetShipSystems();

            Debug.Log($"Switched to ship: {shipId}");
        }
    }

    private void ResetShipSystems()
    {
        // 새 선박에 맞게 시스템 초기화
        // 실제 구현에서는 선박 데이터베이스에서 정보 로드
        shipSystems.Clear();
        InitializeDefaultSystems();

        // 선박 유형에 따라 특화 시스템 추가
        switch (currentShipId)
        {
            case "combat":
                shipSystems.Add(new ShipSystem { name = "Advanced Weapons", health = 100f });
                break;

            case "explorer":
                shipSystems.Add(new ShipSystem { name = "Long Range Scanners", health = 100f });
                break;

            case "cargo":
                shipSystems.Add(new ShipSystem { name = "Cargo Hold", health = 100f });
                break;
        }
    }

    public ShipSystem GetSystem(string systemName)
    {
        return shipSystems.Find(s => s.name == systemName);
    }

    public List<ShipSystem> GetAllSystems()
    {
        return shipSystems;
    }

    public List<string> GetUnlockedShips()
    {
        return unlockedShips;
    }
}
