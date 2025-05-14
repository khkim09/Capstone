using TMPro;
using UnityEngine;

public class YearDisplayUI : MonoBehaviour
{
    public TextMeshProUGUI yearText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateYear(GameManager.Instance.CurrentYear);
            GameManager.Instance.OnYearChanged += UpdateYear;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnYearChanged -= UpdateYear;
    }

    private void UpdateYear(int year)
    {
        yearText.text = $"Year: {year}";
    }

    // 버튼에 연결할 함수
    public void OnAddYearButtonClicked()
    {
        GameManager.Instance.AddYear();
    }
}
