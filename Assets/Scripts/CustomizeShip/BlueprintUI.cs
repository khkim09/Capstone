using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설계도 UI 전체를 관리.
/// B키로 토글 / 가격 표기 / 제작 버튼 활성화 등 담당.
/// </summary>
public class BlueprintUI : MonoBehaviour
{
    /// <summary>
    /// 설계도 UI 루트 패널.
    /// </summary>
    public GameObject blueprintRoot;

    /// <summary>
    /// 총 설계도 가격을 표시할 텍스트 UI.
    /// </summary>
    public Text totalCostText;

    /// <summary>
    /// 함선 제작 버튼.
    /// </summary>
    public Button buildButton;

    /// <summary>
    /// UI 표시 여부 토글 상태.
    /// </summary>
    private bool isActive = false;

    private void Start()
    {
        blueprintRoot.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isActive = !isActive;
            blueprintRoot.SetActive(isActive);
        }

        if (isActive)
        {
            totalCostText.text = $"설계도 가격: {BlueprintManager.Instance.totalBlueprintCost}";

            Ship ship = FindAnyObjectByType<Ship>();
            int currentCurrency = (int)ResourceManager.Instance.GetResource(ResourceType.COMA);
            bool canExchange = ship.IsFullHitPoint();

            buildButton.interactable = BlueprintManager.Instance.CanBuildShip(currentCurrency, canExchange);
        }
    }

    /// <summary>
    /// '함선 제작' 버튼 클릭 시 호출.
    /// </summary>
    public void OnClickBuild()
    {
        var ship = FindAnyObjectByType<Ship>();
        BlueprintManager.Instance.ApplyBlueprintToShip(ship, (int)ResourceManager.Instance.GetResource(ResourceType.COMA));
    }
}
