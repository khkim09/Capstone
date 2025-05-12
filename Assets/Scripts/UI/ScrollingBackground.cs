using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private bool scrollHorizontal = true;
    [SerializeField] private bool scrollVertical = false;
    [SerializeField] private bool reverseDirection = false;
    [SerializeField] private bool randomDirection = false; // ✅ 랜덤 설정
    [SerializeField] private Vector2 tiling = new(2, 1); // 가로 2배, 세로 1배 타일링

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();

        if (randomDirection)
        {
            int directionIndex = Random.Range(0, 8); // 0~7

            switch (directionIndex)
            {
                case 0: // ➡️ 오른쪽
                    scrollHorizontal = true;
                    scrollVertical = false;
                    reverseDirection = false;
                    break;
                case 1: // ⬅️ 왼쪽
                    scrollHorizontal = true;
                    scrollVertical = false;
                    reverseDirection = true;
                    break;
                case 2: // ⬇️ 아래
                    scrollHorizontal = false;
                    scrollVertical = true;
                    reverseDirection = false;
                    break;
                case 3: // ⬆️ 위
                    scrollHorizontal = false;
                    scrollVertical = true;
                    reverseDirection = true;
                    break;
                case 4: // ↘️ 오른아래
                    scrollHorizontal = true;
                    scrollVertical = true;
                    reverseDirection = false;
                    break;
                case 5: // ↙️ 왼쪽아래
                    scrollHorizontal = true;
                    scrollVertical = true;
                    reverseDirection = true;
                    break;
                case 6: // ↖️ 왼쪽위 (양쪽 마이너스)
                    scrollHorizontal = true;
                    scrollVertical = true;
                    reverseDirection = true;
                    break;
                case 7: // ↗️ 오른쪽위 (가로 +, 세로 -)
                    scrollHorizontal = true;
                    scrollVertical = true;
                    reverseDirection = false;
                    break;
            }
        }


        // 텍스처 타일링 설정
        if (rawImage != null && rawImage.texture != null)
        {
            rawImage.texture.wrapMode = TextureWrapMode.Repeat;
            rawImage.uvRect = new Rect(0, 0, tiling.x, tiling.y);
        }
    }

    private void Update()
    {
        if (rawImage == null) return;

        Rect uvRect = rawImage.uvRect;
        Vector2 uvPosition = uvRect.position;

        float directionH = reverseDirection ? -1 : 1;
        float directionV = reverseDirection ? 1 : -1;

        if (scrollHorizontal) uvPosition.x += directionH * scrollSpeed * Time.deltaTime;
        if (scrollVertical) uvPosition.y += directionV * scrollSpeed * Time.deltaTime;

        rawImage.uvRect = new Rect(uvPosition, uvRect.size);
    }
}
