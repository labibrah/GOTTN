using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class BagItemButton : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI QuantityText;

    [Tooltip("Multiplier applied to the icon's default size when populating the bag.")]
    public float iconScale = 1.2f;

    public void Setup(Item newItem, BagManager manager, int quantity = 1)
    {
        itemIcon.sprite = newItem.itemSprite;

        if (QuantityText != null)
        {
            QuantityText.gameObject.SetActive(false);
        }

        if (itemIcon != null)
        {
            itemIcon.rectTransform.localScale = Vector3.one * iconScale;
        }

        Debug.Log("Setting up BagItemButton for item: " + newItem.itemName + " with quantity: " + quantity);
        GetComponent<Button>().onClick.RemoveAllListeners();
    }
}