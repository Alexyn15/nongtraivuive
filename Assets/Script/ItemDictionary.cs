using System.Collections.Generic;
using UnityEngine;

public class ItemDictionary : MonoBehaviour
{
    [SerializeField] private List<Item> itemPrefabs = new List<Item>();
    private Dictionary<int, GameObject> itemDictionary;

    private void Awake()
    {
        RebuildDictionary();
    }

    private void RebuildDictionary()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        foreach (Item item in itemPrefabs)
        {
            if (item == null) continue;

            if (item.ID <= 0)
            {
                Debug.LogError($"ItemDictionary: Item '{item.name}' có ID <= 0. Hãy đặt ID từ 1 trở lên.");
                continue;
            }

            if (itemDictionary.ContainsKey(item.ID))
            {
                Debug.LogError($"ItemDictionary: Trùng ID = {item.ID} ở item '{item.name}'.");
                continue;
            }

            itemDictionary.Add(item.ID, item.gameObject);
        }
    }

    public GameObject GetItemPrefab(int itemID)
    {
        if (itemID <= 0)
        {
            Debug.LogWarning($"ItemDictionary: itemID không hợp lệ: {itemID}");
            return null;
        }

        itemDictionary.TryGetValue(itemID, out GameObject prefab);
        if (prefab == null)
        {
            Debug.LogWarning($"ItemDictionary: Không tìm thấy item với ID = {itemID}");
        }

        return prefab;
    }
}
