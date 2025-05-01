using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 선원 RTS 선택 및 이동 명령 관리 (단일, 다중 선택)
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


    //--- RTS 이동 관련---

    /// <summary>
    /// 기본 그리드
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 선원 경로 계산기
    /// </summary>
    public CrewPathfinder crewPathfinder;

    /// <summary>
    /// 선원 이동 가능 검사기
    /// </summary>
    public CrewMovementValidator movementValidator;

    /// <summary>
    /// 함선
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 싱글톤 instance
    /// </summary>
    public static RTSSelectionManager Instance { get; private set; }

    /// <summary>
    /// 싱글톤 instance 초기화
    /// </summary>
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    //----------외곽선 효과-----------------
    public Material outlineMaterial;
    public Material defaultMaterial;
    /// <summary>
    /// crew의 스프라이트 렌더러에 onoff값에 따라 Material을 변경하며 외곽선 효과를 준다.
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="onoff"></param>
    private void SetOutline(CrewMember crew, bool onoff)
    {
        crew.GetSpriteRenderer().material = onoff ? new Material(outlineMaterial) : defaultMaterial;
    }

    /// <summary>
    /// 게임 오브젝트 연결 (RTS 이동 검사를 위한 오브젝트)
    /// </summary>
    private IEnumerator Start()
    {
        yield return null; // 1 frame 대기

        RefreshMovementData();
        outlineMaterial = Resources.Load<Material>("Material/UnitOutlineMaterial");
        defaultMaterial= Resources.Load<Material>("Material/UnitDefaultMaterial");
    }

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
            SetOutline(crew,false);
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
                SetOutline(crew,true);
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

            // 선택
            if (selectionRect.Contains(screenPos, true))
            {
                selectedCrew.Add(crew);
                // 선택됨 표시 (예: 색상 변경)
                SetOutline(crew, true);
            }
            else
            {
                // 선택되지 않은 경우 원래 색상으로
                SetOutline(crew, false);
            }
        }
    }

    /// <summary>
    /// RTS 이동을 위한 기여 오브젝트 초기화
    /// </summary>
    public void RefreshMovementData()
    {
        if (crewPathfinder != null && movementValidator != null && playerShip != null)
        {
            movementValidator.Initialize(playerShip.GetAllRooms());
            crewPathfinder.Initialize(movementValidator);
        }
        else
            Debug.LogError("CrewPathFinder or MovementValidator가 세팅되지 않음");
    }

    /// <summary>
    /// 선원 이동 명령 최적화
    /// </summary>
    public void IssueMoveCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null)
        {
            Room targetRoom = hit.collider.GetComponent<Room>();
            if (targetRoom == null)
                return;

            // 여기서 기존 점유 타일 해제 해버리셈


            Dictionary<CrewMember, List<Vector2Int>> crewPaths = new Dictionary<CrewMember, List<Vector2Int>>();

            // 1. 선택된 선원별로 이동 가능한 Entry 경로 계산
            foreach (CrewMember crew in selectedCrew)
            {
                Debug.LogWarning($"{crew} 감지 완료, 목적지 : {targetRoom} 경로 추적 시작");
                List<Vector2Int> path;
                if (crew.currentRoom == targetRoom)
                {
                    path=new List<Vector2Int>();
                    path.Add(Vector2Int.zero);
                }
                else
                {
                    path = crewPathfinder.FindPathToRoom(crew, targetRoom);
                }
                if (path != null)
                    crewPaths[crew] = path;
            }

            // 2. 최단 경로가 짧은 선원부터 우선순위 할당
            List<KeyValuePair<CrewMember, List<Vector2Int>>> orderedCrewPaths = crewPaths.OrderBy(x => x.Value.Count).ToList();

            // 3. 방의 회전된 entry 타일 우선순위 가져오기
            List<Vector2Int> rotatedEntryTiles = targetRoom.GetRotatedCrewEntryGridPriority();

            // 4. 이미 예약된 타일 저장소 (선원 도착 전이라도 중복 할당 방지)
            HashSet<Vector2Int> globallyReservedTiles = new HashSet<Vector2Int>();

            // 5. 선원 순회하며 타일 하나씩 할당
            foreach (KeyValuePair<CrewMember, List<Vector2Int>> kvp in orderedCrewPaths)
            {
                CrewMember crew = kvp.Key;

                // 6. 현재 선원이 이미 목적지 방의 올바른 타일에 있다면 이동 명령 생략
                if (crew.currentRoom == targetRoom)
                {
                    // 이 방에서 현재 점유 중인 타일
                    Vector2Int currentTile = crew.GetCurrentTile();

                    // 비어 있는 타일 중 가장 높은 우선 순위의 타일 검색
                    Vector2Int? highestPriorityEmptyTile = rotatedEntryTiles.Where
                    (
                        t => !targetRoom.IsTileOccupiedByCrew(t)
                    ).FirstOrDefault();

                    // 현재 선원이 점유 중인 타일, 빈 타일의 방에서의 우선순위
                    int currentIndex = rotatedEntryTiles.IndexOf(currentTile);
                    int emptyIndex = highestPriorityEmptyTile.HasValue ? rotatedEntryTiles.IndexOf(highestPriorityEmptyTile.Value) : -1;

                    // 비어있는 가장 높은 우선순위 타일의 우선순위 > 점유 중인 타일 우선순위
                    if (currentIndex != -1 && emptyIndex != -1 && currentIndex <= emptyIndex)
                        continue;
                }

                // 사용할 타일 검색 (방 내부 타일 중 미점유 + 예약 X)
                Vector2Int? assignedTile = null;
                foreach (Vector2Int tile in rotatedEntryTiles)
                {
                    // 해당 타일 선원 점유 여부
                    if (!targetRoom.IsTileOccupiedByCrew(tile)
                    && !IsTileOccupiedByCrewGlobal(tile)
                    && !globallyReservedTiles.Contains(tile))
                    {
                        assignedTile = tile;
                        globallyReservedTiles.Add(tile); // 예약 타일 기록
                        break;
                    }
                }

                if (assignedTile == null)
                {
                    Debug.LogWarning("모든 타일 꽉 참");
                    continue;
                }

                // 7. 현재 선원 위치에서 배정된 타일까지 경로 다시 계산
                List<Vector2Int> finalPath = AStar.FindPath(crew.GetCurrentTile(), assignedTile.Value, movementValidator);

                if (finalPath != null)
                {
                    // 선원 예약 목적지 기록
                    crew.reservedRoom = targetRoom;
                    // crew.reservedTile = assignedTile.Value;

                    // 현재 이동 중인 상태에서 새로운 이동 명령 수신
                    if (crew.isMoving)
                        crew.CancelAndRedirect(finalPath);
                    else // 정지 상태에서 새로운 이동 명령 수신
                        crew.AssignPathAndMove(finalPath);
                }
                else
                    Debug.LogWarning($"선원 {crew.crewName}이(가) {assignedTile}로 가는 경로를 찾지 못했습니다.");
            }
        }
    }

    /// <summary>
    /// 타일에 이미 다른 선원이 있는지 검사
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    private bool IsTileOccupiedByCrewGlobal(Vector2Int tile)
    {
        List<CrewBase> crewList = playerShip.GetSystem<CrewSystem>().GetCrews();

        foreach (CrewBase crewBase in crewList)
        {
            CrewMember crewMember = crewBase as CrewMember;
            if (crewMember == null)
                continue;

            if (crewMember.GetCurrentTile() == tile)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 같은 방 내에 적이 있다면 공격 AI
    /// </summary>
    /// <param name="crew"></param>
    public void CheckForCombat(CrewMember crew)
    {
        /*
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
        */
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

            // 타일 점유 해제 및 방 퇴장 처리
            if (target.currentRoom != null)
            {
                Vector2Int currentTile = target.GetCurrentTile();
                target.currentRoom.VacateTile(currentTile);
                target.currentRoom.OnCrewExit(target);
            }

            // 선원 제거 (죽음)
            Destroy(target.gameObject);
        }
    }
}
