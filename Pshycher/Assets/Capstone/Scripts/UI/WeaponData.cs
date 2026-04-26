using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int damage;
    public float attackSpeed;

    public GameObject weaponPrefab;
}