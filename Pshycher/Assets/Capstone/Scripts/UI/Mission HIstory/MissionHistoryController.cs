using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MissionHistoryController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private enum ViewState { Category, List, Detail }

    [Header("데이터")]
    public RecordDatabase database;

    [Header("카테고리 화면")]
    public CanvasGroup categoryView;
    public HorizontalPager categoryPager;
    [Tooltip("비활성 상태로 두는 페이지 틀 (Horizontal Layout Group 포함)")]
    public RectTransform categoryPageTemplate;
    public RecordCategoryItem categoryItemPrefab;
    public int categoriesPerPage = 4;
    public TMP_Text collectedCountText;
    public string collectedCountFormat = "획득 갯수 {0} / {1}";

    [Header("리스트 화면")]
    public CanvasGroup listView;
    public HorizontalPager listPager;
    [Tooltip("비활성 상태로 두는 페이지 틀 — 자식 RectTransform이 포스트잇 자리 (최대 8개)")]
    public RectTransform listPageTemplate;
    public RecordPostIt postItPrefab;
    public GameObject emptyText;                 // "찾은 단서 없음"
    public float maxPostItRotation = 12f;

    [Header("상세 화면")]
    public CanvasGroup detailView;
    public CanvasGroup dimBackground;
    public RectTransform detailCard;
    public Image detailCardImage;
    public CanvasGroup detailContent;
    public TMP_Text detailBodyText;
    public TMP_Text detailTitleText;
    [Tooltip("이만큼 이상 끌어야 다음 기록으로 넘어감")]
    public float swipeThreshold = 80f;

    [Header("뒤로가기")]
    public Button backButton;

    [Header("연출")]
    public float unfoldDuration = 0.45f;
    public float unfoldStagger = 0.04f;
    public float zoomDuration = 0.3f;
    public float fadeDuration = 0.2f;
    public float slideDuration = 0.18f;

    private ViewState state;
    private bool isAnimating;

    private RecordCategoryData currentCategory;
    private List<RecordData> currentRecords = new List<RecordData>();
    private readonly List<RecordPostIt> listItems = new List<RecordPostIt>();

    private int detailIndex;
    private Vector2 detailHome;
    private float dragAccum;
    private bool draggingDetail;

    private Canvas rootCanvas;
    private readonly object tweenId = new object();

    private SaveData Save => SaveManager.Instance != null ? SaveManager.Instance.current : null;
    private int SlotsPerPage => listPageTemplate != null ? Mathf.Max(1, listPageTemplate.childCount) : 8;

    void Awake()
    {
        var canvas = GetComponentInParent<Canvas>();
        rootCanvas = canvas != null ? canvas.rootCanvas : null;

        if (detailCard != null) detailHome = detailCard.anchoredPosition;
        if (categoryPageTemplate != null) categoryPageTemplate.gameObject.SetActive(false);
        if (listPageTemplate != null) listPageTemplate.gameObject.SetActive(false);

        backButton?.onClick.AddListener(OnBack);
    }

    void OnEnable()
    {
        // 기록실에 들어올 때마다 카테고리 화면부터, 최신 해금 상태로
        ShowCategoryInstant();
    }

    void OnDisable()
    {
        DOTween.Kill(tweenId);
        isAnimating = false;
    }

    // ── 뒤로가기 ──────────────────────────────
    private void OnBack()
    {
        if (isAnimating) return;

        switch (state)
        {
            case ViewState.Detail: CloseDetail(); break;
            case ViewState.List: BackToCategory(); break;
            case ViewState.Category: MenuManager.Instance?.BackToMain(); break;
        }
    }

    // ── 카테고리 화면 ─────────────────────────
    private void ShowCategoryInstant()
    {
        DOTween.Kill(tweenId);

        SetView(categoryView, true, 1f);
        SetView(listView, false, 1f);
        SetView(detailView, false, 1f);

        BuildCategories();

        state = ViewState.Category;
        isAnimating = false;
    }

    private void BuildCategories()
    {
        if (database == null || categoryPager == null) return;

        categoryPager.Clear();
        var save = Save;
        int perPage = Mathf.Max(1, categoriesPerPage);
        RectTransform page = null;

        for (int i = 0; i < database.categories.Count; i++)
        {
            var cat = database.categories[i];
            if (cat == null) continue;

            if (page == null || i % perPage == 0)
                page = categoryPager.AddPage(categoryPageTemplate);

            var item = Instantiate(categoryItemPrefab, page);
            item.Setup(cat, database.GetUnlocked(cat, save).Count, OnCategoryClicked);
        }

        categoryPager.GoToPage(0, true);

        if (collectedCountText != null)
            collectedCountText.text = string.Format(collectedCountFormat,
                database.CountUnlocked(save), database.TotalCount);
    }

    // ── 카테고리 → 리스트 (펼치기) ────────────
    private void OnCategoryClicked(RecordCategoryItem item)
    {
        if (isAnimating || state != ViewState.Category) return;
        isAnimating = true;
        currentCategory = item.category;

        SetView(listView, true, 1f);
        BuildList(item.category);
        Canvas.ForceUpdateCanvases();

        var seq = DOTween.Sequence().SetId(tweenId);
        categoryView.blocksRaycasts = false;
        seq.Insert(0f, categoryView.DOFade(0f, fadeDuration));

        int firstPageCount = Mathf.Min(listItems.Count, SlotsPerPage);

        if (firstPageCount == 0)
        {
            // 기록이 없으면 "찾은 단서 없음"만 페이드 인
            listView.alpha = 0f;
            seq.Insert(0f, listView.DOFade(1f, fadeDuration));
        }
        else
        {
            var cards = item.Cards;
            Vector3 lastCardPos = cards.Count > 0
                ? Center(cards[cards.Count - 1])
                : Center(item.stackRoot != null ? item.stackRoot : (RectTransform)item.transform);
            float cardWidth = cards.Count > 0 ? WorldWidth(cards[0]) : 0f;

            item.SetStackVisible(false);

            for (int k = 0; k < firstPageCount; k++)
            {
                RecordPostIt p = listItems[k];

                Vector3 target = p.rt.position;
                float startScale = cardWidth > 0f ? cardWidth / WorldWidth(p.rt) : 1f;
                Vector3 start = k < cards.Count ? Center(cards[k]) : lastCardPos;

                p.rt.position = start;
                p.rt.localRotation = Quaternion.identity;
                p.rt.localScale = Vector3.one * startScale;

                float delay = k * unfoldStagger;

                // 더미보다 많은 장은 맨 뒷장 자리에서 이어서 나타남
                if (k >= cards.Count)
                {
                    p.group.alpha = 0f;
                    seq.Insert(delay, p.group.DOFade(1f, fadeDuration));
                }

                seq.Insert(delay, p.rt.DOMove(target, unfoldDuration).SetEase(Ease.OutCubic));
                seq.Insert(delay, p.rt.DOLocalRotate(new Vector3(0f, 0f, p.baseRotation), unfoldDuration).SetEase(Ease.OutCubic));
                seq.Insert(delay, p.rt.DOScale(1f, unfoldDuration).SetEase(Ease.OutCubic));
            }
        }

        seq.OnComplete(() =>
        {
            SetView(categoryView, false, 1f);
            item.SetStackVisible(true);
            state = ViewState.List;
            isAnimating = false;
        });
    }

    private void BuildList(RecordCategoryData category)
    {
        listPager.Clear();
        listItems.Clear();
        currentRecords = database.GetUnlocked(category, Save);

        int perPage = SlotsPerPage;
        RectTransform page = null;

        for (int i = 0; i < currentRecords.Count; i++)
        {
            if (i % perPage == 0)
                page = listPager.AddPage(listPageTemplate);

            var slot = page.GetChild(i % perPage) as RectTransform;
            var p = Instantiate(postItPrefab, slot);
            p.rt.anchoredPosition = Vector2.zero;
            p.Setup(currentRecords[i], category.postItColor, GetRotation(currentRecords[i]), OnPostItClicked);

            listItems.Add(p);
        }

        if (emptyText != null) emptyText.SetActive(currentRecords.Count == 0);
        listPager.GoToPage(0, true);
    }

    // ── 리스트 → 카테고리 ─────────────────────
    private void BackToCategory()
    {
        isAnimating = true;
        SetView(categoryView, true, 0f);
        listView.blocksRaycasts = false;

        var seq = DOTween.Sequence().SetId(tweenId);
        seq.Insert(0f, listView.DOFade(0f, fadeDuration));
        seq.Insert(0f, categoryView.DOFade(1f, fadeDuration));
        seq.OnComplete(() =>
        {
            SetView(listView, false, 1f);
            state = ViewState.Category;
            isAnimating = false;
        });
    }

    // ── 리스트 → 상세 (확대) ──────────────────
    private void OnPostItClicked(RecordPostIt p)
    {
        if (isAnimating || state != ViewState.List) return;
        isAnimating = true;

        detailIndex = currentRecords.IndexOf(p.record);
        ApplyDetailContent(p.record);

        SetView(detailView, true, 1f);
        dimBackground.alpha = 0f;
        detailContent.alpha = 0f;

        // 상세 카드의 중앙 위치를 구한 뒤 포스트잇 자리에서 출발
        detailCard.localRotation = Quaternion.identity;
        detailCard.localScale = Vector3.one;
        detailCard.anchoredPosition = detailHome;
        Vector3 homeWorld = detailCard.position;
        float startScale = WorldWidth(p.rt) / WorldWidth(detailCard);

        detailCard.position = Center(p.rt);
        detailCard.localRotation = Quaternion.Euler(0f, 0f, p.baseRotation);
        detailCard.localScale = Vector3.one * startScale;

        p.group.alpha = 0f;   // 들어올려진 것처럼 원래 자리를 비움

        var seq = DOTween.Sequence().SetId(tweenId);
        seq.Insert(0f, detailCard.DOMove(homeWorld, zoomDuration).SetEase(Ease.OutCubic));
        seq.Insert(0f, detailCard.DOLocalRotate(Vector3.zero, zoomDuration).SetEase(Ease.OutCubic));
        seq.Insert(0f, detailCard.DOScale(1f, zoomDuration).SetEase(Ease.OutCubic));
        seq.Insert(0f, dimBackground.DOFade(1f, zoomDuration));
        seq.Append(detailContent.DOFade(1f, fadeDuration));
        seq.OnComplete(() =>
        {
            state = ViewState.Detail;
            isAnimating = false;
        });
    }

    // ── 상세 → 리스트 (축소) ──────────────────
    private void CloseDetail()
    {
        isAnimating = true;

        // 지금 보고 있는 기록의 포스트잇 자리로 (다른 페이지면 그 페이지로 이동)
        listPager.GoToPage(detailIndex / SlotsPerPage, true);
        RecordPostIt p = listItems[detailIndex];

        float endScale = WorldWidth(p.rt) / WorldWidth(detailCard);

        var seq = DOTween.Sequence().SetId(tweenId);
        seq.Insert(0f, detailContent.DOFade(0f, fadeDuration * 0.5f));
        seq.Insert(0.05f, detailCard.DOMove(Center(p.rt), zoomDuration).SetEase(Ease.InOutCubic));
        seq.Insert(0.05f, detailCard.DOLocalRotate(new Vector3(0f, 0f, p.baseRotation), zoomDuration).SetEase(Ease.InOutCubic));
        seq.Insert(0.05f, detailCard.DOScale(endScale, zoomDuration).SetEase(Ease.InOutCubic));
        seq.Insert(0.05f, dimBackground.DOFade(0f, zoomDuration));
        seq.OnComplete(() =>
        {
            p.group.alpha = 1f;
            SetView(detailView, false, 1f);

            detailCard.anchoredPosition = detailHome;
            detailCard.localRotation = Quaternion.identity;
            detailCard.localScale = Vector3.one;

            state = ViewState.List;
            isAnimating = false;
        });
    }

    // ── 상세에서 좌우 넘기기 ──────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingDetail = state == ViewState.Detail && !isAnimating;
        dragAccum = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggingDetail) return;

        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        dragAccum += eventData.delta.x / scale;

        // 손가락을 반만큼 따라오게 해서 넘기는 느낌만 줌
        detailCard.anchoredPosition = detailHome + new Vector2(dragAccum * 0.5f, 0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!draggingDetail) return;
        draggingDetail = false;

        int dir = 0;
        if (dragAccum <= -swipeThreshold) dir = 1;    // 왼쪽으로 밀면 다음
        else if (dragAccum >= swipeThreshold) dir = -1;   // 오른쪽으로 밀면 이전

        int next = detailIndex + dir;

        // 끝에서는 멈춤
        if (dir == 0 || next < 0 || next >= currentRecords.Count)
        {
            detailCard.DOAnchorPos(detailHome, slideDuration).SetEase(Ease.OutCubic).SetId(tweenId);
            return;
        }

        SlideTo(next, dir);
    }

    private void SlideTo(int next, int dir)
    {
        isAnimating = true;
        float dist = ((RectTransform)detailCard.parent).rect.width;

        var seq = DOTween.Sequence().SetId(tweenId);
        seq.Append(detailCard.DOAnchorPosX(detailHome.x - dir * dist, slideDuration).SetEase(Ease.InCubic));
        seq.AppendCallback(() =>
        {
            // 들어올려진 포스트잇도 바꿔줌
            listItems[detailIndex].group.alpha = 1f;
            listItems[next].group.alpha = 0f;

            detailIndex = next;
            ApplyDetailContent(currentRecords[next]);
            detailCard.anchoredPosition = new Vector2(detailHome.x + dir * dist, detailHome.y);
        });
        seq.Append(detailCard.DOAnchorPosX(detailHome.x, slideDuration).SetEase(Ease.OutCubic));
        seq.OnComplete(() => isAnimating = false);
    }

    private void ApplyDetailContent(RecordData record)
    {
        if (detailBodyText != null) detailBodyText.text = record.body;
        if (detailTitleText != null) detailTitleText.text = record.title;
        if (detailCardImage != null && currentCategory != null)
            detailCardImage.color = currentCategory.postItColor;
    }

    // ── 유틸 ──────────────────────────────────
    private static void SetView(CanvasGroup g, bool active, float alpha)
    {
        if (g == null) return;
        g.gameObject.SetActive(active);
        g.alpha = alpha;
        g.blocksRaycasts = true;
    }

    private static Vector3 Center(RectTransform rt) => rt.TransformPoint(rt.rect.center);

    private static float WorldWidth(RectTransform rt) => rt.rect.width * rt.lossyScale.x;

    // 기록마다 고정된 무작위 각도 (에셋 이름 기준)
    private float GetRotation(RecordData record)
    {
        uint h = 2166136261;
        foreach (char c in record.name)
        {
            h ^= c;
            h *= 16777619;
        }
        float t = (h % 10000) / 9999f;
        return Mathf.Lerp(-maxPostItRotation, maxPostItRotation, t);
    }
}