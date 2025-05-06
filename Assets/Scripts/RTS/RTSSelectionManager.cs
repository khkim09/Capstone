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
    /// 임시 장비 공격력 변수
    /// </summary>
    public float tmpEquipmentAttack = 0.0f;

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
        defaultMaterial = Resources.Load<Material>("Material/UnitDefaultMaterial");
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
            SetOutline(crew, false);
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

            // 아군만 선택 가능
            if (crew != null && crew.isPlayerControlled)
            {
                selectedCrew.Add(crew);
                SetOutline(crew, true);
                crew.originPosTile = crew.GetCurrentTile();
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

            // 아군만 선택 가능
            if (selectionRect.Contains(screenPos, true) && crew.isPlayerControlled)
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

        // 선택된 선원들에 대해 기존 위치 저장
        foreach (CrewMember crew in selectedCrew)
        {
            crew.originPosTile = crew.GetCurrentTile();
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
    /// 선택된 선원들을 목적지 방의 우선순위 타일로 최단 경로 기반 배정 후 이동시킵니다.
    /// </summary>
    public void IssueMoveCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider == null)
            return;

        Room targetRoom = hit.collider.GetComponent<Room>();
        if (targetRoom == null)
            return;

        List<CrewBase> allCrew = playerShip.CrewSystem.GetCrews();

        // 목적지 방의 우선순위 높은 순 타일 위치 리스트
        List<Vector2Int> entryTiles = targetRoom.GetRotatedCrewEntryGridPriority();

        // 1. 이동 명령 전, 목적지 방에 이미 위치한 선원들의 점유 타일 기록
        HashSet<Vector2Int> reservedTiles = new HashSet<Vector2Int>(targetRoom.occupiedCrewTiles);
        foreach (Vector2Int t in reservedTiles)
            Debug.LogWarning($"누구도 이동하기 전 {targetRoom}에 선원 배치된 위치 : {t}");

        // 1-1. 이동 명령으로 점유 예약됐지만 아직 실 점유하기 전은 제외
        foreach (CrewBase crewBase in allCrew)
        {
            if (crewBase is CrewMember cm)
            {
                // 선원이 이동 중이면서 예약 타일만 존재하고 아직 타일 점유하지 않은 경우 제외
                bool isCurrentlyReserved = selectedCrew.Contains(cm) && cm.reservedRoom == targetRoom && cm.GetCurrentTile() != cm.reservedTile;

                if (isCurrentlyReserved)
                    reservedTiles.Remove(cm.reservedTile);
            }
        }

        foreach (Vector2Int t in reservedTiles)
            Debug.LogWarning($"이동 직전 검사 : {targetRoom} 선원 점유 위치 : {t}");

        // 2. 아직 배정 안 된 선원 리스트 (선택된 모든 선원 중 아직 이동 안 한 선원)
        List<CrewMember> unassignedCrew = new(selectedCrew);

        // 3. 이동 안 한 선원이 없을 때 까지 루프 검사
        while (entryTiles.Count > 0 && unassignedCrew.Count > 0)
        {
            // 3-1. 가장 높은 우선순위의 비어있는 타일 선택
            Vector2Int? targetTile = entryTiles.FirstOrDefault(t => !reservedTiles.Contains(t));
            if (targetTile == null)
            {
                Debug.LogError("모든 타일이 점유되어 있습니다.");
                break;
            }

            Debug.LogWarning($"targettile : {targetTile}");

            // 3-2. 이동 명령에 필요한 필드값 초기화
            Vector2Int tile = targetTile.Value;
            CrewMember bestCrew = null;
            int bestCost = int.MaxValue;
            List<Vector2Int> bestPath = null;

            // 3-3. 아직 배정되지 않은 선원들 중 이 타일까지 최단거리 선원 탐색
            foreach (CrewMember crew in unassignedCrew)
            {
                // 사전 검사로 불필요한 A* 호출 방지
                if (!movementValidator.IsTileWalkable(tile))
                    continue;

                // (목적지 방 == 현재 방) 빈 타일의 우선순위 < 현재 타일의 우선순위 : 이동 X
                if (crew.currentRoom == targetRoom)
                {
                    int currentIndex = targetRoom.GetRotatedCrewEntryGridPriority().IndexOf(crew.GetCurrentTile());
                    int targetIndex = targetRoom.GetRotatedCrewEntryGridPriority().IndexOf(tile);

                    if (currentIndex != -1 && targetIndex != -1 && currentIndex < targetIndex)
                        continue;
                }

                // 각 선원 별 최단 경로 탐색
                List<Vector2Int> path = crewPathfinder.FindPathToTile(crew, tile);

                // case 1) 최단 거리 동일 && (일부는 목적지 방 == 현재 방, 나머지는 목적지 방 != 현재 방)
                // 목적지 방 == 현재 방인 선원들 모두 이동 후 목적지 방 != 현재방인 선원들이 이동하도록

                // case 2) 최단 거리 동일 && (선택된 선원 모두 목적지 방 == 현재 방)
                // 우선순위 더 높은 선원이 이동

                // case 3) 최단 거리 동일 && 선택된 선원 모두 각자 방 -> 동일한 목적지로 이동 시
                // 기존 각자 방에서 우선순위 더 낮은 타일의 선원이 이동

                // case 3-1) 위 상황에서 각자의 방에서 우선순위 호출했더니 동일한 경우
                // 먼저 검사한애가 이동하도록 (조건문에서 등호 제외하면 될듯)
                if (path != null)
                {
                    // 최단 경로
                    if (path.Count < bestCost)
                    {
                        bestCost = path.Count;
                        bestCrew = crew;
                        bestPath = path;
                    }
                    else if (path.Count == bestCost) // 최단 거리 동일
                    {
                        bool crewInTargetRoom = crew.currentRoom == targetRoom;
                        bool bestInTargetRoom = bestCrew.currentRoom == targetRoom;

                        // case 1) 선택된 선원 중 일부 (목적지 방 == 현재 방), 나머지 (목적지 방 != 현재 방) : (목적지 방 == 현재 방) 선원들이 이동
                        if (crewInTargetRoom && !bestInTargetRoom)
                        {
                            bestCrew = crew;
                            bestPath = path;
                        }
                        else if (crewInTargetRoom == bestInTargetRoom)
                        {
                            if (crewInTargetRoom)
                            {
                                // case 2) 선택된 선원 모두 (목적지 방 == 현재 방), 우선순위 더 높은 선원 이동
                                int crewIndex = crew.GetCurrentTilePriorityIndex();
                                int bestIndex = bestCrew.GetCurrentTilePriorityIndex();

                                if (crewIndex >= 0 && bestIndex >= 0 && crewIndex < bestIndex)
                                {
                                    bestCrew = crew;
                                    bestPath = path;
                                }
                            }
                            else
                            {
                                // case 3) 선원 모두 다른 방에서 동일 목적지 방으로 이동 : 더 낮은 우선순위 타일 이동
                                int crewIndex = crew.GetCurrentTilePriorityIndex();
                                int bestIndex = bestCrew.GetCurrentTilePriorityIndex();

                                if (crewIndex > bestIndex)
                                {
                                    bestCrew = crew;
                                    bestPath = path;
                                }
                                else if (crewIndex == bestIndex) // case 3-1) 우선순위 동일 경우 : 기존 bestCrew 유지
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }

            if (bestCrew == null)
            {
                Debug.LogError("해당 타일까지 갈 수 있는 선원이 없음.");
                break;
            }

            // 4. 이동 명령에 필요한 필드 값 세팅
            // 이동 중 새로운 이동 입력 대비, 취소된 예약 방, 타일 기록
            bestCrew.oldReservedRoom = bestCrew.reservedRoom;
            bestCrew.oldReservedTile = bestCrew.reservedTile;

            // 새로운 목적지 타일 설정
            bestCrew.reservedRoom = targetRoom;
            bestCrew.reservedTile = tile;

            // 5. (목적지 방 == 현재 방)인 선원이 차지하고 있던 타일 리스트 갱신
            Debug.LogWarning($"{bestCrew.race} 기존 점유 타일 : {bestCrew.originPosTile}");
            reservedTiles.Remove(bestCrew.originPosTile);
            reservedTiles.Add(tile);
            unassignedCrew.Remove(bestCrew);

            // 6. 이동 처리
            if (bestCrew.isMoving)
                bestCrew.CancelAndRedirect(bestPath);
            else
                bestCrew.AssignPathAndMove(bestPath);

            // 7. 함내 전투 실행 여부 체크 (자동 AI) - oninvoke로 받아와서 무빙 끝났으면 moveforcombat() 호출
            MoveForCombat(bestCrew, reservedTiles);
        }
    }

    /// <summary>
    /// 같은 방 내에 적이 있다면 공격 AI
    /// </summary>
    /// <param name="readyCombatCrew"></param>
    public void MoveForCombat(CrewMember readyCombatCrew, HashSet<Vector2Int> reservedTiles)
    {
        Debug.LogError("전투 이동 검사 시작");

        // 1. 도착한 방에서 적군 탐색
        List<CrewMember> enemiesInRoom = playerShip.CrewSystem.GetCrews().OfType<CrewMember>().Where
        (
            // 선원이 생존해 있고, 위치한 방이 현재 RTS 이동으로 도착한 선원과 같은 방이며, 적군일 때
            c => c.isAlive && c.currentRoom == readyCombatCrew.currentRoom && !c.isPlayerControlled
        ).ToList();

        if (enemiesInRoom.Count == 0)
            return;

        // 2. 현재 도착 위치(RTS 이동 결과)에서 가장 가까운 적 찾기 (A*)
        CrewMember closestEnemy = null;
        int shortestPathLength = int.MaxValue;
        List<Vector2Int> bestPathToEnemy = null;

        foreach (CrewMember enemy in enemiesInRoom)
        {
            List<Vector2Int> path = crewPathfinder.FindPathToTile(readyCombatCrew, enemy.GetCurrentTile());
            if (path != null && path.Count < shortestPathLength)
            {
                shortestPathLength = path.Count;
                closestEnemy = enemy;
                bestPathToEnemy = path;
            }
        }

        if (closestEnemy == null)
            return;

        Vector2Int enemyTile = closestEnemy.GetCurrentTile();
        Debug.LogWarning($"가장 가까운 적 위치 : {enemyTile}");

        // 3. 가장 가까운 적군 주변 4방향 이웃 타일 순회
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborTile = enemyTile + dir;
            Debug.LogWarning($"이웃 타일 검사 : {neighborTile}");

            // 방 내부의 타일인지 검사
            if (!movementValidator.IsTileWalkable(neighborTile))
                continue;

            // 이웃 타일이 같은 방이 아님
            if (!readyCombatCrew.currentRoom.OccupiesTile(neighborTile))
                continue;

            // 이웃 타일이 이미 점유 당한 타일 (해당 위치로 이동 불가) && 타일 예약한게 본인이 아니라면
            if (readyCombatCrew.currentRoom.IsTileOccupiedByCrew(neighborTile) && readyCombatCrew.reservedTile != neighborTile)
                continue;

            // 4. 이동 처리를 위한 필드값 세팅
            readyCombatCrew.oldReservedRoom = readyCombatCrew.reservedRoom;
            readyCombatCrew.oldReservedTile = readyCombatCrew.reservedTile;
            readyCombatCrew.reservedRoom = readyCombatCrew.currentRoom;
            readyCombatCrew.reservedTile = neighborTile;

            // 5. reservedTiles 갱신
            reservedTiles.Remove(readyCombatCrew.oldReservedTile);
            reservedTiles.Add(neighborTile);

            // 6. 실제 이동 처리
            List<Vector2Int> pathToNeighbor = crewPathfinder.FindPathToTile(readyCombatCrew, neighborTile);
            if (pathToNeighbor != null && pathToNeighbor.Count > 0)
                readyCombatCrew.CancelAndRedirect(pathToNeighbor);

            Debug.Log("전투 개시");

            // 7. 전투 구현
            // readyCombatCrew.Attack(closestEnemy); 같이 넘겨서 죽을때까지 전투 구현
            // but, 싸우는 도중 RTS 이동 명령 시 이동 해야됨.
            return;
        }
        Debug.LogError($"{readyCombatCrew.race}가 {closestEnemy.race}에 접근할 수 있는 빈 타일 없음");
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
