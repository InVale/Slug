using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Assets/Weapon", order = 21)]
public class WeaponItem : Item
{
    [Title("Weapon")]
    public FireModeFlag FireMode;
    public float Firerate;
    public float Recoil;
    public float SightDistance;

    public AmmoType Ammo;
    public MagazineItem Magazine;
    public float ReloadDuration;

    public override void ItemCreated()
    {
        if (Magazine != null)
            Magazine = Instantiate(Magazine);
    }
}
