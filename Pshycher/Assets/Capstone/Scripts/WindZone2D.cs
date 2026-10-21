using UnityEngine;

public class WindZone2D : MonoBehaviour
{
    [Header("바람 설정")]
    public Vector2 windDirection = Vector2.left;
    public float windForce = 6f;

    [Header("면역 능력")]
    public AbilityType immuneAbility = AbilityType.IronHardening;

    public Vector2 GetWindVelocity() => windDirection.normalized * windForce;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.EnterWindZone(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>()?.ExitWindZone(this);
    }

    // 현재 능력이 이 바람을 무시하는지
    public bool IsImmune(AbilityManager ability)
    {
        if (ability == null) return false;
        return ability.isAbilityActive && ability.currentAbility == immuneAbility;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (Vector3)windDirection.normalized * 2f);
    }
}