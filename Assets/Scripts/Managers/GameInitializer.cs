using UnityEngine;

/// <summary>
/// 게임 시작 시 필요한 매니저들을 초기화하는 컴포넌트.
/// 중복 생성을 방지하며, 씬에 존재하지 않으면 프리팹을 통해 생성합니다.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    /// <summary>이벤트 매니저 프리팹.</summary>
    [SerializeField] private GameObject eventManagerPrefab;

    /// <summary>게임 매니저 프리팹.</summary>
    [SerializeField] private GameObject gameManagerPrefab;

    /// <summary>퀘스트 매니저 프리팹.</summary>
    [SerializeField] private GameObject questManagerPrefab;

    /// <summary>자원 매니저 프리팹.</summary>
    [SerializeField] private GameObject resourceManagerPrefab;

    /// <summary>UI 매니저 프리팹.</summary>
    [SerializeField] private GameObject uiManagerPrefab;

    /// <summary>게임 상태 매니저 프리팹.</summary>
    [SerializeField] private GameObject gameStateManagerPrefab;

    /// <summary>사기치 매니저 프리팹.</summary>
    [SerializeField] private GameObject moraleMangerPrefab;

    [SerializeField] private GameObject gameObjectFactoryPrefab;


    /// <summary>
    /// 게임 시작 시 필요한 매니저들을 생성합니다.
    /// </summary>
    private void Awake()
    {
        // 필요한 매니저들 초기화 (실제 게임에서는 Prefab 또는 Resources에서 로드)
        InstantiateIfNotExists<GameManager>(gameManagerPrefab);
        InstantiateIfNotExists<ResourceManager>(resourceManagerPrefab);
        InstantiateIfNotExists<EventManager>(eventManagerPrefab);
        InstantiateIfNotExists<QuestManager>(questManagerPrefab);
        InstantiateIfNotExists<UIManager>(uiManagerPrefab);
        InstantiateIfNotExists<GameStateManager>(gameStateManagerPrefab);
        InstantiateIfNotExists<MoraleManager>(moraleMangerPrefab);
        InstantiateIfNotExists<GameObjectFactory>(gameObjectFactoryPrefab);
    }

    /// <summary>
    /// 해당 타입의 오브젝트가 존재하지 않으면 프리팹에서 생성합니다.
    /// </summary>
    /// <typeparam name="T">찾을/생성할 매니저 타입.</typeparam>
    /// <param name="prefab">생성할 프리팹.</param>
    /// <returns>찾은 또는 생성된 컴포넌트.</returns>
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
