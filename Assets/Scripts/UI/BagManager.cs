using UnityEngine;
using UnityEngine.UI;

public class BagManager : MonoBehaviour
{
    public GameObject bagUIPanel;
    public Transform itemGridParent;
    public GameObject itemButtonPrefab;
    public static bool IsBagOpen = false;

    public Button closeButton;
    public Signal CoinSignal;

    public Inventory playerInventory;
    public PetBubble petBubble;

    void Start()
    {
        Debug.Log("=== BAG START ===");

        if (playerInventory == null)
        {
            Debug.LogError("Inventory is NULL");
        }
        else
        {
            Debug.Log("Inventory count: " + playerInventory.items.Count);

            foreach (InventoryEntry entry in playerInventory.items)
            {
                Debug.Log("ITEM: " + entry.item.itemName);
            }
        }

        bagUIPanel.SetActive(false);
        RefreshCoins();

        closeButton.onClick.AddListener(CloseBag);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBag();
            if (petBubble != null)
            {
                petBubble.playerHasOpenedBag = true;

            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseBag();
            if (petBubble != null)
            {
                petBubble.playerHasOpenedBag = true;

            }
        }
    }

    public void ToggleBag()
    {
        bool isActive = bagUIPanel.activeSelf;

        bagUIPanel.SetActive(!isActive);

        IsBagOpen = !isActive;

        if (!isActive)
        {
            PopulateBag();
        }
    }

    public void CloseBag()
    {
        bagUIPanel.SetActive(false);
        IsBagOpen = false;

        petBubble.playerHasOpenedBag = true;
    }

    void PopulateBag()
    {
        foreach (Transform child in itemGridParent)
            Destroy(child.gameObject);

        foreach (InventoryEntry entry in playerInventory.items)
        {
            GameObject buttonObj = Instantiate(itemButtonPrefab, itemGridParent);
            BagItemButton btn = buttonObj.GetComponent<BagItemButton>();
            btn.Setup(entry.item, this, entry.quantity);
        }
    }

    void RefreshCoins()
    {
        CoinSignal.Raise();
    }
}