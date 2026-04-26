using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VerticalCarousel : MonoBehaviour
{
    [Header("Layout")]
    public float itemHeight = 120f;
    public float spacing = 20f;

    [Header("Visible Range (UX)")]
    public float visibleTopY = 300f;
    public float visibleBottomY = -300f;

    [Header("Recycle Range (Logic)")]
    public float recycleTopY = 450f;
    public float recycleBottomY = -450f;

    [Header("Snap")]
    public float snapDuration = 0.25f;
    public Ease snapEase = Ease.OutCubic;

    public List<RectTransform> items = new List<RectTransform>();

    float Step => itemHeight + spacing;

    void Start()
    {
        InitPositions();
        RefreshAll();
    }

    void InitPositions()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].anchoredPosition = new Vector2(0, -i * Step);
        }
    }

    void RefreshAll()
    {
        foreach (var item in items)
        {
            UpdateVisibility(item);
        }

        // 혹시 초기 배치가 recycle 범위를 넘는 경우 대비
        RecycleIfNeeded();
    }

    // 드래그 / 관성 중 호출
    public void MoveAll(float deltaY)
    {
        foreach (var item in items)
        {
            item.anchoredPosition += Vector2.up * deltaY;
            UpdateVisibility(item);
        }

        RecycleIfNeeded();
    }

    // 보이는 영역 제어
    void UpdateVisibility(RectTransform item)
    {
        float y = item.anchoredPosition.y;
        bool visible = y <= visibleTopY && y >= visibleBottomY;

        item.gameObject.SetActive(visible);
    }

    // 무한 재배치
    void RecycleIfNeeded()
    {
        foreach (var item in items)
        {
            float y = item.anchoredPosition.y;

            if (y > recycleTopY)
            {
                MoveToBottom(item);
                break;
            }
            else if (y < recycleBottomY)
            {
                MoveToTop(item);
                break;
            }
        }
    }

    void MoveToBottom(RectTransform item)
    {
        float minY = float.MaxValue;
        foreach (var it in items)
            minY = Mathf.Min(minY, it.anchoredPosition.y);

        item.anchoredPosition = new Vector2(0, minY - Step);
        UpdateVisibility(item);
    }

    void MoveToTop(RectTransform item)
    {
        float maxY = float.MinValue;
        foreach (var it in items)
            maxY = Mathf.Max(maxY, it.anchoredPosition.y);

        item.anchoredPosition = new Vector2(0, maxY + Step);
        UpdateVisibility(item);
    }

    // 중앙 스냅
    public void SnapToCenter()
    {
        RectTransform closest = GetClosestToCenter();
        float offset = -closest.anchoredPosition.y;

        foreach (var item in items)
        {
            item.DOAnchorPosY(item.anchoredPosition.y + offset, snapDuration)
                .SetEase(snapEase)
                .OnUpdate(() => UpdateVisibility(item));
        }
    }

    RectTransform GetClosestToCenter()
    {
        RectTransform closest = null;
        float minDist = float.MaxValue;

        foreach (var item in items)
        {
            float dist = Mathf.Abs(item.anchoredPosition.y);
            if (dist < minDist)
            {
                minDist = dist;
                closest = item;
            }
        }
        return closest;
    }
}
