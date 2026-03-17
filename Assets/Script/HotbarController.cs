using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10;

    private ItemDictionary itemDictionary;
    private Key[] hotbarKeys;

    public int selectedIndex = -1;
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }

    }

    // Update is called once per frame
    void Update()
    {
        //check for hotbar key presses
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                SelectSlot(i);
            }
        }

    }

    void UseItemInSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null) { 
        Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();

        }

    }


    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                hotbarData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex()
                });
            }
        }
        return  hotbarData;
    }

    public void SetHotbarItems(List<InventorySaveData> inventorySaveData)
    {
        // Clear inventory panel - avoid duplicates
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }
        //create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        //populate slots with items from save data
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;
                }
            }
        }
    }

    void SelectSlot(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < slotCount; i++)
        {
            Slot slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();
            UnityEngine.UI.Image image = slot.GetComponent<UnityEngine.UI.Image>();

            if (i == selectedIndex)
                image.color = selectedColor;
            else
                image.color = normalColor;
        }
    }

    public Item GetSelectedItem()
    {
        if (selectedIndex < 0) return null;

        Slot slot = hotbarPanel.transform.GetChild(selectedIndex).GetComponent<Slot>();

        if (slot.currentItem == null) return null;

        return slot.currentItem.GetComponent<Item>();
    }

    public Slot GetSelectedSlot()
    {
        if (selectedIndex < 0) return null;

        if (selectedIndex >= hotbarPanel.transform.childCount)
            return null;

        return hotbarPanel.transform
            .GetChild(selectedIndex)
            .GetComponent<Slot>();
    }


    public void AddItem(Item itemToAdd, int amount)
    {
        // 1️⃣ Tìm slot đã có cùng item để cộng stack
        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Slot slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Item existingItem = slot.currentItem.GetComponent<Item>();

                if (existingItem.ID == itemToAdd.ID)
                {
                    existingItem.quantity += amount;
                    slot.UpdateUI();
                    return;
                }
            }
        }

        // 2️⃣ Nếu chưa có → tìm slot trống
        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Slot slot = hotbarPanel.transform.GetChild(i).GetComponent<Slot>();

            if (slot.currentItem == null)
            {
                GameObject newItemGO = Instantiate(
                    itemDictionary.GetItemPrefab(itemToAdd.ID),
                    slot.transform
                );

                newItemGO.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                Item newItem = newItemGO.GetComponent<Item>();
                newItem.quantity = amount;

                slot.currentItem = newItemGO;
                return;
            }
        }

        Debug.Log("Hotbar đầy!");
    }

}
