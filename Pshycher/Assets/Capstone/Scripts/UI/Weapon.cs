using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData data;

    public void Attack()
    {
        Debug.Log(data.weaponName + " Attack!");
    }
}