using Knife.PostProcessing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInventory : MonoBehaviour
{
    public int Length;
    public int Height;
    public Item[] Items;

    public Inventory ObjectInventory;
    public OutlineRegister ObjectToOutline;

    Renderer _renderer;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        ObjectToOutline = GetComponent<OutlineRegister>();

        ObjectInventory = new Inventory(Length, Height);
        foreach (Item item in Items)
            ObjectInventory.TryAddItem(item);
    }

    private void OnTriggerEnter(Collider other)
    {
        Character character = other.GetComponentInParent<Character>();
        if (character)
        {
            ObjectToOutline.enabled = true;
            character.CurrentLoot = ObjectInventory;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponentInParent<Character>();
        if (character)
        {
            ObjectToOutline.enabled = false;
            if (character.CurrentLoot == ObjectInventory)
                character.CurrentLoot = null;
        }
    }
}
