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
        ClimbUp,    // 사다리 올라가기
        ClimbDown   // 사다리 내려가기
    }

    public ButtonType buttonType;

    private ToolManager toolManager;
    private Protect protect;
    private bool isHolding = false;

    void Awake()
    {
        if (buttonType == ButtonType.Attack)
        {
            toolManager = FindFirstObjectByType<ToolManager>();
            protect = FindFirstObjectByType<Protect>();
        }
    }

    void Update()
    {
        if (buttonType == ButtonType.Attack
            && isHolding
            && toolManager.currentTool == ToolType.Protect
            && !protect.isBlocking)
        {
            toolManager.OnAttackInput();
        }
    }

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
                if (toolManager.currentTool != ToolType.Protect)
                    toolManager.OnAttackInput();
                break;

            case ButtonType.ClimbUp:
                PlayerUIManager.Instance.UIButtonClimbUpDown();
                break;

            case ButtonType.ClimbDown:
                PlayerUIManager.Instance.UIButtonClimbDownDown();
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
                PlayerUIManager.Instance.UIButtonClimbUp(); // 손 떼면 정지
                break;
        }
    }
}