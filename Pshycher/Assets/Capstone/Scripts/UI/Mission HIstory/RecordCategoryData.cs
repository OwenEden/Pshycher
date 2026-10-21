using UnityEngine;

[CreateAssetMenu(fileName = "RecordCategory", menuName = "Game/Record Category")]
public class RecordCategoryData : ScriptableObject
{
    public string displayName = "정규 실험에서의 비밀";
    public Color postItColor = new Color(0.78f, 0.71f, 0.15f);
}