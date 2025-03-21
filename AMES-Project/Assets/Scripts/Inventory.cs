using UnityEngine;

public class Inventory : MonoBehaviour
{
    // The single item slot in the inventory (could be null if no item is present)
    public Item currentItem;

    // Method to add an item to the inventory
    public void AddItem(Item newItem)
    {
        if (currentItem == null)  // If there's no item in the slot
        {
            currentItem = newItem;
            Debug.Log("Item added: " + currentItem.name);
        }
        else
        {
            Debug.Log("Inventory Full: " + currentItem.name);
        }
    }

    // Method to use the item
    public void UseItem()
    {
        if (currentItem != null)
        {
            Debug.Log("Using item: " + currentItem.name);
            // Do something with the item (for example, applying its effect)

            // Optionally, remove the item after using it
            currentItem = null;
        }
        else
        {
            Debug.Log("No item to use!");
        }
    }

    // Method to remove the item (if you want a method to drop or discard it)
    public void RemoveItem()
    {
        if (currentItem != null)
        {
            Debug.Log("Item removed: " + currentItem.name);
            currentItem = null;
        }
        else
        {
            Debug.Log("No item to remove!");
        }
    }
}

[System.Serializable]
public class Item
{
    public string name;
    public string description;
    public Sprite icon;  // You can add an icon to display in the UI if needed

    public Item(string itemName, string itemDescription, Sprite itemIcon)
    {
        name = itemName;
        description = itemDescription;
        icon = itemIcon;
    }
}
