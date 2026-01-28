using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour, Interactable
{

    public string shopID = "shop_merchant_01";
    public string shopkeeperName = "Merchant";

    public List<ShopStockItem> defaultShopStock = new();
    private List<ShopStockItem> currentShopStock = new();

    private bool isInitialized = false;


    [System.Serializable]
    public class ShopStockItem 
    { 
        public int itemID;
        public int quantity;
     
    }


    void Start()
    {



        InitalizeShop();

    }

    private void InitalizeShop()
    {
        if (isInitialized) return;

        currentShopStock = new List<ShopStockItem>();
        foreach (var item in defaultShopStock)
        {
            currentShopStock.Add(new ShopStockItem
            {
                itemID = item.itemID,
                quantity = item.quantity,


            });

        }
    }

    public bool CanInteract()
    {

        //shop only open in the day or at night
        //time
        return true;
    }

    public void Interact()
    {
        if(ShopController.Instance == null) return;

        if(ShopController.Instance.shopPanel.activeSelf)
        {
            ShopController.Instance.CloseShop();
        }
        else
        {
            ShopController.Instance.OpenShop(this);
        } 
            
    }

    public List<ShopStockItem> GetCurrentStock()
    {
        return currentShopStock;
    }    


    public void SetStock(List<ShopStockItem> stocks)
    {
        currentShopStock = stocks;
    }

    public void AddToStock(int itemID, int quantity)
    {
        ShopStockItem existing = currentShopStock.Find(s => s.itemID == itemID);
        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            currentShopStock.Add(new ShopStockItem { itemID = itemID, quantity = quantity });
        }
    }

    public bool RemoveFromShopStock(int itemID, int quantity)
    {
        ShopStockItem existing = currentShopStock.Find(s => s.itemID == itemID);
        if (existing != null && existing.quantity >= quantity)
        {
            existing.quantity -= quantity;
            return true;
        }
        return false;
    }

}
