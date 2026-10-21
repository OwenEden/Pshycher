using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum WheelMode { Tool, Ability }

public class WheelUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("설정")]
    public WheelMode mode = WheelMode.Tool;
    public float activationHoldTime = 0.2f;
    public float selectRadius = 40f;

    [Header("배치")]
    public float wheelRadius = 150f;
    [Tooltip("첫 슬롯 각도 (0 = 오른쪽, 90 = 위, 180 = 왼쪽)")]
    public float startAngle = 90f;
    [Tooltip("슬롯을 펼칠 각도 범위 (360 = 원 전체, 180 = 반원)")]
    [Range(1f, 360f)] public float arcAngle = 360f;
    public bool clockwise = false;

    [Header("선택 판정")]
    public float edgeSlackAngle = 20f;

    [Header("슬롯 (Inspector에서 순서대로 연결)")]
    public GameObject[] slots;
    public Image[] slotHighlights;

    [Header("버튼 아이콘")]
    public Image buttonIcon;            // 버튼에 표시할 아이콘 이미지
    public Sprite defaultSprite;        // 맨손 / 능력 꺼짐일 때
    [Tooltip("슬롯과 같은 순서 (도구: 블레이드~블래스터 / 능력: 철고화~공기참격)")]
    public Sprite[] iconSprites;
    [Range(0f, 1f)] public float inactiveIconAlpha = 0.4f;   // 능력이 꺼져 있을 때 아이콘 투명도

    private Color iconBaseColor;
    private bool iconColorCaptured;

    [Header("잠금 표시")]
    [Tooltip("흑백 머티리얼 (비우면 lockedTint로 어둡게)")]
    public Material lockedMaterial;
    public Color lockedTint = new Color(0.4f, 0.4f, 0.4f, 1f);
    [Range(0f, 1f)] public float unavailableButtonAlpha = 0.35f;

    private ToolManager toolManager;
    private AbilityManager abilityManager;

    private bool isHolding = false;
    private bool wheelOpen = false;
    private float holdTimer = 0f;
    private Vector2 pointerDownPos;
    private int hoveredIndex = -1;

    // 해금 상태
    private bool available = true;           // 해금된 슬롯이 하나라도 있는지
    private bool[] slotUnlocked;
    private CanvasGroup buttonGroup;
    private readonly Dictionary<Image, (Material mat, Color color)> originalLooks
        = new Dictionary<Image, (Material, Color)>();

    private int slotCount => slots != null ? slots.Length : 0;
    private bool IsFullCircle => arcAngle >= 359.9f;

    void Awake()
    {
        toolManager = FindAnyObjectByType<ToolManager>();
        abilityManager = FindAnyObjectByType<AbilityManager>();

        ArrangeSlots();
        CloseWheel();
    }

    void Start()
    {
    // StageInfo·SaveManager가 준비된 뒤 확인
        RefreshAvailability();

        if (mode == WheelMode.Tool && toolManager != null)
            toolManager.OnToolChanged += RefreshIcon;

        if (mode == WheelMode.Ability && abilityManager != null)
            abilityManager.OnAbilityStateChanged += RefreshIcon;

        RefreshIcon();
    }
    void OnDestroy()
    {
        if (toolManager != null) toolManager.OnToolChanged -= RefreshIcon;
        if (abilityManager != null) abilityManager.OnAbilityStateChanged -= RefreshIcon;
    }

    public void RefreshIcon()
    {
        if (buttonIcon == null) return;

        // 처음 한 번 원래 색을 기억 (흐리게 했다가 되돌리기 위해)
        if (!iconColorCaptured)
        {
            iconBaseColor = buttonIcon.color;
            iconColorCaptured = true;
        }

        int index = -1;
        bool dimmed = false;

        if (mode == WheelMode.Tool)
        {
            if (toolManager != null)
            {
                if (toolManager.currentTool != ToolType.None)
                {
                    index = (int)toolManager.currentTool - 1;
                }
                else
                {
                    // 맨손이면 탭했을 때 꺼내질 도구를 흐리게
                    ToolType next = toolManager.NextToggleTool;
                    if (next != ToolType.None)
                    {
                        index = (int)next - 1;
                        dimmed = true;
                    }
                }
            }
        }
        else if (abilityManager != null && abilityManager.currentAbility != AbilityType.None)
        {
            // 골라둔 능력이 있으면 아이콘을 보여주고, 꺼져 있으면 흐리게
            index = (int)abilityManager.currentAbility - 1;
            dimmed = !abilityManager.isAbilityActive;
        }

        bool hasIcon = index >= 0 && iconSprites != null
                    && index < iconSprites.Length && iconSprites[index] != null;

        buttonIcon.sprite = hasIcon ? iconSprites[index] : defaultSprite;

        Color c = iconBaseColor;
        if (hasIcon && dimmed) c.a *= inactiveIconAlpha;
        buttonIcon.color = c;
    }

    // ── 해금 상태 반영 ────────────────────────
    public void RefreshAvailability()
    {
        slotUnlocked = new bool[slotCount];
        available = false;

        for (int i = 0; i < slotCount; i++)
        {
            // 슬롯 i = enum 값 i + 1 (None 제외)
            slotUnlocked[i] = mode == WheelMode.Tool
                ? PlayerUnlocks.IsToolAvailable((ToolType)(i + 1))
                : PlayerUnlocks.IsAbilityAvailable((AbilityType)(i + 1));

            if (slotUnlocked[i]) available = true;
            SetSlotLocked(slots[i], !slotUnlocked[i]);
        }

        // 버튼 자체 - 해금된 게 하나도 없으면 흐리게
        if (buttonGroup == null)
        {
            buttonGroup = GetComponent<CanvasGroup>();
            if (buttonGroup == null) buttonGroup = gameObject.AddComponent<CanvasGroup>();
        }
        buttonGroup.alpha = available ? 1f : unavailableButtonAlpha;

        if (!available && (isHolding || wheelOpen))
        {
            isHolding = false;
            wheelOpen = false;
            CloseWheel();
        }
    }

    private void SetSlotLocked(GameObject slot, bool locked)
    {
        if (slot == null) return;

        foreach (var img in slot.GetComponentsInChildren<Image>(true))
        {
            if (!originalLooks.ContainsKey(img))
                originalLooks[img] = (img.material, img.color);

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
    }

    private bool IsSlotUnlocked(int index)
    {
        return slotUnlocked == null
            || (index >= 0 && index < slotUnlocked.Length && slotUnlocked[index]);
    }

    // ── 배치 ──────────────────────────────────
    [ContextMenu("슬롯 정렬")]
    public void ArrangeSlots()
    {
        if (slotCount == 0) return;

        for (int i = 0; i < slotCount; i++)
        {
            if (slots[i] == null) continue;

            RectTransform rect = slots[i].transform as RectTransform;
            if (rect == null) continue;

            float rad = GetSlotAngle(i) * Mathf.Deg2Rad;
            rect.anchoredPosition = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * wheelRadius;
        }
    }

    private float GetStep()
    {
        if (slotCount <= 1) return 360f;
        return IsFullCircle ? 360f / slotCount : arcAngle / (slotCount - 1);
    }

    private float GetSlotAngle(int index)
    {
        float offset = index * GetStep() * (clockwise ? -1f : 1f);
        return Mathf.Repeat(startAngle + offset, 360f);
    }

    private int GetIndexFromAngle(float angle)
    {
        int best = -1;
        float bestDiff = float.MaxValue;

        for (int i = 0; i < slotCount; i++)
        {
            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, GetSlotAngle(i)));
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }

        float limit = GetStep() * 0.5f + (IsFullCircle ? 0f : edgeSlackAngle);
        return bestDiff <= limit ? best : -1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying) ArrangeSlots();
    }
