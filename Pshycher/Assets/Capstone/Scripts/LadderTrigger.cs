using UnityEngine;
using System.Collections.Generic;

public class LadderTrigger : MonoBehaviour
{
    [Header("레이어 설정")]
    public string playerLayerName = "Player";
    public string ladderTopLayerName = "LadderTop";

    public float centerX { get; private set; }

    private PlayerController playerInRange = null;
    private int playerLayer;
    private int ladderTopLayer;

    // 개별 무시한 콜라이더 목록 저장
    private List<Collider2D> ignoredColliders = new List<Collider2D>();

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer(playerLayerName);
        ladderTopLayer = LayerMask.NameToLayer(ladderTopLayerName);
        centerX = transform.position.x;
    }

    private void Update()
    {
        if (playerInRange == null) return;
        if (playerInRange.isOnLadder) return;

        float climbInput = PlayerUIManager.Instance.GetClimb();
        if (climbInput != 0f)
        {
            SetCollisionWithLadderTop(false);
            playerInRange.OnEnterLadder(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = other.GetComponent<PlayerController>();
        SetButtonsActive(true);
        Debug.Log($"사다리 진입 - 중앙 X: {centerX}");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInRange == null)
        {
            playerInRange = other.GetComponent<PlayerController>();
            SetButtonsActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInRange != null && playerInRange.isOnLadder)
            playerInRange.OnExitLadder();

        playerInRange = null;
        SetButtonsActive(false);
        Debug.Log("사다리 이탈");
    }

    public void SetCollisionWithLadderTop(bool enabled)
    {
        if (playerLayer == -1 || ladderTopLayer == -1)
        {
            Debug.LogWarning("Player 또는 LadderTop 레이어를 찾을 수 없습니다.");
            return;
        }

        Physics2D.IgnoreLayerCollision(playerLayer, ladderTopLayer, !enabled);

        if (!enabled)
            IgnoreOverlappingLadderTop();
        else
            RestoreIgnoredColliders(); // 충돌 복구 시 개별 무시도 함께 복구

        Debug.Log($"Player-LadderTop 충돌: {(enabled ? "활성화" : "비활성화")}");
    }

    private void IgnoreOverlappingLadderTop()
    {
        if (playerInRange == null) return;

        Collider2D playerCollider = playerInRange.GetComponent<Collider2D>();
        if (playerCollider == null) return;

        ignoredColliders.Clear();

        Collider2D[] ladderTopColliders = Physics2D.OverlapAreaAll(
            playerCollider.bounds.min,
            playerCollider.bounds.max,
            1 << ladderTopLayer
        );

        foreach (var col in ladderTopColliders)
        {
            Physics2D.IgnoreCollision(playerCollider, col, true);
            ignoredColliders.Add(col); // 목록에 저장
            Debug.Log($"LadderTop 개별 충돌 무시: {col.name}");
        }
    }

    private void RestoreIgnoredColliders()
    {
        if (playerInRange == null) return;

        Collider2D playerCollider = playerInRange.GetComponent<Collider2D>();
        if (playerCollider == null) return;

        foreach (var col in ignoredColliders)
        {
            if (col != null)
            {
                Physics2D.IgnoreCollision(playerCollider, col, false); // 개별 충돌 복구
                Debug.Log($"LadderTop 개별 충돌 복구: {col.name}");
            }
        }

        ignoredColliders.Clear();
    }

    public void SetButtonsActive(bool active)
    {
        PlayerUIManager.Instance?.RequestVerticalButtons(this, active);
    }
}