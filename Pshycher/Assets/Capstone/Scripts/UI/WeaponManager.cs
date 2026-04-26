using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    public Transform handPoint;
    public List<WeaponData> weapons = new List<WeaponData>();

    private int currentIndex = 0;
    private Weapon currentWeapon;

    public int CurrentIndex => currentIndex;

    void Start()
    {
        EquipWeapon(currentIndex);
    }

    public void NextWeapon()
    {
        currentIndex++;

        if (currentIndex >= weapons.Count)
            currentIndex = 0;

        EquipWeapon(currentIndex);
    }

    public void LoadWeapon(int index)
    {
        currentIndex = index;

        if (currentIndex >= weapons.Count)
            currentIndex = 0;

        EquipWeapon(currentIndex);
    }

    void EquipWeapon(int index)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);

        GameObject weaponObj = Instantiate(weapons[index].weaponPrefab, handPoint);

        currentWeapon = weaponObj.GetComponent<Weapon>();
        currentWeapon.data = weapons[index];
    }

    public void Attack()
    {
        if (currentWeapon != null)
            currentWeapon.Attack();
    }
    public string GetWeaponText()
    {
        return $"무기 상태 : {weapons[currentIndex].weaponName}";
    }
}