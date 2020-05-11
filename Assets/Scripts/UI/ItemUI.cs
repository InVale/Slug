using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [Title("Reference")]
    public Image ItemImage;
    public Image[] Borders;
    public TextMeshProUGUI Counter;
    public TextMeshProUGUI Title;

    Item _item;
    Image _background;
    IConsumableItem _consumable;
    float _backgroundAlpha;

    public void Setup (Item item, float squaresSize)
    {
        ItemImage.sprite = item.Image;
        ItemImage.rectTransform.sizeDelta = new Vector2(item.Length, item.Height) * squaresSize;
        _item = item;
        _background = GetComponent<Image>();
        _consumable = item as IConsumableItem;
        Counter.gameObject.SetActive(_consumable != null);
        Title.text = item.ItemName;

        _backgroundAlpha = _background.color.a;
        ItemImage.color = item.UIColor;
        foreach (Image border in Borders)
            border.color = item.UIColor;
    }

    private void Update()
    {
        ItemImage.rectTransform.localEulerAngles = Vector3.forward * ((_item.Flipped) ? -90 : 0);
        if (_consumable != null)
            Counter.text = _consumable.CurrentValue + "/" + _consumable.MaxValue;
    }

    public void Pickup()
    {
        Color color = _background.color;
        color.a = _backgroundAlpha / 2f;
        _background.color = color;

        color = ItemImage.color;
        color.a = 1f / 2f;
        ItemImage.color = color;
        foreach (Image border in Borders)
            border.color = color;

        color = Counter.color;
        color.a = 1f / 2f;
        Counter.color = color;
    }

    public void Drop()
    {
        Color color = _background.color;
        color.a = _backgroundAlpha;
        _background.color = color;

        color = ItemImage.color;
        color.a = 1f;
        ItemImage.color = color;
        foreach (Image border in Borders)
            border.color = color;

        color = Counter.color;
        color.a = 1f;
        Counter.color = color;
    }
}
