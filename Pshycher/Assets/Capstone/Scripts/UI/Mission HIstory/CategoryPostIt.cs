using UnityEngine;
using UnityEngine.UI;

public class CategoryPostIt : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        RecordRoomUIManager.Instance.ShowListView();
    }
}