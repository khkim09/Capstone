using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    [SerializeField] public GameObject objectToPlace;
    [SerializeField] public float cellSize = 1.0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0.0f;

            // 각 셀 중앙 배치
            float snappedX = Mathf.Floor(worldPos.x / cellSize) * cellSize + cellSize / 2;
            float snappedY = Mathf.Floor(worldPos.y / cellSize) * cellSize + cellSize / 2;
            Vector3 gridPos = new Vector3(snappedX, snappedY, 0);

            Instantiate(objectToPlace, gridPos, Quaternion.identity);
        }

        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            worldPos.z = 0.0f;

            // 각 셀 중앙 배치
            float snappedX = Mathf.Floor(worldPos.x / cellSize) * cellSize + cellSize / 2;
            float snappedY = Mathf.Floor(worldPos.y / cellSize) * cellSize + cellSize / 2;
            Vector3 gridPos = new Vector3(snappedX, snappedY, 0);

            // 설치된 object 검사
            Collider2D hitCollider = Physics2D.OverlapPoint(gridPos);
            if (hitCollider != null)
            {
                Destroy(hitCollider.gameObject);
            }
        }
    }
}
