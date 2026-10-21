using System;
using UnityEngine;
using UnityEngine.UI;

public class ControlLayoutEditor : MonoBehaviour
{
    public static ControlLayoutEditor Current { get; private set; }

    [Serializable]
    public class PadScaleSlider
    {
        public string groupId;
        public Slider slider;
    }

    [Header("편집할 조작 UI 프리팹 (스테이지에서 쓰는 것과 동일)")]
    public ControlLayoutRoot controlUIPrefab;

    [Header("복제본이 놓일 영역 (전체 화면 Stretch)")]
    public RectTransform editArea;

    [Header("체크박스")]
    public Toggle swapToggle;      // 방향패드 ↔ 기능패드 반전
    public Toggle mirrorToggle;    // 기능패드 버튼 배치 반전

    [Header("크기 슬라이더")]
    public PadScaleSlider directionPadScale = new PadScaleSlider { groupId = "dpad" };
    public PadScaleSlider actionPadScale = new PadScaleSlider { groupId = "actionPad" };

    [Header("버튼")]
    public Button saveButton;
    public Button resetButton;
    public Button cancelButton;

    [Tooltip("타이틀·로비에서 보여줄 배경. 일시정지 중에는 숨겨 스테이지가 보이게 함")]
    public GameObject backgroundImage;

    public event Action onClosed;

    private ControlLayoutRoot editCopy;
    private bool bound;

    void Awake()
    {
        Current = this;

        saveButton?.onClick.AddListener(OnSave);
        resetButton?.onClick.AddListener(OnReset);
        cancelButton?.onClick.AddListener(Close);
    }

    void Start()
    {
        bool paused = PauseManager.Instance != null && PauseManager.Instance.isPaused;
        if (backgroundImage != null) backgroundImage.SetActive(!paused);

        CreateEditCopy();
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
        onClosed?.Invoke();   // 저장·취소·씬 전환 등 어떤 경로로 닫혀도 호출됨
    }

    // ── 복제본 생성 ───────────────────────────
    private void CreateEditCopy()
    {
        // 비활성 부모 아래에 만들어 Awake가 돌기 전에 런타임 입력 스크립트 제거
        var holder = new GameObject("EditHolder", typeof(RectTransform));
        var holderRect = (RectTransform)holder.transform;
        holderRect.SetParent(editArea, false);
        holderRect.anchorMin = Vector2.zero;
        holderRect.anchorMax = Vector2.one;
        holderRect.offsetMin = Vector2.zero;
        holderRect.offsetMax = Vector2.zero;
        holder.SetActive(false);

        editCopy = Instantiate(controlUIPrefab, holderRect);
        StripRuntimeScripts(editCopy.gameObject);
        editCopy.MarkAsEditCopy();
        editCopy.onApplied += OnCopyApplied;

        holder.SetActive(true);
    }

    private void StripRuntimeScripts(GameObject go)
    {
        foreach (var w in go.GetComponentsInChildren<WheelUI>(true))
        {
            if (w.slots != null)
                foreach (var s in w.slots) if (s != null) s.SetActive(false);
            DestroyImmediate(w);
        }

        foreach (var c in go.GetComponentsInChildren<UIButtonInput>(true))
            DestroyImmediate(c);

        foreach (var c in go.GetComponentsInChildren<PlayerUIManager>(true))
            DestroyImmediate(c);

        // 누르면 실행되는 기능이 없도록
        foreach (var s in go.GetComponentsInChildren<Selectable>(true))
            s.enabled = false;
    }

    // ── 바인딩 ────────────────────────────────
    // 복제본이 저장값을 적용한 뒤 호출됨
    private void OnCopyApplied()
    {
        if (!bound)
        {
            Bind(directionPadScale);
            Bind(actionPadScale);

            swapToggle?.onValueChanged.AddListener(OnSwapChanged);
            mirrorToggle?.onValueChanged.AddListener(OnMirrorChanged);

            bound = true;
        }
        SyncAll();
    }

    private void Bind(PadScaleSlider s)
    {
        if (s.slider == null) return;

        var group = editCopy.GetGroup(s.groupId);
        s.slider.minValue = editCopy.minScale;
        s.slider.maxValue = editCopy.maxScale;
        s.slider.interactable = group != null;

        if (group == null)
        {
            Debug.LogWarning($"'{s.groupId}' 패드를 찾을 수 없습니다.");
            return;
        }

        // 위치(x, y)는 유지한 채 크기만 변경
        s.slider.onValueChanged.AddListener(v => group.Apply(group.x, group.y, v));
    }

    // ── 동기화 ────────────────────────────────
    private void SyncAll()
    {
        Sync(directionPadScale);
        Sync(actionPadScale);
        swapToggle?.SetIsOnWithoutNotify(editCopy.swapSides);
        mirrorToggle?.SetIsOnWithoutNotify(editCopy.mirrorContents);
    }

    private void Sync(PadScaleSlider s)
    {
        if (s.slider == null) return;

        var group = editCopy.GetGroup(s.groupId);
        if (group != null)
            s.slider.SetValueWithoutNotify(group.scale);
    }

    // ── 체크박스 ──────────────────────────────
    private void OnSwapChanged(bool on) => editCopy.SetSwapSides(on);

    private void OnMirrorChanged(bool on) => editCopy.SetMirrorContents(on);

    // ── 버튼 ──────────────────────────────────
    private void OnSave()
    {
        editCopy.SaveToSettings();   // 실제 조작 UI에도 자동 반영
        Close();
    }

    private void OnReset()
    {
        editCopy.ResetToDefault();
        SyncAll();
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}