using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 PlayerPosition;  
    public string mapBoundary;
    public string sceneName;

    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData> chestSaveData;
    public List<QuestProgress> questProgressData;
    public List<string> handinQuestIDs;

    public int playerGold;
    public List<ShopInstanceData> shopStates = new();
    public List<DebtNPCSaveData> debtNpcStates = new();
}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}

[System.Serializable]
public class ShopInstanceData
{
    public string shopID;
    public List<ShopItemData> stock = new();
}

[System.Serializable]
public class ShopItemData
{
    public int itemID;
    public int quantity;
}

[System.Serializable]
public class DebtNPCSaveData
{
    public string npcID;
    public int currentDebtLevelIndex;
}
