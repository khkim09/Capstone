using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Planet : TooltipPanelBase
{
    [Header("PlanetData")] [SerializeField]
    private PlanetData planetData;

    [Header("행성 스프라이트 목록")] [SerializeField]
    private List<Sprite> planetSprites;

    private Sprite currentSprite;

    [Header("행성 버튼")] [SerializeField] private Button planetButton;

    public PlanetData PlanetData => planetData;

    public bool HasEvent => planetData.currentEvent != null;
    public bool HasQuest => planetData.currentQuest != null;

    protected override void Start()
    {
        base.Start();
    }

    protected override void SetToolTipText()
    {
        if (planetData == null || currentToolTip == null) return;

        PlanetTooltip planetTooltip = currentToolTip.GetComponent<PlanetTooltip>();

        if (planetTooltip != null)
        {
            planetTooltip.planetIcon.sprite = currentSprite;
            planetTooltip.planetNameText.text = planetData.planetName;
            if (!HasQuest && !HasEvent)
            {
                planetTooltip.planetEventUpText.text = "";
                planetTooltip.planetEventDownText.text = "";
                planetTooltip.planetCurrentQuestText.text = "";
                planetTooltip.planetDescriptionText.text =
                    $"{"ui.planet." + planetData.itemPlanet.ToString().ToLower() + "description"}".Localize();

                return;
            }

            planetTooltip.planetDescriptionText.text = "";
        }
    }

    public void SetPlanetData(PlanetData planetData)
    {
        this.planetData = planetData;
        currentSprite = planetSprites[(int)planetData.itemPlanet];
    }
}
