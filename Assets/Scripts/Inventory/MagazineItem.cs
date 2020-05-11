using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AmmoType
{
    Pistol,
    Rifle
}

[CreateAssetMenu(fileName = "Magazine", menuName = "Assets/Magazine", order = 21)]
public class MagazineItem : Item, IConsumableItem
{
    [Title("Magazine")]
    public int MagazineCapacity;
    public AmmoType Ammo;
    public int AmmoCount;

    public int CurrentValue => AmmoCount;
    public int MaxValue => MagazineCapacity;

    public void PackAmmo ()
    {
        //Not implemented
    }
}
