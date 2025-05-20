using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoraleIcon : TooltipPanelBase
{
    /// <summary>
    /// 종족 유형
    /// </summary>
    public CrewRace race;

    /// <summary>
    /// 아이콘에 표시되는 총 사기 변화량
    /// </summary>
    public int totalValue;

    /// <summary>
    /// 이미지 컴포넌트 (UI)
    /// </summary>
    public Image iconImage;

    /// <summary>
    /// 이 아이콘을 통해 표시되는 개별 사기 효과들의 목록
    /// </summary>
    private List<MoraleEffectData> effectDataList = new();

    protected override void Start()
    {
        base.Start(); // TooltipPanelBase의 Start 호출 (툴팁 생성)

        // 이미지 컴포넌트 가져오기
        if (iconImage == null)
        {
            iconImage = GetComponent<Image>();
            if (iconImage == null) iconImage = gameObject.AddComponent<Image>();
        }

        // 초기값 설정 (아직 Initialize가 호출되지 않았을 경우)
        if (totalValue == 0 && effectDataList.Count == 0)
        {
            UpdateSprite();
        }
    }

    /// <summary>
    /// 아이콘 초기화
    /// </summary>
    public void Initialize(CrewRace race)
    {
        this.race = race;

        // 이미 Start가 호출됐는지 확인하고, 아니라면 기본 초기화 진행
        if (currentToolTip == null)
        {
            // 캔버스 컴포넌트 찾기 및 툴팁 생성
            base.Start();

            // 이미지 컴포넌트 가져오기
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
                if (iconImage == null) iconImage = gameObject.AddComponent<Image>();
            }
        }

        // 초기값 설정
        totalValue = 0;
        UpdateSprite();
    }

    /// <summary>
    /// 사기 효과 데이터 추가
    /// </summary>
    public void AddEffectData(MoraleEffectData data)
    {
        effectDataList.Add(data);
        RecalculateTotalValue();
        UpdateSprite();
    }

    /// <summary>
    /// 사기 효과 데이터 제거
    /// </summary>
    public void RemoveEffectData(MoraleEffectData data)
    {
        effectDataList.Remove(data);
        RecalculateTotalValue();
        UpdateSprite();
    }

    /// <summary>
    /// 모든 데이터 제거
    /// </summary>
    public void ClearEffectData()
    {
        effectDataList.Clear();
        totalValue = 0;
        UpdateSprite();
    }

    /// <summary>
    /// 전체 사기 값 재계산
    /// </summary>
    private void RecalculateTotalValue()
    {
        totalValue = 0;
        foreach (MoraleEffectData data in effectDataList) totalValue += data.value;
    }

    /// <summary>
    /// 스프라이트 업데이트
    /// </summary>
    private void UpdateSprite()
    {
        if (iconImage == null) return;

        string spritePath = "Sprites/UI/";
        string change = totalValue >= 0 ? "up" : "down";

        switch (race)
        {
            case CrewRace.None:
                spritePath += "all_";
                break;
            case CrewRace.Human:
                spritePath += "human_";
                break;
            case CrewRace.Amorphous:
                spritePath += "amorphous_";
                break;
            case CrewRace.Beast:
                spritePath += "beast_";
                break;
            case CrewRace.Insect:
                spritePath += "insect_";
                break;
            case CrewRace.MechanicTank:
            case CrewRace.MechanicSup:
                spritePath += "mechanic_";
                break;
        }

        spritePath += change;

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite == null)
        {
            Debug.LogError($"스프라이트를 찾을 수 없습니다: {spritePath}");
            return;
        }

        iconImage.sprite = newSprite;

        // Image의 Native Size로 설정
        iconImage.SetNativeSize();

        // RectTransform 크기 조정 (옵션)
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 크기 조정이 필요한 경우 여기에 코드 추가
            // rectTransform.sizeDelta = new Vector2(50, 50);
        }
    }

    /// <summary>
    /// 툴팁 텍스트 업데이트 (TooltipPanelBase에서 상속)
    /// </summary>
    protected override void SetToolTipText()
    {
        if (currentToolTip == null) return;

        TextMeshProUGUI tooltipTextComponent = currentToolTip.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltipTextComponent == null) return;

        string raceStr = race.Localize();
        string changeAmount = totalValue >= 0 ? $"+{totalValue}" : totalValue.ToString();

        string baseText = "";

        if (race == CrewRace.None)
            baseText = "ui.morale.tooltip.4".Localize(changeAmount);
        else
            baseText = "ui.morale.tooltip.1".Localize(raceStr, changeAmount);

        // 세부 효과 목록 추가
        if (effectDataList.Count > 0)
        {
            baseText += "ui.morale.tooltip.2".Localize();

            foreach (MoraleEffectData data in effectDataList)
            {
                string effectValue = data.value >= 0 ? $"+{data.value}" : data.value.ToString();
                string source = !string.IsNullOrEmpty(data.source) ? $" ({data.source})" : "";
                int yearsLeft = data.EndYear - GameManager.Instance.CurrentYear;

                baseText += "ui.morale.tooltip.3".Localize(effectValue, yearsLeft);
            }
        }

        tooltipTextComponent.text = baseText;
    }
}
