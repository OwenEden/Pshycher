using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    Player player; //데미지 테스트용

    public Vector2 MoveInput { get; private set; }

    bool jumpPressed;

    float uiMove;

    void Awake()
    {
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();//데미지 테스트용
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))//데미지 테스트용
        {
            player.OnDamage(10f);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            jumpPressed = true;
    }

    public float GetMove()
    {
        return MoveInput.x + uiMove;
    }

    public bool GetJump()
    {
        bool value = jumpPressed;
        jumpPressed = false;
        return value;
    }

    // UI BUTTON INPUT

    public void UIButtonLeftDown()
    {
        uiMove = -1;
    }

    public void UIButtonRightDown()
    {
        uiMove = 1;
    }

    public void UIButtonMoveUp()
    {
        uiMove = 0;
    }

    public void UIButtonJump()
    {
        jumpPressed = true;
    }
}