using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RTSSelectionManager : MonoBehaviour
{
    // 드래그 시 표시할 텍스처 (Inspector에서 할당)
    public Texture2D selectionTexture;
    public LayerMask groundLayer;

    // 드래그 영역의 시작점
    private Vector2 dragStartPos;
    private bool isDragging = false;

    // 단일 클릭 임계값
    private float clickThreshold = 10f;

    // 선택된 선원 리스트
    public List<CrewMember> selectedCrew = new List<CrewMember>();

    // 이동 후 전투를 위한 공격 범위 (추후 조정)
    public float attackRange = 2.0f;
    // 임시 장비 공격력 변수
    public float tmpEquipmentAttack = 0.0f;

    // 이동 시 Crew 간 공간
    public float spacing = 0.7f;

    void Update()
    {
        // 왼쪽 마우스 버튼 눌림: 선택 시작
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            isDragging = true;
        }

        // 왼쪽 마우스 버튼 뗌: 선택 완료
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            float distance = Vector2.Distance(dragStartPos, Input.mousePosition);
            if (distance < clickThreshold)
            {
                SelectSingleCrew();
            }
            else
            {
                SelectMultipleCrew();
            }
        }

        // 오른쪽 마우스 버튼 클릭: 이동 명령 발동
        if (Input.GetMouseButtonDown(1))
        {
            IssueMoveCommand();
        }
    }

    // OnGUI를 이용하여 드래그 사각형 표시
    void OnGUI()
    {
        if (isDragging)
        {
            Rect rect = GetScreenRect(dragStartPos, Input.mousePosition);
            DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }

    // 두 스크린 좌표로 사각형 영역 계산 (y좌표 보정 포함)
    Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
    {
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        Vector2 topLeft = Vector2.Min(screenPosition1, screenPosition2);
        Vector2 bottomRight = Vector2.Max(screenPosition1, screenPosition2);
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    // 사각형 내부 채우기
    void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, selectionTexture);
        GUI.color = Color.white;
    }

    // 사각형 테두리 그리기
    void DrawScreenRectBorder(Rect rect, float thickness, Color color)
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

    void DeselectAll()
    {
        selectedCrew.Clear();
        CrewMember[] allCrew = GameObject.FindObjectsOfType<CrewMember>();
        foreach (CrewMember crew in allCrew)
            crew.GetComponent<Renderer>().material.color = Color.white;
    }

    // 단일 선원 선택
    public void SelectSingleCrew()
    {
        DeselectAll();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            CrewMember crew = hit.collider.GetComponent<CrewMember>();
            if (crew)
            {
                selectedCrew.Add(crew);
                crew.GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    // 영역 내 선원 다중 선택
    void SelectMultipleCrew()
    {
        // 선택 리스트 초기화
        selectedCrew.Clear();
        Rect selectionRect = GetScreenRect(dragStartPos, Input.mousePosition);

        // 모든 CrewMember를 찾아서 선택 영역 안에 있는지 확인
        CrewMember[] allCrew = GameObject.FindObjectsOfType<CrewMember>();
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

    // 우클릭한 위치로 이동 명령 전달
    void IssueMoveCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 destination = hit.point;
            destination.z = 0.0f; // z값 0으로 (화면에서 사라짐 방지)

            Debug.Log("이동 목적지: " + destination);

            int cnt = selectedCrew.Count;

            for (int i = 0; i < cnt; i++)
            {
                Vector3 finalDestination = destination;
                if (cnt > 1)
                {
                    float angle = i * Mathf.PI * 2 / cnt;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * spacing;
                    finalDestination += offset;
                }
                StartCoroutine(MoveCrewMember(selectedCrew[i], finalDestination));
            }
        }
        else
        {
            Debug.Log("Ground 레이어 찾지 못함");
        }
    }

    // Lerp를 사용한 이동 코루틴 (추후 디테일 조정 가능)
    IEnumerator MoveCrewMember(CrewMember crew, Vector3 destination)
    {
        float t = 0;
        Vector3 startPos = crew.transform.position;
        float moveDuration = 1.0f; // 이동 시간 (임시 값)
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            crew.transform.position = Vector3.Lerp(startPos, destination, t / moveDuration);
            yield return null;
        }
        crew.transform.position = destination;
        // 이동 완료 후 근처에 적(종족이 다른 선원)이 있는지 확인
        CheckForCombat(crew);
    }

    // 일정 범위 내에 적이 있다면 전투 실행
    void CheckForCombat(CrewMember crew)
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

    // 전투 메서드: 1 hit 당 피해량 계산 후 체력 차감, 체력이 0 이하이면 죽음 처리
    void Attack(CrewMember attacker, CrewMember target)
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
