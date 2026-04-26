using System;

[Serializable]
public class SaveData
{
    public PlayerEquipmentSaveData equipment;

    // ½½·Ô UI Ç¥½Ã¿ë
    public string missionText;
    public string abilityText;
    public string toolText;

    public int secretFound;
    public int secretTotal;

    public string saveTime;
}