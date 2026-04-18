using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private CinemachineConfiner2D confiner;

    private Chest[] chests;
    private ShopNPC[] shops;
    private DebtNPC[] debtNpcs;

    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;

    private void Start()
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
        debtNpcs = FindObjectsOfType<DebtNPC>();
    }

    private bool IsValid()
    {
        if (player == null)
            Debug.LogError("SaveController: player is NULL");

        if (confiner == null)
            Debug.LogError("SaveController: confiner (CinemachineConfiner2D) is NULL");

        if (confiner != null && confiner.BoundingShape2D == null)
            Debug.LogError("SaveController: confiner.BoundingShape2D is NULL");

        if (inventoryController == null)
            Debug.LogError("SaveController: inventoryController is NULL");

        if (hotbarController == null)
            Debug.LogError("SaveController: hotbarController is NULL");

        return player != null
            && confiner != null
            && confiner.BoundingShape2D != null
            && inventoryController != null
            && hotbarController != null;
    }

    public void SaveGame(string overrideSceneName = null)
    {
        if (!IsValid())
        {
            Debug.LogError("SaveGame aborted: invalid references");
            return;
        }

        SaveData saveData = new SaveData
        {
            PlayerPosition = player.position,
            mapBoundary = confiner.BoundingShape2D.name,
            sceneName = string.IsNullOrEmpty(overrideSceneName)
                ?   SceneManager.GetActiveScene().name
                : overrideSceneName,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
            questProgressData = QuestController.Instance != null ? QuestController.Instance.activeQuests : new List<QuestProgress>(),
            handinQuestIDs = QuestController.Instance != null ? QuestController.Instance.handinQuestIDs : new List<string>(),
            playerGold = CurrencyController.Instance != null ? CurrencyController.Instance.GetGold() : 0,
            shopStates = GetShopStates(),
            debtNpcStates = GetDebtNpcStates()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData, true));
        Debug.Log("Saved to: " + saveLocation);
    }

    public void SaveGame()
    {
        SaveGame(null);
    }

    public void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            Debug.Log("No save file found. Initializing new game.");
            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());
            RefreshChests();
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
        if (saveData == null)
        {
            Debug.LogError("LoadGame failed: saveData is null");
            return;
        }

        //string activeScene = SceneManager.GetActiveScene().name;
        //if (!string.IsNullOrEmpty(saveData.sceneName) && saveData.sceneName != activeScene)
        //{
        //    SceneManager.LoadScene(saveData.sceneName);
        //    return;
        //} // test save mapboss 

        player.position = saveData.PlayerPosition;

        GameObject map = GameObject.Find(saveData.mapBoundary);
        if (map != null)
        {
            confiner.BoundingShape2D = map.GetComponent<PolygonCollider2D>();
        }

        inventoryController.SetInventoryItems(saveData.inventorySaveData ?? new List<InventorySaveData>());
        hotbarController.SetHotbarItems(saveData.hotbarSaveData ?? new List<InventorySaveData>());

        RefreshChests();
        LoadChestStates(saveData.chestSaveData);

        if (QuestController.Instance != null)
        {
            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
            QuestController.Instance.handinQuestIDs = saveData.handinQuestIDs ?? new List<string>();
        }

        LoadShopStates(saveData.shopStates);

        if (CurrencyController.Instance != null)
        {
            CurrencyController.Instance.SetGold(saveData.playerGold);
        }

        RefreshDebtNpcs();
        LoadDebtNpcStates(saveData.debtNpcStates);

        MapController_Manual.Instance?.HighlightArea(saveData.mapBoundary);

        Debug.Log("Loaded save file");
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

        InitializeNewGameState();
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach (Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpen
            });
        }

        return chestStates;
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
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);
            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }

    private List<ShopInstanceData> GetShopStates()
    {
        List<ShopInstanceData> shopStates = new List<ShopInstanceData>();

        foreach (ShopNPC shop in shops)
        {
            ShopInstanceData shopData = new ShopInstanceData
            {
                shopID = shop.shopID,
                stock = new List<ShopItemData>()
            };

            foreach (ShopNPC.ShopStockItem stockItem in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData
                {
                    itemID = stockItem.itemID,
                    quantity = stockItem.quantity
                });
            }

            shopStates.Add(shopData);
        }

        return shopStates;
    }

    private void LoadShopStates(List<ShopInstanceData> shopStates)
    {
        if (shopStates == null) return;

        foreach (ShopNPC shop in shops)
        {
            ShopInstanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.shopID);
            if (shopData == null) continue;

            List<ShopNPC.ShopStockItem> loadedStock = new List<ShopNPC.ShopStockItem>();
            foreach (ShopItemData itemData in shopData.stock)
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

    private List<DebtNPCSaveData> GetDebtNpcStates()
    {
        List<DebtNPCSaveData> states = new List<DebtNPCSaveData>();

        foreach (DebtNPC npc in debtNpcs)
        {
            if (npc != null)
            {
                states.Add(npc.GetSaveData());
            }
        }

        return states;
    }

    private void LoadDebtNpcStates(List<DebtNPCSaveData> states)
    {
        if (states == null) return;

        foreach (DebtNPC npc in debtNpcs)
        {
            if (npc == null) continue;

            DebtNPCSaveData data = states.FirstOrDefault(s => s.npcID == npc.npcID);
            npc.LoadFromSave(data);
        }
    }

    private void RefreshChests()
    {
        chests = FindObjectsOfType<Chest>();
    }

    private void RefreshDebtNpcs()
    {
        debtNpcs = FindObjectsOfType<DebtNPC>();
    }

    private void InitializeNewGameState()
    {
        if (inventoryController == null)
            inventoryController = FindObjectOfType<InventoryController>();

        if (hotbarController == null)
            hotbarController = FindObjectOfType<HotbarController>();
    
        inventoryController?.SetInventoryItems(new List<InventorySaveData>());
        hotbarController?.SetHotbarItems(new List<InventorySaveData>());

        RefreshChests();

        if (QuestController.Instance != null)
        {
            QuestController.Instance.activeQuests = new List<QuestProgress>();
            QuestController.Instance.handinQuestIDs = new List<string>();
            QuestController.Instance.CheckInventoryForQuests();
        }

        CurrencyController.Instance?.SetGold(0);

        Debug.Log("Initialized new game state after deleting save.");
    }

    public void SaveGameForSceneTransition(string targetSceneName, Vector3 targetPlayerPosition, string targetMapBoundaryName)
    {
        if (!IsValid())
        {
            Debug.LogError("SaveGameForSceneTransition aborted: invalid references");
            return;
        }

        SaveData saveData = new SaveData
        {
            PlayerPosition = targetPlayerPosition,
            mapBoundary = targetMapBoundaryName,
            sceneName = targetSceneName,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),
            questProgressData = QuestController.Instance != null ? QuestController.Instance.activeQuests : new List<QuestProgress>(),
            handinQuestIDs = QuestController.Instance != null ? QuestController.Instance.handinQuestIDs : new List<string>(),
            playerGold = CurrencyController.Instance != null ? CurrencyController.Instance.GetGold() : 0,
            shopStates = GetShopStates(),
            debtNpcStates = GetDebtNpcStates()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData, true));
        Debug.Log("Saved transition to: " + targetSceneName);
    }
}
        