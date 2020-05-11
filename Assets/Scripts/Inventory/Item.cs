using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Assets/Item", order = 1)]
public class Item : ScriptableObject
{
    public string ItemName;
    public Sprite Image;
    public Color UIColor;
    [Space]
    public int Length;
    public int Height;

    [Title("Debug")]
    public bool Flipped;

    public (int length, int height) Size => (Flipped) ? (Height, Length) : (Length, Height);

    [NonSerialized] public RectTransform UI;
    [NonSerialized] public Inventory currentInventory;

    public void Flip()
    {
        Flipped = !Flipped;
    }

    public virtual void ItemCreated() { }
}
