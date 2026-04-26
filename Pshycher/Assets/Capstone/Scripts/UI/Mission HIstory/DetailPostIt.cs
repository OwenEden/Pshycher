using UnityEngine;
using TMPro;
using DG.Tweening;

public class DetailPostIt : MonoBehaviour
{
    public static DetailPostIt Instance;

    public RectTransform postIt;
    public TextMeshProUGUI contentText;

    void Awake()
    {
        Instance = this;
    }

    public void SetData(int stageId)
    {
        contentText.text = StageTextDatabase.GetText(stageId);

        postIt.localScale = Vector3.zero;
        postIt.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
    }
}