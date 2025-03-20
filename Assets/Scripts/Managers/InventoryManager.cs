using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class InventoryManager : MonoBehaviour
{
    // 인벤토리 변경 이벤트
    public delegate void InventoryChangedHandler(InventoryItem item);

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Crafting,
        Special
    }

    [SerializeField] private List<InventoryItem> items = new();
    private readonly int maxInventorySize = 20;
    public static InventoryManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event InventoryChangedHandler OnItemAdded;
    public event InventoryChangedHandler OnItemRemoved;
    public event InventoryChangedHandler OnItemUsed;

    public bool AddItem(string itemId)
    {
        // 실제 구현에서는 아이템 데이터베이스에서 ID로 아이템 정보 조회
        InventoryItem newItem = new()
        {
            id = itemId,
            name = "Item " + itemId,
            description = "A sample item",
            type = ItemType.Consumable,
            quantity = 1
        };

        return AddItem(newItem);
    }

    public bool AddItem(InventoryItem item)
    {
        // 이미 같은 아이템이 있는지 확인
        InventoryItem existingItem = items.Find(i => i.id == item.id);

        if (existingItem != null)
            // 중첩 가능한 아이템인 경우
            if (existingItem.type == ItemType.Consumable || existingItem.type == ItemType.Crafting)
            {
                existingItem.quantity += item.quantity;
                OnItemAdded?.Invoke(existingItem);
                return true;
            }

        // 새 아이템 추가
        if (items.Count < maxInventorySize)
        {
            items.Add(item);
            OnItemAdded?.Invoke(item);
            return true;
        }

        // 인벤토리가 가득 참
        Debug.Log("Inventory is full!");
        return false;
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        InventoryItem item = items.Find(i => i.id == itemId);

        if (item != null)
        {
            if (item.quantity > quantity)
            {
                item.quantity -= quantity;
                OnItemRemoved?.Invoke(item);
                return true;
            }

            if (item.quantity == quantity)
            {
                items.Remove(item);
                OnItemRemoved?.Invoke(item);
                return true;
            }
        }

        // 제거할 아이템이 충분하지 않음
        return false;
    }

    public bool UseItem(string itemId)
    {
        InventoryItem item = items.Find(i => i.id == itemId);

        if (item != null)
            // 아이템 유형에 따른 사용 효과
            switch (item.type)
            {
                case ItemType.Consumable:
                    // 사용 효과 처리
                    ApplyItemEffects(item);

                    // 수량 감소
                    if (item.quantity > 1)
                        item.quantity--;
                    else
                        items.Remove(item);

                    OnItemUsed?.Invoke(item);
                    return true;

                case ItemType.Weapon:
                case ItemType.Armor:
                    // 장비 아이템은 장착/해제
                    ToggleEquipItem(item);
                    return true;
            }

        return false;
    }

    private void ApplyItemEffects(InventoryItem item)
    {
        // 실제 게임에서는 아이템 효과 데이터베이스를 참조
        if (item.id == "medkit")
        {
            // 특정 상태의 크루원 회복
            List<int> sickCrewIndices = new();

            for (int i = 0; i < DefaultCrewManagerScript.Instance.GetAliveCrewCount(); i++)
            {
                CrewMember crewMember = DefaultCrewManagerScript.Instance.GetCrewMember(i);
                if (crewMember != null && crewMember.status == CrewStatus.Sick) sickCrewIndices.Add(i);
            }

            if (sickCrewIndices.Count > 0)
            {
                int targetIndex = sickCrewIndices[Random.Range(0, sickCrewIndices.Count)];

                DefaultCrewManagerScript.Instance.ApplyCrewEffect(new CrewEffect
                {
                    effectType = CrewEffectType.StatusChange,
                    targetCrewIndex = targetIndex,
                    statusEffect = CrewStatus.Normal
                });

                DefaultCrewManagerScript.Instance.ApplyCrewEffect(new CrewEffect
                {
                    effectType = CrewEffectType.Heal, targetCrewIndex = targetIndex, healthChange = 20f
                });
            }
        }
        else if (item.id == "repairkit")
        {
            // 선박 수리 효과
            ResourceManager.Instance.ChangeResource(ResourceType.COMA, 5);
        }
    }

    private void ToggleEquipItem(InventoryItem item)
    {
        if (item.isEquippable)
        {
            if (item.isEquipped)
            {
                // 장비 해제
                item.isEquipped = false;
                // 캐릭터 스탯 업데이트 등
            }
            else
            {
                // 같은 유형의 장착된 아이템 해제
                foreach (InventoryItem equippedItem in items)
                    if (equippedItem.type == item.type && equippedItem.isEquipped)
                        equippedItem.isEquipped = false;

                // 새 아이템 장착
                item.isEquipped = true;
                // 캐릭터 스탯 업데이트 등
            }

            OnItemUsed?.Invoke(item);
        }
    }

    public List<InventoryItem> GetAllItems()
    {
        return items;
    }

    public List<InventoryItem> GetItemsByType(ItemType type)
    {
        return items.FindAll(i => i.type == type);
    }

    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string name;
        public string description;
        public ItemType type;
        public int quantity;
        public bool isEquippable;
        public bool isEquipped;
        public Dictionary<string, float> stats = new();
    }
}
