using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private bool scrollHorizontal = true;
    [SerializeField] private bool scrollVertical = false;
    [SerializeField] private bool reverseDirection = false;
    [SerializeField] private Vector2 tiling = new(2, 1); // 가로 2배, 세로 1배 타일링

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();

        // 텍스처 타일링 설정
        if (rawImage != null && rawImage.texture != null)
        {
            // 텍스처의 Wrap Mode가 Repeat인지 확인 (런타임에서 변경 가능)
            rawImage.texture.wrapMode = TextureWrapMode.Repeat;

            // 초기 타일링 설정
            rawImage.uvRect = new Rect(0, 0, tiling.x, tiling.y);
        }
    }

    private void Update()
    {
        if (rawImage == null) return;

        // 현재 UV Rect 가져오기
        Rect uvRect = rawImage.uvRect;
        Vector2 uvPosition = uvRect.position;

        // 스크롤 방향 설정 (기본은 왼쪽으로 이동)
        float directionH = reverseDirection ? -1 : 1;
        float directionV = reverseDirection ? 1 : -1;

        // UV 좌표 업데이트
        if (scrollHorizontal) uvPosition.x += directionH * scrollSpeed * Time.deltaTime;

        if (scrollVertical) uvPosition.y += directionV * scrollSpeed * Time.deltaTime;

        // 새 UV Rect 적용 (크기는 그대로 유지)
        rawImage.uvRect = new Rect(uvPosition, uvRect.size);
    }
}
