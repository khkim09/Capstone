using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipInfoPanel : TooltipPanelBase
{
    [SerializeField] private Image panelSprite;
    [SerializeField] private Sprite[] panelSpriteList;
    [SerializeField] private Image roomImage;
    [SerializeField] private TextMeshProUGUI roomName;

    private int roomCount = 0;
    public DamageLevel currentDamageLevel;
    private RoomType roomType;

    private List<Room> rooms = new();

    protected override void Start()
    {
        base.Start();
    }

    public void Initialize(Room room)
    {
        roomCount = 0;
        roomType = room.roomType;
        rooms.Add(room);
        currentDamageLevel = room.damageCondition;

        string color = "";
        switch (currentDamageLevel)
        {
            case DamageLevel.good:
                panelSprite.sprite = panelSpriteList[(int)DamageLevel.good];
                color = "green";
                break;
            case DamageLevel.scratch:
                panelSprite.sprite = panelSpriteList[(int)DamageLevel.scratch];
                color = "yellow";
                break;
            case DamageLevel.breakdown:
                panelSprite.sprite = panelSpriteList[(int)DamageLevel.breakdown];
                color = "red";
                break;
            default:
                panelSprite.sprite = panelSpriteList[(int)DamageLevel.good];
                color = "green";
                break;
        }


        roomName.text = $"{room.roomType.Localize()} : {++roomCount}";
        roomImage.sprite =
            Resources.Load<Sprite>($"Sprites/UI/Room Icons/{color}/{roomType.ToString().ToLower()}_{color}");
    }

    public void AddInfo(Room room)
    {
        if (room.damageCondition > currentDamageLevel)
        {
            panelSprite.sprite = panelSpriteList[(int)room.damageCondition];
            currentDamageLevel = room.damageCondition;
        }

        rooms.Add(room);
        roomName.text = $"{room.roomType.Localize()} : {++roomCount}";
    }

    // 부모 클래스의 추상 메서드 구현
    protected override void SetToolTipText()
    {
        if (rooms.Count == 0 || currentToolTip == null) return;

        TextMeshProUGUI toolTipText = currentToolTip.GetComponentInChildren<TextMeshProUGUI>();
        if (toolTipText != null)
        {
            toolTipText.text = "ui.roominfo.title".Localize() + "\n\n";

            foreach (Room room in rooms)
            {
                string roomNameText = $"{roomType.Localize()} Lv.{room.GetCurrentLevel()} ";
                string roomState = "";
                if (!room.isActive)
                    roomState = "room.active.false".Localize();
                else
                    roomState = room.damageCondition.Localize();

                toolTipText.text += roomNameText + roomState + "\n";

                Dictionary<ShipStat, float> shipStatDictionary = room.GetStatContributions();

                foreach (KeyValuePair<ShipStat, float> pair in shipStatDictionary)
                {
                    ShipStat shipStat = pair.Key;
                    float value = pair.Value;

                    if (value == 0) continue;

                    string roomContribution = $"- {"ship.shipstat.{shipStat}".Localize()} : {value}\n";
                    toolTipText.text += roomContribution;
                }

                toolTipText.text += "\n";
            }
        }
    }

    private string GetDamageText(DamageLevel damageLevel)
    {
        switch (damageLevel)
        {
            case DamageLevel.good:
                return "Condition: Good";
            case DamageLevel.scratch:
                return "Condition: Scratched";
            case DamageLevel.breakdown:
                return "Condition: Broken Down";
            default:
                return "Condition: Unknown";
        }
    }
}
