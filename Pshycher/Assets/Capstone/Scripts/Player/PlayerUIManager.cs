using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;

        PlayerStat player; //데미지 테스트용

    public Vector2 MoveInput { get; private set; }

    bool jumpPressed;

    float uiMove;

    // 사다리 입력 상태
    private float climbInput = 0f;

    // PlayerController에서 호출
    void Awake()
    {
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStat>();//데미지 테스트용
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
        Debug.Log($"Move Input: {MoveInput}");
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

    public float GetClimb() => MoveInput.y + climbInput;

    // UI BUTTON INPUT
    public void UIButtonClimbUpDown() => climbInput = 1f;
    public void UIButtonClimbDownDown() => climbInput = -1f;
    public void UIButtonClimbUp() => climbInput = 0f;

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