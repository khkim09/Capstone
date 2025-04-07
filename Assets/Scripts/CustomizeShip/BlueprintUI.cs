using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설계도 UI 전체를 관리.
/// B키로 토글 / 가격 표기 / 제작 버튼 활성화 등 담당.
/// </summary>
public class BlueprintUI : MonoBehaviour
{
    [Header("UI")]
    /// <summary>
    /// 설계도 UI 루트 패널.
    /// </summary>
    public GameObject blueprintRoot;

    /// <summary>
    /// 총 설계도 가격을 표시할 텍스트 UI.
    /// </summary>
    public TMP_Text totalCostText;

    /// <summary>
    /// 함선 제작 버튼.
    /// </summary>
    public Button buildButton;

    [Header("Connections")]
    /// <summary>
    /// 제작한 설계도
    /// </summary>
    public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 현재 소유 함선
    /// </summary>
    public Ship playerShip;

    private void Update()
    {
        if (targetBlueprintShip != null && playerShip != null)
        {
            totalCostText.text = $"Blueprint Cost: {targetBlueprintShip.totalBlueprintCost}";

            int currentCurrency = (int)ResourceManager.Instance.GetResource(ResourceType.COMA);
            buildButton.interactable = currentCurrency >= targetBlueprintShip.totalBlueprintCost && playerShip.IsFullHitPoint();
        }
    }

    /// <summary>
    /// '함선 제작' 버튼 클릭 시 호출.
    /// </summary>
    public void OnClickBuild()
    {
        if (targetBlueprintShip != null && playerShip != null)
            playerShip.ReplaceShipWithBlueprint(targetBlueprintShip);
        // var ship = FindAnyObjectByType<Ship>();
        // BlueprintShip.ApplyBlueprintToShip(ship, (int)ResourceManager.Instance.GetResource(ResourceType.COMA));
    }
}
