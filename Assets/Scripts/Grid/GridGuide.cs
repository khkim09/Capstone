using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))] // 2D 정렬

public class GridGuide : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1.0f;
    public Vector2 gridMin = new Vector2(-5.0f, -4.0f);
    public Vector2 gridMax = new Vector2(5.0f, 4.0f);

    [Header("Line Renderer Settings")]
    public Material lineMaterial;
    public float lineWidth = 0.03f;
    public int sortingOrder = 100;

    void Start()
    {
        // 수직선 (세로)
        for (float x = gridMin.x; x <= gridMax.x; x += cellSize)
        {
            CreateLine(
                new Vector3(x, gridMin.y, 0),
                new Vector3(x, gridMax.y, 0),
                "LineX_" + x
            );
        }

        // 수평선 (가로)
        for (float y = gridMin.y; y <= gridMax.y; y += cellSize)
        {
            CreateLine(
                new Vector3(gridMin.x, y, 0),
                new Vector3(gridMax.x, y, 0),
                "LineY_" + y
            );
        }
    }

    private void CreateLine(Vector3 startPos, Vector3 endPos, string objName)
    {
        GameObject lineObj = new GameObject(objName);
        lineObj.transform.parent = transform;

        // line 그리기
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.positionCount = 2;
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        // 2D 정렬을 위해서 Sorting Layer/Order 설정 (스프라이트 위에 그리려면)
        lr.sortingOrder = sortingOrder;
    }
}
