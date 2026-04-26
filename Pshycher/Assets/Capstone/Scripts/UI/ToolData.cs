using UnityEngine;

[CreateAssetMenu(menuName = "Game/Tool")]
public class ToolData : ScriptableObject
{
    public string toolName;
    public GameObject toolPrefab;
}