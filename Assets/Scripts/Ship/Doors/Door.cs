using UnityEngine;

// TODO: 문 회전 부분 수정해야 함.

/// <summary>
/// 방에 부착되어 있는 문 객체입니다.
/// </summary>
public class Door : MonoBehaviour
{
    /// <summary>문의 데이터</summary>
    [SerializeField] private DoorData doorData;

    /// <summary>현재 문 레벨</summary>
    [SerializeField] private int currentLevel = 1;

    /// <summary>문의 방향</summary>
    [SerializeField] private DoorDirection direction;

    /// <summary>문의 방향 프로퍼티</summary>
    public DoorDirection Direction => direction;

    /// <summary>현재 문이 열려있는지 여부</summary>
    [SerializeField] private bool isOpen = false;

   /// <summary>문의 원래 그리드 위치 (방 회전 전)</summary>
   [SerializeField] private Vector2Int originalGridPosition;

    /// <summary>원래 그리드 위치 프로퍼티</summary>
    public Vector2Int OriginalGridPosition
    {
        get => originalGridPosition;
        set => originalGridPosition = value;
    }

    /// <summary>문의 원래 방향 (방 회전 전)</summary>
    [SerializeField] private DoorDirection originalDirection;

    /// <summary>원래 방향 프로퍼티</summary>
    public DoorDirection OriginalDirection
    {
        get => originalDirection;
        set => originalDirection = value;
    }

    /// <summary>문 통과 쿨다운 타이머</summary>
    private float passTimer = 0f;

    /// <summary>문의 현재 내구도</summary>
    private int currentHitPoint;

    /// <summary>문 스프라이트 렌더러</summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>문 애니메이터</summary>
    private Animator animator;

    /// <summary>문 오디오 소스</summary>
    private AudioSource audioSource;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        InitializeDoor();
    }

    private void Update()
    {
        // 쿨다운 타이머 업데이트
        if (passTimer > 0)
        {
            passTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 외부에서 문 초기화
    /// </summary>
    public void Initialize(DoorData doorData, int level, DoorDirection dir)
    {
        this.doorData = doorData;
        this.currentLevel = level;
        this.direction = dir;
        this.originalDirection = dir;  // 초기 방향 저장

        // 다른 초기화 작업
        InitializeDoor();
    }

    /// <summary>
    /// 문의 초기화
    /// </summary>
    private void InitializeDoor()
    {
        DoorData.DoorLevel level = doorData.GetDoorData(currentLevel);
        if (level == null) return;

        spriteRenderer.sprite = level.doorSprite;

        // 방향에 따른 회전 설정
        SetDoorRotation();
    }

    /// <summary>
    /// 문의 방향 설정
    /// </summary>
    public void SetDirection(DoorDirection newDirection)
    {
        this.direction = newDirection;
        SetDoorRotation();
    }

    /// <summary>
    /// 방향을 반시계 방향으로 90도 회전
    /// </summary>
    private DoorDirection RotateDirectionCounterClockwise(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North: return DoorDirection.West;
            case DoorDirection.East: return DoorDirection.North;
            case DoorDirection.South: return DoorDirection.East;
            case DoorDirection.West: return DoorDirection.South;
            default: return dir;
        }
    }


    /// <summary>
    /// 문의 위치 설정
    /// </summary>
    public void SetGridPosition(Vector2Int newPosition)
    {
        // 그리드 위치 업데이트 (실제 transform.position은 Room에서 관리)
        this.originalGridPosition = newPosition;
    }

    /// <summary>
    /// 문의 방향에 따라 회전 설정
    /// </summary>
    private void SetDoorRotation()
    {
        switch (direction)
        {
            case DoorDirection.North:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case DoorDirection.East:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case DoorDirection.South:
                transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case DoorDirection.West:
                transform.rotation = Quaternion.Euler(0, 0, 270);
                break;
        }
    }

    /// <summary>
    /// 문 열기 시도
    /// </summary>
    public bool TryOpenDoor(bool bypassCooldown = false)
    {
        // 쿨다운 체크
        if (!bypassCooldown && passTimer > 0)
            return false;

        DoorData.DoorLevel level = doorData.GetDoorData(currentLevel);
        if (level == null) return false;

        // 자동문인 경우 전력 체크
        if (level.powerRequirement > 0 && !HasEnoughPower())
            return false;

        // 문 열기
        isOpen = true;

        // 애니메이션 재생
        if (animator != null)
            animator.SetBool("IsOpen", true);

        // 사운드 재생
        if (audioSource != null && level.openSound != null)
        {
            audioSource.clip = level.openSound;
            audioSource.Play();
        }

        // 쿨다운 설정
        passTimer = level.passThroughDelay;

        return true;
    }

    /// <summary>
    /// 문 닫기
    /// </summary>
    public void CloseDoor()
    {
        if (!isOpen) return;

        isOpen = false;

        // 애니메이션 재생
        if (animator != null)
            animator.SetBool("IsOpen", false);

        // 사운드 재생
        DoorData.DoorLevel level = doorData.GetDoorData(currentLevel);
        if (audioSource != null && level != null && level.closeSound != null)
        {
            audioSource.clip = level.closeSound;
            audioSource.Play();
        }
    }

    /// <summary>
    /// 전력 요구사항 체크
    /// </summary>
    private void CheckPowerRequirement()
    {
        DoorData.DoorLevel level = doorData.GetDoorData(currentLevel);
        if (level == null || level.powerRequirement <= 0) return;

        // 자동문이지만 전력이 없으면 닫기
        if (isOpen && !HasEnoughPower())
        {
            CloseDoor();
        }
    }

    /// <summary>
    /// 전력이 충분한지 체크 (함선의 전력 시스템과 연동 필요)
    /// </summary>
    private bool HasEnoughPower()
    {
        // 함선의 전력 시스템에서 전력 확인 로직
        // 임시로 항상 true 반환
        return true;
    }
}
