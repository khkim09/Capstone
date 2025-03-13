using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject room;
    public float cellSize = 1.0f;

    // grid 영역 설정
    public Vector2 gridMin = new Vector2(-5.0f, 4.0f);
    public Vector2 gridMax = new Vector2(5.0f, 4.0f);

    // 배치하는 room 담을 container
    public Transform roomsContainer;

    void Update()
    {
        Vector3 mousePos;
        Vector3 worldPos;
        worldPos.z = 0.0f;

        float snappedX;
        float snappedY;
        Vector3 gridPos;

        Collider2D hitCollider;

        // room 생성 (좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
            worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // 각 셀 중앙 배치
            snappedX = Mathf.Floor(worldPos.x / cellSize) * cellSize + cellSize / 2;
            snappedY = Mathf.Floor(worldPos.y / cellSize) * cellSize + cellSize / 2;
            gridPos = new Vector3(snappedX, snappedY, 0);

            // 설치 가능 범위 (grid guideline 내부)
            if (gridMin.x <= gridPos.x && gridPos.x <= gridMax.x && gridMin.y <= gridPos.y && gridPos.y <= gridMax.y)
            {
                // 중복 설치 불가
                hitCollider = Physics2D.OverlapPoint(gridPos);
                if (hitCollider == null)
                    if (roomsContainer)
                        Instantiate(room, gridPos, Quaternion.identity).transform.parent = roomsContainer;
            }
        }

        // room 삭제 (우클릭)
        if (Input.GetMouseButtonDown(1))
        {
            mousePos = Input.mousePosition;
            worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            // 각 셀 중앙 배치
            snappedX = Mathf.Floor(worldPos.x / cellSize) * cellSize + cellSize / 2;
            snappedY = Mathf.Floor(worldPos.y / cellSize) * cellSize + cellSize / 2;
            gridPos = new Vector3(snappedX, snappedY, 0);

            // 설치된 object 검사
            hitCollider = Physics2D.OverlapPoint(gridPos);
            if (hitCollider != null)
            {
                Destroy(hitCollider.gameObject);
            }
        }
    }

    // room 모두 삭제
    public void ClearGrid()
    {
        if (roomsContainer)
            foreach (Transform rooms in roomsContainer)
                Destroy(rooms.gameObject);

    }
}
