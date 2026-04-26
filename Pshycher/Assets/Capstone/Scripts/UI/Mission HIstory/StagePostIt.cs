using UnityEngine;
using UnityEngine.UI;

public class StagePostIt : MonoBehaviour
{
    /*public int stageId;
    public GameObject lockIcon;
    public GameObject clearIcon;*/

    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

/*    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = StageData.IsUnlocked(stageId);
        bool cleared = StageData.IsCleared(stageId);

        lockIcon.SetActive(!unlocked);
        clearIcon.SetActive(cleared);

        button.interactable = unlocked;
    }*/

    void OnClick()
    {
        //if (!StageData.IsUnlocked(stageId)) return;

        RecordRoomUIManager.Instance.ToggleDetailView();
        //DetailPostIt.Instance.SetData(stageId);
    }
}