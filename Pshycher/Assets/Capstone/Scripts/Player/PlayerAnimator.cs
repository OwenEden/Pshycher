using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController controller;
    private Rigidbody2D rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        animator.SetBool("isMoving", controller.isSprinting);
        animator.SetBool("isJumping", controller.isJumping);
       // animator.SetBool("isGrounded", controller.isGrounded);
       // animator.SetFloat("verticalVelocity", rb.linearVelocity.y);
        animator.SetBool("isOnLadder", controller.isOnLadder);
        animator.SetBool("isClimbing", controller.isClimbing);

        HandleClimbAnimation();
    }
    private void HandleClimbAnimation()
    {
        if (!controller.isOnLadder)
        {
            // 사다리 벗어나면 애니메이션 속도 복구
            animator.speed = 1f;
            return;
        }

        // isClimbing이면 재생, 아니면 정지
        animator.speed = controller.isClimbing ? 1f : 0f;
    }
    public void PlayAttackAnim(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.Blade: animator.SetTrigger("attackBlade"); break;
            case ToolType.Buster: animator.SetTrigger("attackBuster"); break;
            case ToolType.Rope: animator.SetTrigger("attackRope"); break;
            case ToolType.Protect: animator.SetTrigger("protect"); break;
            case ToolType.Blaster: animator.SetTrigger("attackBlaster"); break;
        }
    }

    public void PlayAbilityAnim(AbilityType ability)
    {
        animator.SetTrigger("abilityActivate");
        animator.SetInteger("currentAbility", (int)ability);
    }
}