using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 행성의 정보를 UI 툴팁 형태로 표시하는 컴포넌트입니다.
/// </summary>
public class PlanetTooltip : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    public TextMeshProUGUI planetNameText;

    public Image planetIcon;
    public TextMeshProUGUI planetDescriptionText;

    public TextMeshProUGUI planetEventUpText;
    public TextMeshProUGUI planetEventDownText;

    public TextMeshProUGUI planetCurrentQuestText;
}
