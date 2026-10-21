using UnityEngine;

public class StartPoint : MonoBehaviour
{
    [Header("바라볼 방향")]
    public bool faceRight = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Vector3 dir = faceRight ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, dir * 0.8f);
    }
}