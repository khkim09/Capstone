using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconInteraction : TooltipPanelBase
{
    private RoomType roomType;
    public Room room;

    private TextMeshProUGUI description;

    protected void Start()
    {
        room = transform.parent.GetComponent<Room>();
        roomType = room.roomType;
    }

    protected override void SetToolTipText()
    {
        string text = room.GetRoomData().GetRoomDataByLevel(room.GetCurrentLevel()).roomName.Localize() + "\n";
        switch (roomType)
        {
            case RoomType.Ammunition:
            case RoomType.Cockpit:
            case RoomType.Engine:
            case RoomType.MedBay:
            case RoomType.Power:
            case RoomType.Shield:
            case RoomType.WeaponControl:
                text += "ui.room.icon.workingCrew".Localize() + "\n";
                if (room.workingCrew != null)
                    text += room.workingCrew.crewName;
                else
                    text += " -";
                text += "\n";
                break;
        }

        Dictionary<ShipStat, float> contributions = room.GetStatContributions();
        text += "HitPointsMax".Localize() + room.currentHitPoints + "/" + room.GetMaxHitPoints() + "\n";
        contributions.Remove(ShipStat.HitPointsMax);
        foreach (KeyValuePair<ShipStat, float> pair in contributions)
        {
            if (pair.Value != 0)
            {
                text += "    -" + pair.Key.ToString().Localize() + " +" + pair.Value + "\n";
            }
        }

        description.text = text;

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    protected void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 캔버스 컴포넌트 찾기
        GameObject canvasObject = GameObject.FindWithTag("RoomIconTooltip");
        if (canvasObject == null)
            return;
        canvasComponent = GameObject.FindWithTag("RoomIconTooltip").GetComponent<Canvas>();
        if (canvasComponent != null)
            canvasRectTransform = canvasComponent.transform as RectTransform;

        tooltipParent = GameObject.FindWithTag("TooltipParent");

        if (tooltipParent == null)
        {
            tooltipParent = new GameObject("TooltipParent");
            tooltipParent.tag = "TooltipParent";
            tooltipParent.transform.SetParent(canvasComponent.transform, false);
        }

        // 툴팁 생성
        CreateTooltip();
        description = currentToolTip.GetComponentInChildren<TextMeshProUGUI>();
        isMouseOver = true;
        lastMousePosition = Input.mousePosition; // 마우스 위치 저장

        // 기존 코루틴이 실행 중이면 중지
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 0.3초 후 툴팁 표시 (툴팁이 비활성 상태일 때만)
        if (currentToolTip != null && !currentToolTip.activeInHierarchy)
            toolTipCoroutine = StartCoroutine(ShowToolTipWithDelay());
    }

    protected IEnumerator ShowToolTipWithDelay()
    {
        // 0.2초 대기
        yield return new WaitForSeconds(0.2f);

        // 마우스가 여전히 패널 위에 있는지 확인
        if (isMouseOver && currentToolTip != null && !currentToolTip.activeInHierarchy)
        {
            // Canvas Group 가져오기
            CanvasGroup canvasGroup = currentToolTip.GetComponent<CanvasGroup>();

            // 완전히 투명하게 만들어서 보이지 않게 함
            canvasGroup.alpha = 0f;

            // 툴팁 활성화
            currentToolTip.SetActive(true);

            // 툴팁 텍스트 설정 (자식 클래스에서 구현)
            SetToolTipText();

            // 레이아웃 계산 완료 대기
            yield return null;

            // 위치 조정
            UpdateToolTipPosition();

            // 0.1초 후 서서히 나타나게 함
            yield return new WaitForSeconds(0.1f);

            // 마우스가 여전히 위에 있는지 재확인
            if (isMouseOver && currentToolTip != null && currentToolTip.activeInHierarchy)
                canvasGroup.alpha = 1f;
        }

        // 코루틴 참조 초기화
        toolTipCoroutine = null;
    }

    protected void UpdateToolTipPosition()
    {
        if (currentToolTip == null || canvasComponent == null) return;

        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, Input.mousePosition, canvasComponent.worldCamera, out mousePosition);

        UpdateToolTipPositionInternal(mousePosition);
    }

    protected void OnMouseExit()
    {
        isMouseOver = false;

        // 코루틴이 실행 중이면 중지
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 툴팁 숨기기
        if (currentToolTip != null)
            currentToolTip.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (room.GetIsPowerRequested())
        {
            if (!room.GetIsPowered() && room.parentShip.GetStat(ShipStat.PowerUsing) + room.GetPowerConsumption() <
                room.parentShip.GetStat(ShipStat.PowerCapacity))
            {
                room.isPowered = true;
                room.SetIsActive(false);
            }
            room.SetIsActive(!room.isActive);
            OnMouseExit();
            OnMouseEnter();
        }
    }
}
