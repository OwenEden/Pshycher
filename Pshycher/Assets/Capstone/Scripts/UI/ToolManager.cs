using UnityEngine;
using System.Collections.Generic;

public class ToolManager : MonoBehaviour
{
    public Transform handPoint;
    public List<ToolData> tools = new List<ToolData>();

    private int currentIndex = 0;
    private Tool currentTool;

    public int CurrentIndex => currentIndex;

    void Start()
    {
        EquipTool(currentIndex);
    }

    public void NextTool()
    {
        currentIndex++;

        if (currentIndex >= tools.Count)
            currentIndex = 0;

        EquipTool(currentIndex);
    }

    public void LoadTool(int index)
    {
        currentIndex = index;

        if (currentIndex >= tools.Count)
            currentIndex = 0;

        EquipTool(currentIndex);
    }

    void EquipTool(int index)
    {
        if (currentTool != null)
            Destroy(currentTool.gameObject);

        GameObject toolObj = Instantiate(tools[index].toolPrefab, handPoint);

        currentTool = toolObj.GetComponent<Tool>();
        currentTool.data = tools[index];
    }

    public void UseTool()
    {
        if (currentTool != null)
            currentTool.Use();
    }
    public string GetToolText()
    {
        return $"도구 상태 : {tools[currentIndex].toolName}";
    }
}