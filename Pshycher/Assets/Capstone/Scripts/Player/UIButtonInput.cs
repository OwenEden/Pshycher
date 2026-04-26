using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ButtonType
    {
        Left,
        Right,
        Jump
    }

    public ButtonType buttonType;

    public void OnPointerDown(PointerEventData eventData)
    {
        switch (buttonType)
        {
            case ButtonType.Left:
                InputManager.Instance.UIButtonLeftDown();
                break;

            case ButtonType.Right:
                InputManager.Instance.UIButtonRightDown();
                break;

            case ButtonType.Jump:
                InputManager.Instance.UIButtonJump();
                break;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonType == ButtonType.Left || buttonType == ButtonType.Right)
        {
            InputManager.Instance.UIButtonMoveUp();
        }
    }
}