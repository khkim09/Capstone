using UnityEngine;
using UnityEngine.EventSystems;

public class RoomDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject previewPrefab;
    public Vector2Int roomSize = new Vector2Int(2, 2);

    private GameObject previewInstance;

    public void OnBeginDrag(PointerEventData eventData)
    {
        previewInstance = Instantiate(previewPrefab);
        SetAlpha(previewInstance, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        previewInstance.transform.position = ShipGridManager.Instance.GridToWorldPosition(gridPos)
            + new Vector2((roomSize.x - 1) * 0.5f, (roomSize.y - 1) * 0.5f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        GridPlacer.Instance.PlaceRoom(gridPos, roomSize);
        Destroy(previewInstance);
    }

    private void SetAlpha(GameObject obj, float alpha)
    {
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }
}
