using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [HideInInspector] public GameObject currentItem;

    private void Awake()
    {
        // currentItem chỉ được gán runtime bởi InventoryController
        currentItem = null;
    }

    public void ClearSlot()
    {
        currentItem = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentItem == null) return;

        Item item = currentItem.GetComponent<Item>();
        if (item == null) return;

        Text quantityText = GetComponentInChildren<Text>();
        if (quantityText != null)
        {
            quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
        }
    }
}