using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;


    private TMP_Text quantityText;

    //shop 
    public int buyPrice = 10; //buy from shop
    [Range(0, 1)]
    public float sellPriceMultiplier = 0.5f; // sell for 50% buy price

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }


    public int GetSellPrice()
    {
        return Mathf.RoundToInt(buyPrice * sellPriceMultiplier);
    }

    public void UpdateQuantityDisplay()
    {
        if (quantityText == null)
            quantityText = GetComponentInChildren<TMP_Text>();

        if (quantityText == null)
            return;

        quantityText.text = quantity > 1 ? quantity.ToString() : "";
    }


    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
       GameObject clone = Instantiate(gameObject);
       Item cloneItem = clone.GetComponent<Item>();
         cloneItem.quantity = newQuantity;
         cloneItem.UpdateQuantityDisplay();
         return clone;
    }


    public virtual void UseItem()
    {
        Debug.Log($"Using item: {Name}");
    }




    public virtual void ShowPopUp()
    {
        Sprite itemIcon = GetComponent<Image>().sprite;
        if(ItemPickupUIController.instance != null)
        {
            ItemPickupUIController.instance.ShowItemPickup(Name, itemIcon);
        }
    }
}
