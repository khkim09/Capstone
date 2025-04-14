using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

/// <summary>
/// 선원 단일 선택, 다중 선택 관리 Manager
/// </summary>
public class RTSSelectionManager : MonoBehaviour
{
    /// <summary>
    /// 드래그 선택 시 표시할 텍스처입니다.
    /// </summary>
    public Texture2D selectionTexture;

    /// <summary>
    /// 이동 가능 지면으로 인식할 레이어입니다.
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// 드래그 시작 위치입니다.
    /// </summary>
    private Vector2 dragStartPos;

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 단일 클릭 임계값
    /// </summary>
    private float clickThreshold = 10f;

    /// <summary>
    /// 선택된 선원 리스트
    /// </summary>
    public List<CrewMember> selectedCrew = new List<CrewMember>();

    /// <summary>
    /// 이동 후 전투를 위한 공격 범위 (추후 조정)
    /// </summary>
    public float attackRange = 2.0f;

    /// <summary>
    /// 임시 장비 공격력 변수
    /// </summary>
    public float tmpEquipmentAttack = 0.0f;

    /// <summary>
    /// 이동 시 Crew 간 공간
    /// </summary>
    public float spacing = 0.7f;

    /// <summary>
    /// 이동 명령 시 움직임 속도 (수정 필요)
    /// </summary>
    public float speed = 10.0f;

    /// <summary>
    /// 좌클릭 감지 - 다중 선택 시 영역 표시
    /// 우클릭 감지 - 선택한 선원 이동 명령
    /// </summary>
    private void Update()
    {
        bool isMainUI = IsMainUIActive();

        // 왼쪽 마우스 버튼 눌림: 선택 시작
        if (isMainUI && Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        // 왼쪽 마우스 버튼 뗌: 선택 완료
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                isDragging = false;

                float distance = Vector2.Distance(dragStartPos, Input.mousePosition);
                if (distance < clickThreshold)
                    SelectSingleCrew();
                else
                    SelectMultipleCrew();
            }
        }

