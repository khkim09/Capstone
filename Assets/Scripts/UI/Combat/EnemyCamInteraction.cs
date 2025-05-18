using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyCamInteraction : MonoBehaviour, IPointerClickHandler, IDragHandler,IPointerDownHandler
{
    public Camera enemyCam;
    public RectTransform displayRect;
    public RenderTexture enemyCamTexture;
    private bool displayOn = true;

    public void PanelClicked()
    {
        if (displayOn)
        {
            displayOn = false;
            gameObject.SetActive(false);
        }
        else
        {
            displayOn = true;
            gameObject.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2? worldPoint = EnemyCamToWorldPos(eventData);
        if (worldPoint != null)
        {
            //좌클릭
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                RTSSelectionManager.Instance.SelectSingleCrew(worldPoint);

            }
            //우클릭
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                RTSSelectionManager.Instance.CleanUpSelectedCrew();

                Ray ray = enemyCam.ScreenPointToRay(worldPoint.Value);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
                if (hit.collider == null)
                    return;
                Room targetRoom = hit.collider.GetComponent<Room>();

                Debug.Log(targetRoom.name);

                RTSSelectionManager.Instance.IssueMoveCommand(targetRoom);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {

        Vector2? curPos=EnemyCamToWorldPos(eventData).Value;

        if (curPos.HasValue)
        {
            RTSSelectionManager.Instance.SelectMultipleCrew(dragStartPos,curPos.Value);
        }
    }

    Vector2 dragStartPos;
    public void OnPointerDown(PointerEventData eventData)
    {
        dragStartPos=EnemyCamToWorldPos(eventData).Value;
    }

    private Vector2 RectPointToUV(Vector2 point, RectTransform rect)
    {
        Rect r = rect.rect;
        return new Vector2((point.x - r.x) / r.width, (point.y - r.y) / r.height);
    }

    private Vector2? EnemyCamToWorldPos(PointerEventData eventData)
    {
        Vector2 localPoint;
        Vector2 worldPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(displayRect, eventData.position,
                eventData.pressEventCamera, out localPoint))
        {
            Vector2 uv = RectPointToUV(localPoint,displayRect);
            worldPoint=new Vector2(
                uv.x*enemyCamTexture.width,
                uv.y*enemyCamTexture.height);
            return worldPoint;
        }
        return null;
    }
}
