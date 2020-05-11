using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public float CursorTickrate;
    [Space]
    public float BordersSize;
    public float LinesSize;
    public float SquaresSize;

    [Title("Debug")]
    [ReadOnly] public int InventoryIndex;
    [ReadOnly] public Vector2 CursorIndex;
    [ReadOnly] public bool InMenu;

    [Title("References")]
    public GameObject LinesPrefab;
    public GameObject ItemPrefab;
    [Space]
    public GameObject Holder;
    public GameObject LootHolder;
    public RectTransform PlayerInventoryBackground;
    public RectTransform LootInventoryBackground;
    public RectTransform Cursor;

    public Inventory PlayerInventory;
    public Inventory LootInventory;

    List<GameObject> _spawnedObjects = new List<GameObject>();
    bool _pressed;
    float _lastInputTimestamp;
    Item _pickedUpItem;
    bool _pickedFlip;

    private void Awake()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }

    void Start()
    {
        Cursor.sizeDelta = Vector2.one * SquaresSize;
    }

    public void RenderInventories(Inventory player, Inventory loot)
    {
        PlayerInventory = player;
        LootInventory = loot;

        foreach (GameObject obj in _spawnedObjects)
            Destroy(obj);
        _spawnedObjects.Clear();

        DrawInventory(player, PlayerInventoryBackground);
        if (loot != null)
            DrawInventory(loot, LootInventoryBackground);
        LootHolder.SetActive((loot != null));

        InventoryIndex = (loot != null) ? 1 : 0;
        CursorIndex = Vector2.zero;

        Holder.SetActive(true);
        InMenu = true;
    }

    void DrawInventory(Inventory inventory, RectTransform render)
    {
        render.sizeDelta = new Vector2(inventory.Length, inventory.Height) * SquaresSize + Vector2.one * BordersSize * 2;
        for(int i = 1; i < inventory.Length; i++)
        {
            RectTransform line = Instantiate(LinesPrefab, render).GetComponent<RectTransform>();
            _spawnedObjects.Add(line.gameObject);
            line.anchorMin = new Vector2(0.5f, 0f);
            line.anchorMax = new Vector2(0.5f, 1f);
            line.sizeDelta = new Vector2(LinesSize, 0);
            line.anchoredPosition = Vector3.right * (-((float)inventory.Length / 2) + i) * SquaresSize;
        }
        for (int i = 1; i < inventory.Height; i++)
        {
            RectTransform line = Instantiate(LinesPrefab, render).GetComponent<RectTransform>();
            _spawnedObjects.Add(line.gameObject);
            line.anchorMin = new Vector2(0f, 0.5f);
            line.anchorMax = new Vector2(1f, 0.5f);
            line.sizeDelta = new Vector2(0, LinesSize);
            line.anchoredPosition = Vector3.up * (-((float)inventory.Height / 2) + i) * SquaresSize;
        }

        foreach(Item item in inventory.Items.Keys)
        {
            RectTransform itemRender = Instantiate(ItemPrefab, render).GetComponent<RectTransform>();
            _spawnedObjects.Add(itemRender.gameObject);
            item.UI = itemRender;
            (int x, int y) pos = item.Size;
            itemRender.sizeDelta = new Vector2(pos.x, pos.y) * SquaresSize;
             pos = inventory.Items[item];
            itemRender.anchoredPosition = new Vector3(1, -1, 0) * BordersSize + new Vector3(pos.x, -pos.y, 0) * SquaresSize;
            itemRender.GetComponent<ItemUI>().Setup(item, SquaresSize);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            InMenu = !InMenu;
            Holder.SetActive(InMenu);
            if (InMenu)
            {
                RenderInventories(Character.Instance.PlayerInventory, null);
            }
        }

        if (InMenu)
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            bool pressed = input != Vector2.zero;

            if (pressed)
            {
                if (!_pressed || Time.time >= _lastInputTimestamp + (1 / CursorTickrate))
                {
                    if (input.x != 0)
                    {
                        _pressed = pressed;
                        _lastInputTimestamp = Time.time;
                        float move = CursorIndex.x + Mathf.Sign(input.x);
                        if (LootInventory != null)
                        {
                            if (move < 0 && InventoryIndex > 0)
                            {
                                InventoryIndex--;
                                move = PlayerInventory.Length - 1;
                            }
                            else if (move > PlayerInventory.Length - 1 && InventoryIndex < 1)
                            {
                                InventoryIndex++;
                                move = 0;
                            }
                        }
                        else
                            move = Mathf.Clamp(move, 0, PlayerInventory.Length - 1);
                        CursorIndex.x = Mathf.Clamp(move, 0, ((InventoryIndex == 0) ? PlayerInventory : LootInventory).Length - 1);
                    }
                    else if (input.y != 0)
                    {
                        _pressed = pressed;
                        _lastInputTimestamp = Time.time;
                        float move = CursorIndex.y - Mathf.Sign(input.y);
                        CursorIndex.y = Mathf.Clamp(move, 0, ((InventoryIndex == 0) ? PlayerInventory : LootInventory).Height - 1);
                    }
                }
            }

            RectTransform inv = (InventoryIndex == 0) ? PlayerInventoryBackground : LootInventoryBackground;
            Cursor.SetParent(inv);
            Cursor.localPosition = new Vector3(1, -1, 0) * BordersSize + new Vector3(CursorIndex.x, -CursorIndex.y, 0) * SquaresSize;

            Inventory inventory = (InventoryIndex == 0) ? PlayerInventory : LootInventory;
            if (_pickedUpItem == null)
            {
                Item item = inventory.Container[Mathf.RoundToInt(CursorIndex.x)][Mathf.RoundToInt(CursorIndex.y)];
                if (item != null)
                {
                    if (Input.GetButtonDown("Submit"))
                    {
                        (int x, int y) pos = inventory.Items[item];
                        CursorIndex = new Vector2(pos.x, pos.y);
                        Cursor.localPosition = new Vector3(1, -1, 0) * BordersSize + new Vector3(CursorIndex.x, -CursorIndex.y, 0) * SquaresSize;

                        _pickedUpItem = item;
                        Cursor.sizeDelta = new Vector2(item.Size.length, item.Size.height) * SquaresSize;
                        item.UI.SetParent(Cursor.GetChild(0));
                        item.UI.GetComponent<ItemUI>().Pickup();
                        _pickedFlip = item.Flipped;
                    }
                }
            }
            else
            {
                if (Input.GetButtonDown("Submit"))
                {
                    if (inventory.CheckPosition(Mathf.RoundToInt(CursorIndex.x), Mathf.RoundToInt(CursorIndex.y), _pickedUpItem.Size, _pickedUpItem))
                    {
                        bool flippedState = _pickedUpItem.Flipped;
                        _pickedUpItem.Flipped = _pickedFlip;
                        _pickedUpItem.currentInventory.RemoveItem(_pickedUpItem);
                        _pickedUpItem.Flipped = flippedState;
                        inventory.AddItem(Mathf.RoundToInt(CursorIndex.x), Mathf.RoundToInt(CursorIndex.y), _pickedUpItem);
                        _pickedUpItem.UI.SetParent(inv);
                        Cursor.sizeDelta = Vector2.one * SquaresSize;
                        _pickedUpItem.UI.GetComponent<ItemUI>().Drop();
                        _pickedUpItem = null;
                        Cursor.SetParent(null);
                    }
                }
                else if (Input.GetButtonDown("Cancel"))
                {
                    RectTransform itemInv = (LootInventory == _pickedUpItem.currentInventory) ? LootInventoryBackground : PlayerInventoryBackground;
                    Destroy(_pickedUpItem.UI.gameObject);

                    _pickedUpItem.Flipped = _pickedFlip;
                    RectTransform itemRender = Instantiate(ItemPrefab, itemInv).GetComponent<RectTransform>();
                    _spawnedObjects.Add(itemRender.gameObject);
                    _pickedUpItem.UI = itemRender;
                    (int x, int y) pos = _pickedUpItem.Size;
                    itemRender.sizeDelta = new Vector2(pos.x, pos.y) * SquaresSize;
                    pos = _pickedUpItem.currentInventory.Items[_pickedUpItem];
                    itemRender.anchoredPosition = new Vector3(1, -1, 0) * BordersSize + new Vector3(pos.x, -pos.y, 0) * SquaresSize;
                    itemRender.GetComponent<ItemUI>().Setup(_pickedUpItem, SquaresSize);


                    Cursor.SetParent(null);
                    Cursor.sizeDelta = Vector2.one * SquaresSize;
                    _pickedUpItem = null;
                }
                else if (Input.GetButtonDown("Right Bumper"))
                {
                    _pickedUpItem.Flip();
                    (int x, int y) pos = _pickedUpItem.Size;
                    _pickedUpItem.UI.sizeDelta = new Vector2(pos.x, pos.y) * SquaresSize;
                    Cursor.sizeDelta = new Vector2(_pickedUpItem.Size.length, _pickedUpItem.Size.height) * SquaresSize;
                }
            }
        }
    }
}
