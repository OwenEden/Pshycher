using UnityEngine;

public class FallZone : MonoBehaviour
{
    [Header("추락 피해")]
    [Range(0f, 1f)]
    public float healthLossRate = 0.2f;   // 최대 체력의 20%

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CheckpointManager.Instance?.OnPlayerFall(other.gameObject, healthLossRate);
    }
}