        // 오른쪽 마우스 버튼 클릭: 이동 명령 발동
        if (isMainUI && Input.GetMouseButtonDown(1))
        {
            IssueMoveCommand();
        }
    }

    /// <summary>
    /// MainUI에서만 RTS기능이 동작하도록 제한하는 함수
    /// </summary>
    /// <returns></returns>
    private bool IsMainUIActive()
    {
        return CrewUIHandler.Instance != null && CrewUIHandler.Instance.mainUIScreen != null && CrewUIHandler.Instance.mainUIScreen.activeSelf;
    }


    /// <summary>
    /// OnGUI를 이용하여 드래그 영역의 사각형 표시
    /// </summary>
    private void OnGUI()
    {
        if (isDragging && IsMainUIActive())
        {
            Rect rect = GetScreenRect(dragStartPos, Input.mousePosition);
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    /// <summary>
    /// 두 스크린 좌표로 사각형 영역 계산 (y좌표 보정 포함)
    /// </summary>
    /// <param name="screenPosition1"></param>
    /// <param name="screenPosition2"></param>
    /// <returns></returns>
    private Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
    {
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        Vector2 topLeft = Vector2.Min(screenPosition1, screenPosition2);
        Vector2 bottomRight = Vector2.Max(screenPosition1, screenPosition2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    /// <summary>
    /// 사각형 내부 채우기
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="color"></param>
    private void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, selectionTexture);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 사각형 테두리 그리기
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="thickness"></param>
    /// <param name="color"></param>
    private void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        GUI.color = color;
        // 위쪽 테두리
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), selectionTexture);
        // 왼쪽 테두리
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), selectionTexture);
        // 오른쪽 테두리
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), selectionTexture);
        // 아래쪽 테두리
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), selectionTexture);
        GUI.color = Color.white;
    }

    /// <summary>
    /// 선택한 선원 모두 해제
    /// </summary>
    private void DeselectAll()
    {
        selectedCrew.Clear();
        CrewMember[] allCrew = GameObject.FindObjectsByType<CrewMember>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (CrewMember crew in allCrew)
            crew.GetComponent<Renderer>().material.color = Color.white;
    }

    /// <summary>
    /// 단일 선원 선택
    /// </summary>
    public void SelectSingleCrew()
    {
        DeselectAll();

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            CrewMember crew = hit.collider.GetComponent<CrewMember>();
            if (crew != null)
            {
                selectedCrew.Add(crew);
                crew.GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    /// <summary>
    /// 영역 내 선원 다중 선택
    /// </summary>
    public void SelectMultipleCrew()
    {
        // 선택 리스트 초기화
        selectedCrew.Clear();
        Rect selectionRect = GetScreenRect(dragStartPos, Input.mousePosition);

        // 모든 CrewMember를 찾아서 선택 영역 안에 있는지 확인
        CrewMember[] allCrew = GameObject.FindObjectsByType<CrewMember>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (CrewMember crew in allCrew)
        {
            // 월드 좌표를 스크린 좌표로 변환 (y좌표 보정)
            Vector3 screenPos = Camera.main.WorldToScreenPoint(crew.transform.position);
            screenPos.y = Screen.height - screenPos.y;

            // 클릭한 경우도 드래그 영역의 크기가 작으면 선택
            if (selectionRect.Contains(screenPos, true))
            {
                selectedCrew.Add(crew);
                // 선택된 표시 (예: 색상 변경)
                crew.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                // 선택되지 않은 경우 원래 색상으로
                crew.GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }

    /// <summary>
    /// 클릭한 타일의 중앙 반환
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    private Vector3 GetTileCenterFromWorld(Vector3 worldPos)
    {
        float tileSize = 1.0f;
        int gridX = Mathf.FloorToInt(worldPos.x / tileSize);
        int gridY = Mathf.FloorToInt(worldPos.y / tileSize);

        float centerX = gridX * tileSize + tileSize / 2f;
        float centerY = gridY * tileSize + tileSize / 2f;

        return new Vector3(centerX, centerY, 0);
    }

    /// <summary>
    /// 직선적인 움직임 - 타일 중앙만 거치도록
    /// (astar Alg 추가 필요)
    /// (방, 복도 등 이동 가능한 곳만을 통해서 이동, 각 칸에 1명 이상 있을 시 우회)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private List<Vector3> GetPathViaRoomCenters(Vector3 start, Vector3 end)
    {
        Vector3 startCenter = GetTileCenterFromWorld(start);
        Vector3 endCenter = GetTileCenterFromWorld(end);

        List<Vector3> path = new List<Vector3>();

        if (Vector3.Distance(start, startCenter) > 0.01f)
            path.Add(startCenter);

        if (Mathf.Abs(startCenter.x - endCenter.x) > 0.01f)
            path.Add(new Vector3(endCenter.x, startCenter.y, 0)); // 가로 이동

        if (Mathf.Abs(startCenter.y - endCenter.y) > 0.01f)
            path.Add(endCenter); // 세로 이동

        return path;
    }

    /// <summary>
    /// 경로 따라 이동
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveAlongPath(CrewMember crew, List<Vector3> path)
    {
        foreach (Vector3 waypoint in path)
        {
            Vector3 startPos = crew.transform.position;
            float distance = Vector3.Distance(startPos, waypoint);
            float moveDuration = distance / speed;

            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                float fraction = t / moveDuration;
                crew.transform.position = Vector3.Lerp(startPos, waypoint, fraction);
                yield return null;
            }

            crew.transform.position = waypoint;
        }

        CheckForCombat(crew);
    }

    /// <summary>
    /// 우클릭한 위치로 이동 명령 전달
    /// </summary>
    public void IssueMoveCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            /*
            Vector3 rawTarget = hit.point;
            rawTarget.z = 0.0f; // z값 0으로 (화면에서 사라짐 방지)

            Debug.Log("이동 목적지: " + rawTarget);

            int cnt = selectedCrew.Count;

            for (int i = 0; i < cnt; i++)
            {
                Vector3 finalDestination = rawTarget;
                if (cnt > 1)
                {
                    float angle = i * Mathf.PI * 2 / cnt;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spacing;
                    finalDestination += offset;
                }
                StartCoroutine(MoveCrewMember(selectedCrew[i], finalDestination));
            }*/

            Vector3 rawTarget = hit.point;
            Vector3 targetCenter = GetTileCenterFromWorld(rawTarget);

            foreach (CrewMember crew in selectedCrew)
            {
                List<Vector3> path = GetPathViaRoomCenters(crew.transform.position, targetCenter);
                StartCoroutine(MoveAlongPath(crew, path));
            }
        }
    }

    /// <summary>
    /// Lerp를 사용한 이동 코루틴 (추후 디테일 조정 가능)
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="destination"></param>
    /// <returns></returns>
    public IEnumerator MoveCrewMember(CrewMember crew, Vector3 destination)
    {
        Vector3 startPos = crew.transform.position;
        float distance = Vector3.Distance(startPos, destination);
        float moveDuration = distance / speed;  // 거리에 따른 이동 시간 계산

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float fraction = t / moveDuration;
            crew.transform.position = Vector3.Lerp(startPos, destination, fraction);
            yield return null;
        }
        crew.transform.position = destination;
        CheckForCombat(crew);
    }

    /// <summary>
    /// 일정 범위 내에 적이 있다면 전투 실행
    /// </summary>
    /// <param name="crew"></param>
    public void CheckForCombat(CrewMember crew)
    {
        Collider[] colliders = Physics.OverlapSphere(crew.transform.position, attackRange);
        foreach (Collider col in colliders)
        {
            CrewMember targetCrew = col.GetComponent<CrewMember>();
            if (targetCrew != null && targetCrew != crew)
            {
                // 간단히 종족이 다르면 적으로 간주 (추후 팀 개념 등으로 확장 가능)
                if (crew.race != targetCrew.race)
                {
                    Attack(crew, targetCrew);
                }
            }
        }
    }

    /// <summary>
    /// 전투 메서드: 1 hit 당 피해량 계산 후 체력 차감, 체력이 0 이하이면 죽음 처리
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="target"></param>
    public void Attack(CrewMember attacker, CrewMember target)
    {
        // 피해량 계산식: (공격 주체 기본 공격 + 장비 공격력(tmp)) * (1 - (상대 방어력 / 100))
        float damage = (attacker.attack + tmpEquipmentAttack) * (1 - target.defense / 100f);
        target.health -= damage;
        Debug.Log($"{attacker.crewName}이(가) {target.crewName}에게 {damage}의 피해를 입혔습니다.");

        if (target.health <= 0)
        {
            target.isAlive = false;
            Debug.Log($"{target.crewName}이(가) 사망하였습니다.");
            Destroy(target.gameObject);
        }
    }
}
