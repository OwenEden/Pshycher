using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class HorizontalPager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("페이지들이 놓일 부모 (이 오브젝트의 자식, 가운데 앵커)")]
    public RectTransform content;
    [Tooltip("페이지 폭 (0이면 이 오브젝트의 폭)")]
    public float pageWidth = 0f;

    [Header("넘김")]
    [Range(0.05f, 0.5f)] public float pageTurnRatio = 0.15f;   // 페이지 폭의 이 비율 이상 끌면 넘어감
    [Range(0f, 0.5f)] public float overscrollRatio = 0.15f;    // 처음·끝에서 더 끌리는 정도
    public float snapDuration = 0.25f;
    public Ease snapEase = Ease.OutCubic;

    public event Action<int> OnPageChanged;
    public int CurrentPage { get; private set; }
    public int PageCount => pages.Count;

    private readonly List<RectTransform> pages = new List<RectTransform>();
    private RectTransform viewport;
    private Canvas rootCanvas;
    private bool initialized;

    public float PageWidth
    {
        get { Init(); return pageWidth > 0f ? pageWidth : viewport.rect.width; }
    }

    void Awake() => Init();

    void OnDisable() => DOTween.Kill(this);

    // 비활성 상태에서 먼저 불려도 동작하도록 지연 초기화
    private void Init()
    {
        if (initialized) return;
        initialized = true;

        viewport = (RectTransform)transform;
        var canvas = GetComponentInParent<Canvas>(true);
        rootCanvas = canvas != null ? canvas.rootCanvas : null;

        // 빈 곳도 끌 수 있도록 투명 그래픽 확보
        if (GetComponent<Graphic>() == null)
        {
            var img = gameObject.AddComponent<Image>();
            img.color = Color.clear;
        }
    }

    // ── 페이지 구성 ───────────────────────────
    public void Clear()
    {
        Init();
        DOTween.Kill(this);

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        pages.Clear();
        CurrentPage = 0;
        content.anchoredPosition = Vector2.zero;
    }

    public RectTransform AddPage(RectTransform template)
    {
        Init();

        RectTransform page = Instantiate(template, content);
        page.gameObject.SetActive(true);
        page.anchorMin = page.anchorMax = new Vector2(0.5f, 0.5f);
        page.pivot = new Vector2(0.5f, 0.5f);
        page.sizeDelta = viewport.rect.size;
        page.localRotation = Quaternion.identity;
        page.localScale = Vector3.one;
        page.anchoredPosition = new Vector2(pages.Count * PageWidth, 0f);

        pages.Add(page);
        return page;
    }

    public RectTransform GetPage(int index) =>
        index >= 0 && index < pages.Count ? pages[index] : null;

    public void GoToPage(int page, bool instant = false)
    {
        if (pages.Count == 0) { CurrentPage = 0; return; }

        page = Mathf.Clamp(page, 0, pages.Count - 1);
        bool changed = page != CurrentPage;
        CurrentPage = page;

        float x = -page * PageWidth;
        DOTween.Kill(this);

        if (instant)
            content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
        else
            content.DOAnchorPosX(x, snapDuration).SetEase(snapEase).SetId(this);

        if (changed) OnPageChanged?.Invoke(page);
    }

    // ── 드래그 ────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        DOTween.Kill(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pages.Count == 0) return;

        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        float w = PageWidth;
        float x = content.anchoredPosition.x + eventData.delta.x / scale;

        float min = -(pages.Count - 1) * w - w * overscrollRatio;
        float max = w * overscrollRatio;

        content.anchoredPosition = new Vector2(Mathf.Clamp(x, min, max), content.anchoredPosition.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float w = PageWidth;
        float moved = content.anchoredPosition.x - (-CurrentPage * w);
        float threshold = w * pageTurnRatio;

        int target = CurrentPage;
        if (moved < -threshold) target++;   // 왼쪽으로 밀면 다음 페이지
        else if (moved > threshold) target--;

        GoToPage(target);
    }
}