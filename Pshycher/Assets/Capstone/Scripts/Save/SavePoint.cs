using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [Header("식별")]
    public string checkpointId = "cp_1";   // 스테이지 내에서 고유

    [Header("리스폰 위치")]
    [Tooltip("비워두면 이 오브젝트 위치 사용")]
    public Transform respawnPoint;
    public bool faceRight = true;

    [Header("연출")]
    public GameObject activatedEffect;     // 활성화 시 켜질 오브젝트
    public bool onlyOncePerVisit = true;   // 한 번 활성화되면 재발동 안 함

    private bool activated = false;

    public Vector2 GetRespawnPosition()
    {
        return respawnPoint != null ? respawnPoint.position : transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (onlyOncePerVisit && activated) return;

        activated = true;
        CheckpointManager.Instance?.Activate(this);

        if (activatedEffect != null)
            activatedEffect.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GetRespawnPosition(), 0.3f);
    }
}