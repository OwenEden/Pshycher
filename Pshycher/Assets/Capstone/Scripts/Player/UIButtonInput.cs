using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType
    {
        Left,
        Right,
        Jump,
        Attack,
        ClimbUp,
        ClimbDown,
        Pause,
        Dash
    }

    public ButtonType buttonType;

    [Header("대쉬 버튼 전용")]
    [Range(0f, 1f)] public float dashDisabledAlpha = 0.35f;

    // 공격
    private ToolManager toolManager;
    private Protect protect;
    private bool isHolding = false;

    // 대쉬
    private PlayerController controller;
    private AbilityManager abilityManager;
    private CanvasGroup group;

    void Awake()
    {
        switch (buttonType)
        {
            case ButtonType.Attack:
                toolManager = FindAnyObjectByType<ToolManager>();
                // 프로텍트는 장착 전 비활성 상태라 Include로 찾아야 함
                protect = FindAnyObjectByType<Protect>(FindObjectsInactive.Include);
                break;

            case ButtonType.Dash:
                controller = FindAnyObjectByType<PlayerController>();
                abilityManager = FindAnyObjectByType<AbilityManager>();
                group = GetComponent<CanvasGroup>();
                if (group == null) group = gameObject.AddComponent<CanvasGroup>();
                break;
        }
    }

    void Start()
    {
        if (buttonType != ButtonType.Dash) return;

        if (abilityManager != null)
            abilityManager.OnAbilityStateChanged += RefreshDash;

        RefreshDash();
    }

    void OnDestroy()
    {
        if (abilityManager != null)
            abilityManager.OnAbilityStateChanged -= RefreshDash;
    }

    void Update()
    {
        // 프로텍트 선택 중 누르고 있으면 지속 발동
        if (buttonType == ButtonType.Attack && isHolding
            && toolManager != null && protect != null
            && toolManager.currentTool == ToolType.Protect
            && !protect.isBlocking)
        {
            toolManager.OnAttackInput();
        }
    }

    // ── 대쉬 버튼 활성화 ──────────────────────
    // 어떤 능력이 필요한지는 PlayerController가 정함
    private void RefreshDash()
    {
        bool on = controller != null && controller.IsDashAbilityActive;

        group.alpha = on ? 1f : dashDisabledAlpha;
        group.interactable = on;
        group.blocksRaycasts = on;   // 꺼져 있으면 터치 자체를 받지 않음
    }

    // ── 입력 ──────────────────────────────────
    public void OnPointerDown(PointerEventData eventData)
    {
        switch (buttonType)
        {
            case ButtonType.Left:
                PlayerUIManager.Instance.UIButtonLeftDown();
                break;

            case ButtonType.Right:
                PlayerUIManager.Instance.UIButtonRightDown();
                break;

            case ButtonType.Jump:
                PlayerUIManager.Instance.UIButtonJump();
                break;

            case ButtonType.Attack:
                isHolding = true;
                if (toolManager != null && toolManager.currentTool != ToolType.Protect)
                    toolManager.OnAttackInput();
                break;

            case ButtonType.ClimbUp:
                PlayerUIManager.Instance.UIButtonClimbUpDown();
                break;

            case ButtonType.ClimbDown:
                PlayerUIManager.Instance.UIButtonClimbDownDown();
                break;

            case ButtonType.Pause:
                PauseManager.Instance?.Pause();
                break;

            case ButtonType.Dash:
                PlayerUIManager.Instance.UIButtonDash();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        switch (buttonType)
        {
            case ButtonType.Left:
            case ButtonType.Right:
                PlayerUIManager.Instance.UIButtonMoveUp();
                break;

            case ButtonType.Attack:
                isHolding = false;
                break;

            case ButtonType.ClimbUp:
            case ButtonType.ClimbDown:
                PlayerUIManager.Instance.UIButtonClimbUp();
                break;
        }
    }
}