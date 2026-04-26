using UnityEngine;

public class Tool : MonoBehaviour
{
    public ToolData data;

    public void Use()
    {
        Debug.Log(data.toolName + " Used!");
    }
}