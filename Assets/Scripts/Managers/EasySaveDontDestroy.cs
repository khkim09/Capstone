using UnityEngine;

public class EasySaveDontDestroy : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static EasySaveDontDestroy Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 패턴 구현
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
}
