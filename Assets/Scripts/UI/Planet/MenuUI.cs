using UnityEngine;

public class MenuUI : MonoBehaviour
{
    private PlanetData planetData;

    private GameObject storageButton;

    private GameObject moveButton;

    public int moveCost = 1000;

    void Start()
    {
        planetData = GameManager.Instance.WhereIAm();

        // IsHome();

        Debug.Log(planetData.planetRace);
    }

    private void IsHome()
    {
        if (planetData.isHome)
        {
            transform.Find("StorageButton").gameObject.SetActive(true);
            transform.Find("MoveButton").gameObject.SetActive(false);
        }
        else
        {
            transform.Find("StorageButton").gameObject.SetActive(false);
            transform.Find("MoveButton").gameObject.SetActive(true);
        }
    }

    public void OnTradeButtonClicked()
    {
        //SceneChanger.Instance.LoadScene("Trade");
    }

    public void OnStorageButtonClicked()
    {
        //SceneChanger.Instance.LoadScene("Storage");
    }

    public void OnMoveButtonClicked()
    {
        //todo:거점 이동용 현재 재화 확인 및 체크 후 ishome 전환
        GameObject message = transform.Find("MoveMessage").gameObject;
        message.SetActive(true);
    }

    public void OnEmployButtonClicked()
    {
        //todo:고용창 활성화
    }

    public void OnBlueprintButtonClicked()
    {
        SceneChanger.Instance.LoadScene("Customize");
    }

    public void OnMoveAcceptButtonClicked()
    {
        if (ResourceManager.Instance.COMA >= moveCost)
        {
            planetData.isHome = true;
            ResourceManager.Instance.ChangeResource(ResourceType.COMA, -moveCost);
            transform.Find("MoveMessage").gameObject.SetActive(false);
        }
        else
        {
            transform.Find("MoveMessage/NoCOMA").gameObject.SetActive(true);
        }
    }

    public void OnMoveCancelButtonClicked()
    {
        transform.Find("MoveMessage").gameObject.SetActive(false);
    }

    public void OnNoCOMAClicked()
    {
        GameObject message = transform.Find("MoveMessage").gameObject;
        message.transform.Find("NoCOMA").gameObject.SetActive(false);
        message.SetActive(false);
    }
}
