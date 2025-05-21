using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyCamInteraction : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
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


    Vector2 dragStartPos;
    Vector2 dragEndPos;
    public void OnPointerDown(PointerEventData eventData)
    {
        dragStartPos=MainScreenPointToEnemyCamScreenPoint(eventData).Value;

        //우클릭
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RTSSelectionManager.Instance.CleanUpSelectedCrew();

            Ray ray = enemyCam.ScreenPointToRay(MainScreenPointToEnemyCamScreenPoint(eventData).Value);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
            if (hit.collider == null)
                return;
            Room targetRoom = hit.collider.GetComponent<Room>();

            Debug.Log(targetRoom.name);

            RTSSelectionManager.Instance.IssueMoveCommand(targetRoom);
        }
        //좌클릭
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            RTSSelectionManager.Instance.dragStartPos=Input.mousePosition;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            RTSSelectionManager.Instance.DeselectAll();
            dragEndPos=MainScreenPointToEnemyCamScreenPoint(eventData).Value;

            float dragDistance = Vector2.Distance(dragStartPos, dragEndPos);

            //단일 유닛 선택
            if(dragDistance<RTSSelectionManager.Instance.clickThreshold)
            {
                Ray ray = enemyCam.ScreenPointToRay(MainScreenPointToEnemyCamScreenPoint(eventData).Value);
                RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
                if (hit.collider == null)
                    return;
                CrewMember crew = hit.collider.GetComponent<CrewMember>();


                // 아군만 선택 가능
                if (crew != null /* && crew.isPlayerControlled*/)
                {
                    RTSSelectionManager.Instance.selectedCrew.Add(crew);
                    RTSSelectionManager.Instance.SetOutline(crew, true);
                    crew.originPosTile = crew.GetCurrentTile();
                }
            }
            //다중 유닛 선택
            else
            {
                RTSSelectionManager.Instance.selectedCrew.Clear();
                Rect selectionRect = GetWorldRect();

                CrewMember[] allCrew = FindObjectsByType<CrewMember>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (CrewMember crew in allCrew)
                {
                    if (selectionRect.Contains(crew.transform.position))
                    {
                        RTSSelectionManager.Instance.selectedCrew.Add(crew);
                        RTSSelectionManager.Instance.SetOutline(crew, true);
                    }
                    else
                    {
                        RTSSelectionManager.Instance.SetOutline(crew,false);
                    }
                }

                foreach (CrewMember crew in RTSSelectionManager.Instance.selectedCrew)
                {
                    crew.originPosTile = crew.GetCurrentTile();
                }
            }
        }
    }

    private Vector2 RectPointToUV(Vector2 point, RectTransform rect)
    {
        Rect r = rect.rect;
        return new Vector2((point.x - r.x) / r.width, (point.y - r.y) / r.height);
    }

    private Vector2? MainScreenPointToEnemyCamScreenPoint(PointerEventData eventData)
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

    private Rect GetWorldRect()
    {
        Vector2 worldDragStartPos = enemyCam.ScreenToWorldPoint(dragStartPos);
        Vector2 worldDragEndPos=enemyCam.ScreenToWorldPoint(dragEndPos);
        Vector2 LeftBottom = Vector2.Min(worldDragStartPos, worldDragEndPos);
        Vector2 RightTop = Vector2.Max(worldDragStartPos, worldDragEndPos);
        float width = RightTop.x - LeftBottom.x;
        float height = RightTop.y - LeftBottom.y;

        Rect selectionRect = new Rect(LeftBottom.x, LeftBottom.y, width, height);
        return selectionRect;
    }

    private void CalculateBounds()
    {
        Ship enemyShip = GameObject.Find("EnemyShip").GetComponent<Ship>();
        //todo:EnemyShip으로 변경 필요
        Renderer[] renderers = enemyShip.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        enemyBound = bounds;
    }

    private Bounds enemyBound;

    public float enemyCamPadding = 1.1f;
    public void EnemyCamAim()
    {
        CalculateBounds();

        enemyCam.transform.position = new Vector3(enemyBound.center.x, enemyBound.center.y,0);

        float camHeight = enemyBound.size.y * 0.5f * enemyCamPadding;
        float camWidth = enemyBound.size.x * 0.5f * enemyCamPadding / enemyCam.aspect;

        enemyCam.orthographicSize = Mathf.Max(camHeight, camWidth);
    }

    void Start()
    {
        enemyCam=GameObject.Find("EnemyCam").GetComponent<Camera>();
        displayRect=transform.GetChild(0).GetComponent<RectTransform>();
        enemyCamTexture = enemyCam.targetTexture;

        //EnemyCamAim();
        Debug.Log("Camera forward: " + enemyCam.transform.forward);

    }

    void Update()
    {
        //EnemyCamAim(CalculateBounds());
    }
}
