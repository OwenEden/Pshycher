using UnityEditor.EditorTools;
using UnityEngine;

public class PlayerEquipmentSaver : MonoBehaviour
{
    public WeaponManager weaponManager;
    public ToolManager toolManager;

    public PlayerEquipmentSaveData GetSaveData()
    {
        return new PlayerEquipmentSaveData
        {
            weaponIndex = weaponManager.CurrentIndex,
            toolIndex = toolManager.CurrentIndex
        };
    }

    public void LoadFromData(PlayerEquipmentSaveData data)
    {
        if (data == null) return;

        weaponManager.LoadWeapon(data.weaponIndex);
        toolManager.LoadTool(data.toolIndex);
    }
}