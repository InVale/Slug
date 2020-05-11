using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public int Length;
    public int Height;

    public Item[][] Container;
    public Dictionary<Item, (int column, int row)> Items = new Dictionary<Item, (int column, int row)>();

    public Inventory(int length, int height)
    {
        Length = length;
        Height = height;

        Container = new Item[Length][];
        for (int i = 0; i < Container.Length; i++)
            Container[i] = new Item[Height];
    }

    public bool CheckPosition(int column, int row, (int length, int height) size, Item itemToIgnore = null)
    {
        if (column + size.length > Length || row + size.height > Height)
            return false;

        for (int x = column; x < column + size.length; x++)
            for (int y = row; y < row + size.height; y++)
                if (Container[x][y] != null && Container[x][y] != itemToIgnore)
                    return false;

        return true;
    }

    public void AddItem(int column, int row, Item item)
    {
        for (int x = column; x < column + item.Size.length; x++)
            for (int y = row; y < row + item.Size.height; y++)
                Container[x][y] = item;
        Items.Add(item, (column, row));
        item.currentInventory = this;
    }

    public void RemoveItem(Item item)
    {
        (int column, int row) pivot = Items[item];
        for (int x = pivot.column; x < pivot.column + item.Size.length; x++)
            for (int y = pivot.row; y < pivot.row + item.Size.height; y++)
                Container[x][y] = null;
        Items.Remove(item);
        item.currentInventory = null;
    }

    public bool TryAddItem(Item item)
    {
        for (int y = 0; y < Length; y++)
            for (int x = 0; x < Height; x++)
            {
                if (CheckPosition(x, y, item.Size))
                {
                    AddItem(x, y, item);
                    return true;
                }
                if (CheckPosition(x, y, (item.Height, item.Length)))
                {
                    item.Flip();
                    AddItem(x, y, item);
                    return true;
                }
            }

        return false;
    }
}
