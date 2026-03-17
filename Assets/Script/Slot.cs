using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public GameObject currentItem;

    public void ClearSlot()
    {
        currentItem = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (currentItem == null) return;

        Item item = currentItem.GetComponent<Item>();

        // Ví dụ nếu bạn có Text hiển thị số lượng
        Text quantityText = GetComponentInChildren<Text>();

        if (quantityText != null)
        {
            quantityText.text = item.quantity > 1 ? item.quantity.ToString() : "";
        }
    }
}