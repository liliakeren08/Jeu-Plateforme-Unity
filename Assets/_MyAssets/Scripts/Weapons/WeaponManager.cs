using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    private List<Weapons> _weapons = new List<Weapons>();

    public void AddWeapon(Weapons weaponPrefab)
    {
        Weapons newWeapon = Instantiate(weaponPrefab, transform);
        newWeapon.Init(GetComponent<Player>());

        _weapons.Add(newWeapon);
    }
}