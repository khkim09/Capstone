using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameObject crewManagerPrefab;
    [SerializeField] private GameObject eventManagerPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject inventoryManagerPrefab;
    [SerializeField] private GameObject questManagerPrefab;
    [SerializeField] private GameObject resourceManagerPrefab;
    [SerializeField] private GameObject shipManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;
    [SerializeField] private GameObject gameStateManagerPrefab;

    private void Awake()
    {
        // 필요한 매니저들 초기화 (실제 게임에서는 Prefab 또는 Resources에서 로드)
        InstantiateIfNotExists<GameManager>(gameManagerPrefab);
        InstantiateIfNotExists<ResourceManager>(resourceManagerPrefab);
        InstantiateIfNotExists<CrewManager>(crewManagerPrefab);
        InstantiateIfNotExists<EventManager>(eventManagerPrefab);
        InstantiateIfNotExists<QuestManager>(questManagerPrefab);
        InstantiateIfNotExists<InventoryManager>(inventoryManagerPrefab);
        InstantiateIfNotExists<UIManager>(uiManagerPrefab);
        InstantiateIfNotExists<GameStateManager>(gameStateManagerPrefab);
    }

    private T InstantiateIfNotExists<T>(GameObject prefab) where T : MonoBehaviour
    {
        T existing = FindAnyObjectByType<T>();
        if (existing == null && prefab != null)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = typeof(T).Name;
            return obj.GetComponent<T>();
        }

        return existing;
    }
}
