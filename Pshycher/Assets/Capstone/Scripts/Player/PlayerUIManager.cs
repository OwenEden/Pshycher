using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;
    [Header("상하 버튼 (사다리/수영 공용)")]
    public GameObject upButton;
    public GameObject downButton;

    private readonly HashSet<object> verticalButtonRequesters = new HashSet<object>();

    PlayerStat player;   // 데미지 테스트용

    public Vector2 MoveInput { get; private set; }

    bool jumpPressed;
    bool attackPressed;
    bool dashPressed;

    float uiMove;
    float climbInput = 0f;

    private PlayerInput playerInput;
    private ToolManager toolManager;

    void Awake()
    {
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStat>();

        playerInput = FindAnyObjectByType<PlayerInput>();
        SubscribeActions(true);
    }

    void Start()
    {
        toolManager = FindAnyObjectByType<ToolManager>();
        UpdateVerticalButtons();
    }

    void OnDestroy()
    {
        SubscribeActions(false);
    }

    private void SubscribeActions(bool subscribe)
    {
        if (playerInput == null || playerInput.actions == null) return;

        var move = playerInput.actions["Move"];
        var jump = playerInput.actions["Jump"];
        var attack = playerInput.actions["Attack"];
        var pause = playerInput.actions["Pause"];
        var dash = playerInput.actions["Dash"];

        if (subscribe)
        {
            if (move != null)
            {
                move.performed += OnMove;
                move.canceled += OnMove;
            }
            if (jump != null) jump.performed += OnJump;
            if (dash != null) dash.performed += OnDash;
            if (attack != null) attack.performed += OnAttack;
            if (pause != null) pause.performed += OnPause;
        }
        else
        {
            if (move != null)
            {
                move.performed -= OnMove;
                move.canceled -= OnMove;
            }
            if (jump != null) jump.performed -= OnJump;
            if (dash != null) dash.performed -= OnDash;
            if (attack != null) attack.performed -= OnAttack;
            if (pause != null) pause.performed -= OnPause;
        }
    }
    // 사다리·수영이 각자 요청, 하나라도 있으면 표시
    public void RequestVerticalButtons(object requester, bool active)
    {
        if (requester == null) return;

        if (active) verticalButtonRequesters.Add(requester);
        else verticalButtonRequesters.Remove(requester);

        UpdateVerticalButtons();
    }

    private void UpdateVerticalButtons()
    {
        bool show = verticalButtonRequesters.Count > 0;

        if (upButton != null) upButton.SetActive(show);
        if (downButton != null) downButton.SetActive(show);

        // 누른 채로 버튼이 꺼지면 PointerUp이 안 와서 입력이 남음
        if (!show) climbInput = 0f;
    }

    // ── InputAction 콜백 ──────────────────────
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpPressed = true;
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed) dashPressed = true;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        attackPressed = true;
        toolManager?.OnAttackInput();
    }
    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var pause = PauseManager.Instance;
        if (pause == null) return;

        // 같은 키로 열고 닫기
        if (pause.isPaused)
            pause.Resume();
        else
            pause.Pause();
    }
    // ── 조회 ──────────────────────────────────
    public float GetMove() => MoveInput.x + uiMove;

    public bool GetJump()
    {
        bool value = jumpPressed;
        jumpPressed = false;
        return value;
    }
    public bool GetDash()
    {
        bool value = dashPressed;
        dashPressed = false;
        return value;
    }

    public bool GetAttack()
    {
        bool value = attackPressed;
        attackPressed = false;
        return value;
    }

    public float GetClimb() => MoveInput.y + climbInput;

    // ── UI 버튼 입력 ──────────────────────────
    public void UIButtonClimbUpDown() => climbInput = 1f;
    public void UIButtonClimbDownDown() => climbInput = -1f;
    public void UIButtonClimbUp() => climbInput = 0f;

    public void UIButtonLeftDown() => uiMove = -1;
    public void UIButtonRightDown() => uiMove = 1;
    public void UIButtonMoveUp() => uiMove = 0;

    public void UIButtonJump() => jumpPressed = true;
    public void UIButtonDash() => dashPressed = true;
}