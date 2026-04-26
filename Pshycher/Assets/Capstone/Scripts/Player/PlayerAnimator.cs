using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController controller;

    void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        animator.SetBool("isMoving", controller.isSprinting);
        animator.SetBool("isJumping", controller.isjumping);
    }
}
