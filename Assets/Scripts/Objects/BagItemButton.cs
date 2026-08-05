using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BagItemButton : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI QuantityText;

    public void Setup(Item newItem, BagManager manager, int quantity = 1)
    {
        itemIcon.sprite = newItem.itemSprite;

        if (QuantityText != null)
        {
            QuantityText.text = quantity.ToString();
        }

        Debug.Log("Setting up BagItemButton for item: " + newItem.itemName + " with quantity: " + quantity);

        GetComponent<Button>().onClick.RemoveAllListeners();
    }
}