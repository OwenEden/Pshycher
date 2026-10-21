using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordCategoryItem : MonoBehaviour
{
    [Header("포스트잇 더미")]
    public RectTransform stackRoot;
    public Image stackCardPrefab;                          // 포스트잇 한 장
    public Vector2 stackOffset = new Vector2(10f, 10f);    // 뒷장일수록 이만큼 밀림
    public int maxStack = 5;

    [Header("라벨")]
    public TMP_Text labelText;
    public Button button;

    public RecordCategoryData category { get; private set; }

    // 앞장부터 순서대로
    private readonly List<RectTransform> cards = new List<RectTransform>();
    public IReadOnlyList<RectTransform> Cards => cards;

    public void Setup(RecordCategoryData data, int unlockedCount, Action<RecordCategoryItem> onClick)
    {
        category = data;
        if (labelText != null) labelText.text = data.displayName;

        BuildStack(Mathf.Min(unlockedCount, maxStack), data.postItColor);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(this));
        }
    }

    private void BuildStack(int count, Color color)
    {
        foreach (var c in cards)
            if (c != null) Destroy(c.gameObject);
        cards.Clear();

        if (stackCardPrefab == null || stackRoot == null) return;

        // 뒷장부터 생성해야 앞장이 위에 그려짐
        var ordered = new RectTransform[count];
        for (int i = count - 1; i >= 0; i--)
        {
            Image card = Instantiate(stackCardPrefab, stackRoot);
            card.color = color;
            card.rectTransform.anchoredPosition = stackOffset * i;
            ordered[i] = card.rectTransform;
        }
        cards.AddRange(ordered);
    }

    public void SetStackVisible(bool visible)
    {
        if (stackRoot != null) stackRoot.gameObject.SetActive(visible);
    }
}