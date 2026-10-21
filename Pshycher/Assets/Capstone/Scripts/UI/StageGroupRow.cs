using UnityEngine;
using TMPro;

public class StageGroupRow : MonoBehaviour
{
    [Header("UI 참조")]
    public TMP_Text groupNameText;
    public Transform cardParent;      // Horizontal Layout Group

    public void SetGroupName(string name)
    {
        if (groupNameText != null)
            groupNameText.text = name;
    }
}