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

    Renderer _renderer;
    OutlineRegister _outline;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _outline = GetComponent<OutlineRegister>();

        ObjectInventory = new Inventory(Length, Height);
        foreach (Item item in Items)
            ObjectInventory.TryAddItem(item);
    }

    private void OnTriggerEnter(Collider other)
    {
        _outline.enabled = true;
        other.GetComponentInParent<Character>().CurrentLoot = ObjectInventory;
    }

    private void OnTriggerExit(Collider other)
    {
        _outline.enabled = false;
        if (other.GetComponentInParent<Character>().CurrentLoot == ObjectInventory)
            other.GetComponentInParent<Character>().CurrentLoot = null;
    }
}
