using UnityEngine;

public class Animal : MonoBehaviour, Interactable
{
    [Header("Food Settings")]
    public int requiredFoodItemID;      // ID item dùng để cho ăn (ví dụ: cỏ, lúa)
    public int foodPerProduct = 3;      // cần ăn bao nhiêu lần để tạo ra 1 sản phẩm

    [Header("Product Settings")]
    public int productItemID;           // ID item sản phẩm (trứng, sữa…)
    public int productAmount = 1;       // mỗi lần cho bao nhiêu sản phẩm

    [Header("State")]
    public int currentFoodCount = 0;    // đã ăn bao nhiêu
    public bool hasProduct = false;     // đã sẵn sàng cho thu hoạch chưa

    private InventoryController inventory;
    private ItemDictionary itemDictionary;

    private void Start()
    {
        inventory = InventoryController.Instance;
        itemDictionary = FindObjectOfType<ItemDictionary>();
    }

    // --- Interactable implementation ---
    public bool CanInteract()
    {
        // luôn cho phép tương tác, hoặc bạn có thể thêm logic khác
        return true;
    }

    public void Interact()
    {
        // Ví dụ: bấm E lần 1 cho ăn, lần 2 nếu có product thì thu hoạch
        if (!hasProduct)
        {
            TryFeed();
        }
        else
        {
            TryCollectProduct();
        }
    }

    // --- Gameplay logic ---

    private void TryFeed()
    {
        if (inventory == null) return;

        // Đồng bộ lại cache trước khi kiểm tra
        inventory.RebuildItemCounts();

        bool removed = inventory.RemoveItemsFromInventory(requiredFoodItemID, 1);
        if (!removed)
        {
            Debug.LogWarning($"Không trừ được food itemID={requiredFoodItemID}. Kiểm tra ID item sau load/save.");
            return;
        }

        currentFoodCount++;
        Debug.Log($"Động vật được cho ăn. Đã ăn: {currentFoodCount}/{foodPerProduct}");

        if (currentFoodCount >= foodPerProduct)
        {
            currentFoodCount = 0;
            hasProduct = true;
            Debug.Log("Động vật đã sẵn sàng cho sản phẩm!");
        }
    }

    private void TryCollectProduct()
    {
        if (!hasProduct) return;
        if (itemDictionary == null || inventory == null) return;

        GameObject productPrefab = itemDictionary.GetItemPrefab(productItemID);
        if (productPrefab == null)
        {
            Debug.LogError($"Không tìm thấy prefab cho product itemID = {productItemID}");
            return;
        }

        // Cách 1: add thẳng vào inventory
        bool added = inventory.AddItem(productPrefab);
        if (added)
        {
            hasProduct = false;
            Debug.Log($"Thu hoạch {productAmount} sản phẩm từ động vật.");
        }
        else
        {
            Debug.Log("Inventory full, không thể nhận sản phẩm.");
        }

        // Nếu bạn muốn mỗi lần cho nhiều hơn 1 sản phẩm:
        // for (int i = 0; i < productAmount; i++)
        // {
        //     if (!inventory.AddItem(productPrefab))
        //     {
        //         Debug.Log("Inventory full khi đang thêm sản phẩm.");
        //         break;
        //     }
        // }
        // hasProduct = false;
    }
}