#endif

    // ── 입력 ──────────────────────────────────
    void Update()
    {
        if (!isHolding) return;

        holdTimer += Time.unscaledDeltaTime;
        if (!wheelOpen && holdTimer >= activationHoldTime)
            OpenWheel();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!available) return;

        isHolding = true;
        holdTimer = 0f;
        pointerDownPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isHolding) return;

        if (!wheelOpen)
        {
            if (mode == WheelMode.Ability)
                abilityManager?.ToggleAbility();
            else
                toolManager?.ToggleTool();
        }
        else if (hoveredIndex >= 0)
        {
            ConfirmSelection(hoveredIndex);
        }

        isHolding = false;
        wheelOpen = false;
        CloseWheel();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!wheelOpen || slotCount == 0) return;

        Vector2 dir = eventData.position - pointerDownPos;

        int index = -1;
        if (dir.magnitude >= selectRadius)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            index = GetIndexFromAngle(angle);

            // 잠긴 슬롯은 선택 불가
            if (index >= 0 && !IsSlotUnlocked(index)) index = -1;
        }

        if (index != hoveredIndex)
        {
            hoveredIndex = index;
            if (index >= 0) UpdateHighlight(index);
            else ClearHighlight();
        }
    }

    // ── 열기/닫기 ─────────────────────────────
    private void OpenWheel()
    {
        wheelOpen = true;
        hoveredIndex = -1;

        foreach (var slot in slots)
            if (slot != null) slot.SetActive(true);
    }

    private void CloseWheel()
    {
        if (slots != null)
            foreach (var slot in slots)
                if (slot != null) slot.SetActive(false);

        ClearHighlight();
    }

    private void ConfirmSelection(int index)
    {
        if (!IsSlotUnlocked(index)) return;

        if (mode == WheelMode.Tool)
            toolManager?.SwitchTool((ToolType)(index + 1));
        else
            abilityManager?.SelectAndActivate((AbilityType)(index + 1));
    }

    private void UpdateHighlight(int index)
    {
        ClearHighlight();
        if (index >= 0 && index < slotHighlights.Length && slotHighlights[index] != null)
            slotHighlights[index].color = new Color(1f, 1f, 1f, 0.9f);
    }

    private void ClearHighlight()
    {
        if (slotHighlights == null) return;
        foreach (var h in slotHighlights)
            if (h != null) h.color = new Color(1f, 1f, 1f, 0.5f);
    }
}