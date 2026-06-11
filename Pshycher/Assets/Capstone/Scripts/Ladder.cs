using UnityEngine;

public class Ladder : MonoBehaviour
{
    [Header("상/하 버튼 UI")]
    public GameObject upButton;
    public GameObject downButton;

    [Header("레이어 설정")]
    public string playerLayerName = "Player";
    public string groundLayerName = "Ground";

    private PlayerController playerInRange = null;

    private void Update()
    {
        if (playerInRange == null) return;
        if (playerInRange.isOnLadder) return;

        float climbInput = PlayerUIManager.Instance.GetClimb();

        if (climbInput != 0f)
            playerInRange.OnEnterLadder(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = other.GetComponent<PlayerController>();
        SetButtonsActive(true);
        Debug.Log("사다리 범위 진입 - 상/하 버튼 활성화");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (playerInRange != null && playerInRange.isOnLadder)
            playerInRange.OnExitLadder();

        playerInRange = null;
        SetButtonsActive(false);
        Debug.Log("사다리 범위 이탈 - 상/하 버튼 비활성화");
    }

    public void SetCollisionWithGround(bool enabled)
    {
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int groundLayer = LayerMask.NameToLayer(groundLayerName);

        if (playerLayer == -1 || groundLayer == -1)
        {
            Debug.LogWarning("Player 또는 Ground 레이어를 찾을 수 없습니다. 레이어 이름을 확인해주세요.");
            return;
        }

        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, !enabled);
        Debug.Log($"Player-Ground 충돌: {(enabled ? "활성화" : "비활성화")}");
    }

    public void SetButtonsActive(bool active)
    {
        if (upButton != null) upButton.SetActive(active);
        if (downButton != null) downButton.SetActive(active);
    }
}