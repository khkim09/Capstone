using UnityEngine;
using UnityEngine.UI;

public class InGameBackground : MonoBehaviour
{
    private RawImage backgroundImage;

    private void Awake()
    {
        backgroundImage = GetComponent<RawImage>();
    }

    private void Start()
    {
        int randomIndex = Random.Range(0, 11); // 0 ~ 10 포함
        string path = $"Sprites/UI/Space Backgrounds/Space Background_{randomIndex}";
        Texture2D randomTexture = Resources.Load<Texture2D>(path);

        if (randomTexture != null)
            backgroundImage.texture = randomTexture;
        else
            Debug.LogWarning($"배경 텍스처를 불러올 수 없습니다: {path}");
    }
}
