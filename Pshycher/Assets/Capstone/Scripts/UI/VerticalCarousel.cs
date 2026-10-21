using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class VerticalCarousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Layout")]
    public float itemHeight = 120f;
    public float spacing = 20f;

    [Header("Visible Range (UX)")]
    public float visibleTopY = 300f;
    public float visibleBottomY = -300f;

    [Header("Drag")]
    public float dragSensitivity = 1f;

    [Header("Snap")]
    public float snapDuration = 0.25f;
    public Ease snapEase = Ease.OutCubic;

    [Header("항목 (순서 = 도구/능력 순서, None 제외)")]
    public List<RectTransform> items = new List<RectTransform>();

    [Header("해금 표시")]
    public WheelMode itemType = WheelMode.Tool;
    [Tooltip("흑백 머티리얼 (비우면 lockedTint로 어둡게)")]
    public Material lockedMaterial;
    public Color lockedTint = new Color(0.4f, 0.4f, 0.4f, 1f);
    [Tooltip("잠긴 항목 중앙에 붙일 자물쇠")]
    public GameObject lockIconPrefab;

    private Canvas rootCanvas;

    // 해금 표시용
    private bool[] unlocked;
    private readonly Dictionary<RectTransform, Image[]> itemImages = new Dictionary<RectTransform, Image[]>();
    private readonly Dictionary<Image, (Material mat, Color color)> originalLooks
        = new Dictionary<Image, (Material, Color)>();
    private readonly Dictionary<RectTransform, GameObject> lockIcons = new Dictionary<RectTransform, GameObject>();

    float Step => itemHeight + spacing;
    float Span => items.Count * Step;

    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;

        // 항목 사이 빈 곳도 잡을 수 있게 투명 그래픽 확보
        if (GetComponent<Graphic>() == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = Color.clear;
        }

        CacheItemImages();
        ValidateItemCount();
    }

    void OnEnable()
    {
        // 화면에 들어올 때마다 해금 상태 갱신
        RefreshUnlocks();
    }

    void Start()
    {
        InitPositions();
    }

    void OnDisable()
    {
        DOTween.Kill(this);
    }

    // ── 해금 표시 ─────────────────────────────
    public bool IsUnlocked(int index)
    {
        return unlocked != null && index >= 0 && index < unlocked.Length && unlocked[index];
    }

    public void RefreshUnlocks()
    {
        unlocked = new bool[items.Count];

        for (int i = 0; i < items.Count; i++)
        {
            // 항목 i = enum 값 i + 1 (None 제외)
            unlocked[i] = itemType == WheelMode.Tool
                ? PlayerUnlocks.IsToolAvailable((ToolType)(i + 1))
                : PlayerUnlocks.IsAbilityAvailable((AbilityType)(i + 1));

            ApplyLockVisual(items[i], !unlocked[i]);
        }
    }

    // 자물쇠를 붙이기 전, 원래 항목에 있던 이미지만 기억 (자물쇠는 흑백 대상에서 제외)
    private void CacheItemImages()
    {
        foreach (var item in items)
        {
            if (item == null) continue;

            var images = item.GetComponentsInChildren<Image>(true);
            itemImages[item] = images;

            foreach (var img in images)
                originalLooks[img] = (img.material, img.color);
        }
    }

    private void ApplyLockVisual(RectTransform item, bool locked)
    {
        if (item == null || !itemImages.TryGetValue(item, out var images)) return;

        foreach (var img in images)
        {
            if (img == null) continue;
            var original = originalLooks[img];

            if (!locked)
            {
                img.material = original.mat;
                img.color = original.color;
            }
            else if (lockedMaterial != null)
            {
                img.material = lockedMaterial;
                img.color = original.color;
            }
            else
            {
                img.material = original.mat;
                img.color = original.color * lockedTint;
            }
        }

        SetLockIcon(item, locked);
    }

    private void SetLockIcon(RectTransform item, bool show)
    {
        lockIcons.TryGetValue(item, out var icon);

        if (show && icon == null && lockIconPrefab != null)
        {
            icon = Instantiate(lockIconPrefab, item);
            icon.name = "LockIcon";
            if (icon.transform is RectTransform rt) rt.anchoredPosition = Vector2.zero;
            lockIcons[item] = icon;
        }

        if (icon != null) icon.SetActive(show);
    }

    private void ValidateItemCount()
    {
        Type enumType = itemType == WheelMode.Tool ? typeof(ToolType) : typeof(AbilityType);
        int expected = Enum.GetValues(enumType).Length - 1;   // None 제외

        if (items.Count != expected)
            Debug.LogWarning($"{name}: {itemType} 캐러셀 항목 수가 {items.Count}개입니다. ({expected}개여야 순서가 맞음)", this);
    }

    // ── 드래그 ────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        DOTween.Kill(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        MoveAll(eventData.delta.y / scale * dragSensitivity);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SnapToCenter();
    }

    // ── 배치 ──────────────────────────────────
    void InitPositions()
    {
        for (int i = 0; i < items.Count; i++)
        {
            items[i].anchoredPosition = new Vector2(0, -i * Step);
            Wrap(items[i]);
            UpdateVisibility(items[i]);
        }
    }

    public void MoveAll(float deltaY)
    {
        foreach (var item in items)
        {
            item.anchoredPosition += Vector2.up * deltaY;
            Wrap(item);
            UpdateVisibility(item);
        }
    }

    void Wrap(RectTransform item)
    {
        if (items.Count == 0) return;

        float half = Span * 0.5f;
        Vector2 p = item.anchoredPosition;

        while (p.y > half) p.y -= Span;
        while (p.y <= -half) p.y += Span;

        item.anchoredPosition = p;
    }

    void UpdateVisibility(RectTransform item)
    {
        float y = item.anchoredPosition.y;
        bool visible = y <= visibleTopY && y >= visibleBottomY;

        if (item.gameObject.activeSelf != visible)
            item.gameObject.SetActive(visible);
    }

    // ── 스냅 ──────────────────────────────────
    public void SnapToCenter()
    {
        RectTransform closest = GetClosestToCenter();
        if (closest == null) return;

        float target = -closest.anchoredPosition.y;
        float applied = 0f;

        DOTween.Kill(this);
        DOTween.To(() => applied, v =>
        {
            MoveAll(v - applied);
            applied = v;
        }, target, snapDuration)
            .SetEase(snapEase)
            .SetId(this);
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