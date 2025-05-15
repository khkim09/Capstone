using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CrewInfoPanel : MonoBehaviour
{
    [SerializeField] private Button atkButton;
    [SerializeField] private Button defButton;
    [SerializeField] private Button assistButton;

    [SerializeField] private TextMeshProUGUI crewName;
    [SerializeField] private TextMeshProUGUI currentHealth;
    [SerializeField] private Image crewIcon;

    [SerializeField] private Button rtsButton;
    [SerializeField] private GameObject crewInfoToolTip;

    private GameObject currentToolTip;
    private Canvas canvasComponent;
    private RectTransform canvasRectTransform;
    private bool isMouseOver = false; // 마우스가 버튼 위에 있는지 추적
    private Coroutine toolTipCoroutine; // 툴팁 표시 지연을 위한 코루틴

    private CrewMember currentCrew;

    private void Start()
    {
        // 캔버스 컴포넌트 찾기
        canvasComponent = GetComponentInParent<Canvas>();
        if (canvasComponent != null) canvasRectTransform = canvasComponent.transform as RectTransform;

        // 툴팁이 아직 생성되지 않았을 때만 생성 (중복 생성 방지)
        if (crewInfoToolTip != null && currentToolTip == null)
        {
            currentToolTip = Instantiate(crewInfoToolTip, canvasComponent.transform);

            // Canvas Group 추가
            CanvasGroup canvasGroup = currentToolTip.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = currentToolTip.AddComponent<CanvasGroup>();

            // 비활성화
            currentToolTip.SetActive(false);
        }
    }

    private void Update()
    {
        // 툴팁이 활성화되어 있고 마우스가 버튼 위에 있을 때 지속적으로 위치 업데이트
        if (currentToolTip != null && currentToolTip.activeInHierarchy && isMouseOver) UpdateToolTipPositionFromMouse();
    }

    public void Initialize(CrewMember crew)
    {
        currentCrew = crew;
        crewName.text = crew.crewName;
        currentHealth.text = $"{crew.health}/{crew.maxHealth}";
        crewIcon.sprite = Resources.Load<Sprite>($"Sprites/Crew/{crew.race.ToString().ToLower()}");

        Image atkImage = atkButton.GetComponent<Image>();
        Image defImage = defButton.GetComponent<Image>();
        Image assistImage = assistButton.GetComponent<Image>();

        if (crew.equippedWeapon != null)
            atkImage.sprite = crew.equippedWeapon.eqIcon;
        if (crew.equippedShield != null)
            defImage.sprite = crew.equippedShield.eqIcon;
        if (crew.equippedAssistant != null)
            assistImage.sprite = crew.equippedAssistant.eqIcon;

        // TODO : 장비 UI SetActive(true)
        atkButton.onClick.AddListener(() => { });
        defButton.onClick.AddListener(() => { });
        assistButton.onClick.AddListener(() => { });

        rtsButton.onClick.AddListener(() => { OnRTSButtonClicked(); });
        // 마우스 호버 이벤트 추가
        SetupToolTipEvents();
    }


    private void OnRTSButtonClicked()
    {
        RTSSelectionManager.Instance.Select(currentCrew);
    }

    private void SetupToolTipEvents()
    {
        // EventTrigger 컴포넌트 추가 또는 가져오기
        EventTrigger trigger = rtsButton.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = rtsButton.gameObject.AddComponent<EventTrigger>();

        // 기존 트리거 제거
        trigger.triggers.Clear();

        // 마우스 엔터 이벤트
        EventTrigger.Entry enterEntry = new();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((eventData) => { OnMouseEnter((PointerEventData)eventData); });
        trigger.triggers.Add(enterEntry);

        // 마우스 종료 이벤트
        EventTrigger.Entry exitEntry = new();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((eventData) => { OnMouseExit((PointerEventData)eventData); });
        trigger.triggers.Add(exitEntry);
    }

    private void OnMouseEnter(PointerEventData eventData)
    {
        isMouseOver = true;

        // 기존 코루틴이 실행 중이면 중지
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 0.3초 후 툴팁 표시 (툴팁이 비활성 상태일 때만)
        if (currentToolTip != null && !currentToolTip.activeInHierarchy)
            toolTipCoroutine = StartCoroutine(ShowToolTipWithDelay(eventData));
    }

    private void OnMouseExit(PointerEventData eventData)
    {
        isMouseOver = false;

        // 코루틴이 실행 중이면 중지
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 툴팁 숨기기 (Destroy 대신 SetActive 사용)
        if (currentToolTip != null) currentToolTip.SetActive(false);
    }

    private void UpdateToolTipPositionFromMouse()
    {
        if (currentToolTip == null || canvasComponent == null) return;

        // 현재 마우스 위치를 화면 좌표에서 캔버스 로컬 좌표로 변환
        Vector2 mousePosition;
        Vector2 screenMousePosition = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, screenMousePosition, canvasComponent.worldCamera, out mousePosition);

        // 기존 UpdateToolTipPosition 로직 사용
        UpdateToolTipPositionInternal(mousePosition);
    }

    private void UpdateToolTipPosition(PointerEventData eventData)
    {
        if (currentToolTip == null || canvasComponent == null) return;

        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, eventData.position, canvasComponent.worldCamera, out mousePosition);

        UpdateToolTipPositionInternal(mousePosition);
    }

    private void UpdateToolTipPositionInternal(Vector2 mousePosition)
    {
        RectTransform toolTipRect = currentToolTip.GetComponent<RectTransform>();
        if (toolTipRect == null) return;

        Vector2 toolTipSize = toolTipRect.sizeDelta;
        Vector2 canvasSize = canvasRectTransform.sizeDelta;

        float offsetX = 20f;
        float offsetY = 20f;

        Vector2 targetPosition = mousePosition;

        // 🥇 왼쪽 아래를 기본으로 시도
        bool canShowLeft = mousePosition.x - toolTipSize.x - offsetX >= -canvasSize.x / 2;
        bool canShowBelow = mousePosition.y - toolTipSize.y - offsetY >= -canvasSize.y / 2;

        float pivotX = canShowLeft ? 1f : 0f;
        float pivotY = canShowBelow ? 1f : 0f;
        toolTipRect.pivot = new Vector2(pivotX, pivotY);

        targetPosition.x += canShowLeft ? -offsetX : offsetX;
        targetPosition.y += canShowBelow ? -offsetY : offsetY;

        // 위치 적용
        toolTipRect.anchoredPosition = targetPosition;
    }

    private IEnumerator ShowToolTipWithDelay(PointerEventData eventData)
    {
        // 0.3초 대기
        yield return new WaitForSeconds(0.3f);

        // 마우스가 여전히 버튼 위에 있는지 확인
        if (isMouseOver && currentToolTip != null && !currentToolTip.activeInHierarchy)
        {
            // Canvas Group 가져오기 (이미 Start에서 생성됨)
            CanvasGroup canvasGroup = currentToolTip.GetComponent<CanvasGroup>();

            // 완전히 투명하게 만들어서 보이지 않게 함
            canvasGroup.alpha = 0f;

            // 툴팁 활성화 (Instantiate 대신 SetActive 사용)
            currentToolTip.SetActive(true);

            TextMeshProUGUI toolTipText = currentToolTip.GetComponentInChildren<TextMeshProUGUI>();
            toolTipText.text =
                $"{"ui.crewinfo.attack".Localize()} {currentCrew.attack} {"ui.crewinfo.defense".Localize()} {currentCrew.defense}\n"
                + $"<{"ui.crewinfo.skill"}>\n"
                + $"{"room.roomtype.cockpit".Localize()} {currentCrew.skills[SkillType.PilotSkill]}/{currentCrew.maxPilotSkillValue}\n"
                + $"{"room.roomtype.engine".Localize()} {currentCrew.skills[SkillType.EngineSkill]}/{currentCrew.maxEngineSkillValue}\n"
                + $"{"room.roomtype.power".Localize()} {currentCrew.skills[SkillType.PowerSkill]}/{currentCrew.maxPowerSkillValue}\n"
                + $"{"room.roomtype.shield".Localize()} {currentCrew.skills[SkillType.ShieldSkill]}/{currentCrew.maxShieldSkillValue}\n"
                + $"{"room.roomtype.weaponcontrol".Localize()} {currentCrew.skills[SkillType.WeaponSkill]}/{currentCrew.maxWeaponSkillValue}\n"
                + $"{"room.roomtype.ammunition".Localize()} {currentCrew.skills[SkillType.AmmunitionSkill]}/{currentCrew.maxAmmunitionSkillValue}\n"
                + $"{"room.roomtype.medbay".Localize()} {currentCrew.skills[SkillType.MedBaySkill]}/{currentCrew.maxMedBaySkillValue}\n"
                + $"{"room.skilltype.repairshort".Localize()} {currentCrew.skills[SkillType.RepairSkill]}/{currentCrew.maxRepairSkillValue}";

            // 다음 프레임에서 위치 조정 (레이아웃이 완전히 계산된 후)
            yield return null;

            // 위치 조정
            UpdateToolTipPosition(eventData);

            // 0.1초 후 서서히 나타나게 함
            yield return new WaitForSeconds(0.1f);

            // 마우스가 여전히 위에 있는지 재확인
            if (isMouseOver && currentToolTip != null && currentToolTip.activeInHierarchy) canvasGroup.alpha = 1f;
        }

        // 코루틴 참조 초기화
        toolTipCoroutine = null;
    }

    private void OnDestroy()
    {
        // 실행 중인 코루틴 정리
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 툴팁이 존재하면 명시적으로 삭제 (중복 생성 방지를 위해)
        if (currentToolTip != null)
        {
            Destroy(currentToolTip);
            currentToolTip = null;
        }
    }
}
