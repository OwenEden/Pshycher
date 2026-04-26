using UnityEngine;

public class RecordRoomUIManager : MonoBehaviour
{
    public static RecordRoomUIManager Instance;

    public GameObject categoryView;
    public GameObject stageListView;
    public GameObject detailView;
    public GameObject button_returntoMainlobby;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowCategory();
    }

    public void ShowCategory()
    {
        categoryView.SetActive(true);
        stageListView.SetActive(false);
        detailView.SetActive(false);
        button_returntoMainlobby.SetActive(true);
    }

    public void ShowListView()
    {
        categoryView.SetActive(false);
        stageListView.SetActive(true);
        detailView.SetActive(false);
        button_returntoMainlobby.SetActive(false);
    }

    public void ToggleDetailView()
    {
        detailView.SetActive(!detailView.activeSelf);
    }
}