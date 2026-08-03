using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("고유 ID (스테이지 내에서 중복 불가)")]
    public string collectibleId;

    [Header("이미 획득한 경우")]
    public bool hideIfAlreadyCollected = false;   // true면 재획득 불가

    private bool collected = false;

    private void Start()
    {
        if (hideIfAlreadyCollected
            && StageInfo.Current != null
            && StageInfo.Current.WasCollectedBefore(collectibleId))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        collected = true;
        StageInfo.Current?.OnCollect(collectibleId);
        gameObject.SetActive(false);
    }
}