using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 각 무기 항목에 대한 UI 버튼.
/// </summary>
public class WeaponInventoryButton : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler,
    IEndDragHandler
{
    /// <summary>
    /// 인벤토리의 무기 이미지
    /// </summary>
    [Header("UI")] public Image icon;

    /// <summary>
    /// 무기 이름 라벨
    /// </summary>
    public TMP_Text label;

    /// <summary>
    /// 무기 정보
    /// </summary>
    private ShipWeaponData weaponData;

    /// <summary>
    /// 무기 드래그를 관리하는 handler
    /// </summary>
    private BlueprintWeaponDragHandler dragHandler;

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 무기 기본 크기 - 가로
    /// </summary>
    [Header("button size")] private float baseWidth = 100f;

    /// <summary>
    /// 무기 기본 크기 - 세로
    /// </summary>
    private float baseHeight = 50f;

    /// <summary>
    /// 무기 정보에 따른 초기화 작업
    /// </summary>
    /// <param name="data">무기 정보</param>
    /// <param name="handler">드래그 핸들러</param>
    public void Initialize(ShipWeaponData data, BlueprintWeaponDragHandler handler)
    {
        weaponData = data;
        dragHandler = handler;

        if (label != null) label.text = data.weaponName;

        // 무기 아이콘 설정
        if (icon != null)
        {
            if (data.weaponIcon != null)
                icon.sprite = data.weaponIcon;
            else
                icon.sprite = null; // 기본 스프라이트 또는 프로젝트에서 정의한 기본 무기 아이콘
        }

        // 인벤토리 버튼 크기 설정 (모든 무기는 2x1 크기로 고정)
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null) rt.sizeDelta = new Vector2(baseWidth, baseHeight);
    }

    /// <summary>
    /// 클릭 시 바로 드래그 시작 준비
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isDragging = true;
    }

    /// <summary>
    /// 드래그 시작
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDragging || dragHandler == null)
            return;

        dragHandler.StartDragging(weaponData);
    }

    /// <summary>
    /// 드래그 중 (WeaponDragHandler가 업데이트에서 처리)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        // WeaponDragHandler가 업데이트에서 처리함
    }

    /// <summary>
    /// 드래그 종료
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
