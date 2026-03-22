using System.IO;
using UnityEngine;
using Unity.Cinemachine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CinemachineConfiner2D confiner;

    private Chest[] chests;

    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;

    private ShopNPC[] shops;



    void Start()
    {
        InitializeComponents();
        if (!IsValid())
        {
            Debug.LogError("SaveController missing references");
            return;
        }

        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
        chests = FindObjectsOfType<Chest>();
        shops = FindObjectsOfType<ShopNPC>();
    }



    bool IsValid()
    {
        if (player == null)
            Debug.LogError("SaveController: player is NULL");

        if (confiner == null)
            Debug.LogError("SaveController: confiner (CinemachineConfiner2D) is NULL");

        if (confiner != null && confiner.BoundingShape2D == null)
            Debug.LogError("SaveController: confiner.BoundingShape2D is NULL");

        return player != null
            && confiner != null
            && confiner.BoundingShape2D != null;
    }

    public void SaveGame()
    {
        if (!IsValid())
        {
            Debug.LogError("SaveGame aborted: null reference");
            return;
        }

        SaveData saveData = new SaveData
        {
            PlayerPosition = player.position,
            mapBoundary = confiner.BoundingShape2D.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
            questProgressData = QuestController.Instance.activeQuests,
            handinQuestIDs = QuestController.Instance.handinQuestIDs,
            playerGold = CurrencyController.Instance.GetGold(),
            shopStates = GetShopStates()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData, true));
        Debug.Log("Saved to: " + saveLocation);
    }

    private List<ShopInstanceData> GetShopStates()
    {
        List<ShopInstanceData> shopStates = new List<ShopInstanceData>();
        foreach(var shop in shops)
        {
            ShopInstanceData shopData = new ShopInstanceData
            {
                shopID = shop.shopID,
                stock = new List<ShopItemData>()
            };

            foreach(var stockItem in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData
                {
                    itemID = stockItem.itemID,
                    quantity = stockItem.quantity,
                });
            }

            shopStates.Add(shopData);
        }
        return shopStates;
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpen
            };
            chestStates.Add(chestSaveData);
        }
        return chestStates;
    }



    public void LoadGame()
    {
        //  FIRST TIME PLAYER
        if (!File.Exists(saveLocation))
        {
            Debug.Log("No save file found. Initializing new game.");

            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());
            RefreshChests(); // chest mặc định là đóng
            


            return;
        }

        // LOAD SAVE
        SaveData saveData =
            JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        player.position = saveData.PlayerPosition;
        GameObject map = GameObject.Find(saveData.mapBoundary);
        if (map != null)
        {
            confiner.BoundingShape2D =
                map.GetComponent<PolygonCollider2D>();
        }

        inventoryController.SetInventoryItems(
            saveData.inventorySaveData ?? new List<InventorySaveData>());

        hotbarController.SetHotbarItems(
            saveData.hotbarSaveData ?? new List<InventorySaveData>());

        RefreshChests();
        LoadChestStates(saveData.chestSaveData);
        QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
        QuestController.Instance.handinQuestIDs = saveData.handinQuestIDs;

        LoadShopStates(saveData.shopStates);
        CurrencyController.Instance.SetGold(saveData.playerGold);

        //load highlighted map area
        MapController_Manual.Instance?.HighlightArea(saveData.mapBoundary);

        Debug.Log("Loaded save file");
    }



    private void RefreshChests()
    {
        chests = FindObjectsOfType<Chest>();
    }

    private void LoadShopStates(List<ShopInstanceData> shopStates)
    {
        if (shopStates == null) return;

        foreach (var shop in shops)
        {
            ShopInstanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.shopID);

            if (shopData != null)
            {
                List<ShopNPC.ShopStockItem> loadedStock = new List<ShopNPC.ShopStockItem>();

                foreach(var itemData in shopData.stock)
                {
                    loadedStock.Add(new ShopNPC.ShopStockItem
                    {
                        itemID = itemData.itemID,
                        quantity = itemData.quantity
                    });
                }

                shop.SetStock(loadedStock);
            }
        }
    }







    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        if (chestStates == null)
        {
            Debug.LogWarning("No chest save data found, skipping chest load");
            return;
        }

        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData =
                chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);

            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }

    public void DeleteSaveFile()
    {
        try
        {
            if (!string.IsNullOrEmpty(saveLocation) && File.Exists(saveLocation))
            {
                File.Delete(saveLocation);
                Debug.Log("Save file deleted: " + saveLocation);
            }
            else
            {
                Debug.Log("No save file to delete at: " + saveLocation);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to delete save file: " + e.Message);
        }

        // Sau khi xoá save, reset game về trạng thái mới
        InitializeNewGameState();
    }

    private void InitializeNewGameState()
    {
        // giống nhánh "first time player" trong LoadGame
        if (inventoryController == null)
            inventoryController = FindObjectOfType<InventoryController>();
        if (hotbarController == null)
            hotbarController = FindObjectOfType<HotbarController>();

        inventoryController?.SetInventoryItems(new List<InventorySaveData>());
        hotbarController?.SetHotbarItems(new List<InventorySaveData>());
        RefreshChests(); // chest mặc định đóng

        // Reset quest & vàng nếu muốn
        if (QuestController.Instance != null)
        {
            QuestController.Instance.activeQuests = new();
            QuestController.Instance.handinQuestIDs = new();
            QuestController.Instance.CheckInventoryForQuests();
        }

        CurrencyController.Instance?.SetGold(0);

        Debug.Log("Initialized new game state after deleting save.");
    }
}
