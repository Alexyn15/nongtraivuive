using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;

    private InventoryController inventoryController;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;// save original parent
        transform.SetParent(transform.root);// move to top level in hierarchy
        canvasGroup.blocksRaycasts = false;// disable raycast blocking
        canvasGroup.alpha = 0.6f;// make semi transparent
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;// follow the mouse

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;// enable raycast blocking
        canvasGroup.alpha = 1f;// make fully opaque

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();// get the slot we are dropped on

        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();// get the original slot

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if (draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                else
                {
                    // swap items between slots
                    dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                    originalSlot.currentItem = dropSlot.currentItem;
                    dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    transform.SetParent(dropSlot.transform);
                    dropSlot.currentItem = gameObject;

                    GetComponent<RectTransform>().anchoredPosition = Vector2.zero;// center
                }

            }
            else
            {
                originalSlot.currentItem = null;// original slot is now empty
                transform.SetParent(dropSlot.transform);
                dropSlot.currentItem = gameObject;
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;// center
            }

        }
        else
        {
            //If where we're dropping is mot within the inventory
            if (!IsWithinInventory(eventData.position))
            {
                //drop our item
                DropItem(originalSlot);
            }
            else
            {
                // no slot under drop point
                transform.SetParent(originalParent);
                GetComponent<RectTransform>().anchoredPosition = Vector2.zero;// center
            }
        }
    }



    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }




    void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        int quantity = item.quantity;

        if (quantity > 1)
        {
            item.RemoveFromStack();
            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            quantity = 1;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player not found in scene!");
            return;
        }

        // random drop position near player
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        // Lấy prefab gốc từ ItemDictionary thay vì instantiate chính UI item
        GameObject itemPrefab = null;
        if (InventoryController.Instance != null)
        {
            var itemDict = FindObjectOfType<ItemDictionary>();
            if (itemDict != null)
            {
                itemPrefab = itemDict.GetItemPrefab(item.ID);
            }
        }

        GameObject dropItem;
        if (itemPrefab != null)
        {
            dropItem = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
        }
        else
        {
            // fallback: dùng bản sao từ gameObject nhưng chỉ 1 lần
            dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        }

        Item droppedItem = dropItem.GetComponent<Item>();
        if (droppedItem != null)
        {
            droppedItem.quantity = 1;
            droppedItem.UpdateQuantityDisplay();
        }

        //Destroy UI item nếu không còn trong slot
        if (quantity <= 1 && originalSlot.currentItem == null)
        {
            Destroy(gameObject);
        }

        InventoryController.Instance.RebuildItemCounts();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Splitstack();

        }
    }

    private void Splitstack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1)
        {
            return;
        }

        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount);

        GameObject newItem = item.CloneItem(splitAmount);


        if (inventoryController == null || newItem == null) return;


        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }

        item.AddToStack(splitAmount);
        Destroy(newItem);

    }

}
