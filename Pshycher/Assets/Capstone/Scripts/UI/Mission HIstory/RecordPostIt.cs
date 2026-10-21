using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordPostIt : MonoBehaviour
{
    public Image background;
    public TMP_Text labelText;
    public Button button;

    public RecordData record { get; private set; }
    public float baseRotation { get; private set; }
    public RectTransform rt { get; private set; }
    public CanvasGroup group { get; private set; }

    void Awake()
    {
        rt = (RectTransform)transform;
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(RecordData data, Color color, float rotation, Action<RecordPostIt> onClick)
    {
        record = data;
        baseRotation = rotation;

        if (background != null) background.color = color;
        if (labelText != null) labelText.text = data.listLabel;

        rt.localRotation = Quaternion.Euler(0f, 0f, rotation);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(this));
        }
    }
}