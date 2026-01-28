using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public System.Collections.Generic.List<QuestProgress> activeQuests = new();

    private QuestUI questUI;
    public List<string> handinQuestIDs = new();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        questUI = FindObjectOfType<QuestUI>();
        InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;

    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;


        activeQuests.Add(new QuestProgress (quest));
        CheckInventoryForQuests();
        if (questUI != null)
            questUI.UpdateQuestUI();

    }

    public bool IsQuestActive(string questID) => activeQuests.Exists(q => q.QuestID == questID);

    public void CheckInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();

        foreach(QuestProgress quest in activeQuests)
        {
            foreach(QuestObjective questObjective in quest.objectives)
            {
                
                if(questObjective.type != ObjectiveType.CollectItem) continue;

                if (!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                int newAmount = itemCounts.TryGetValue(itemID, out int count) ? Mathf.Min(count, questObjective.requiredAmount) : 0;

                if(questObjective.currentAmount != newAmount)
                {
                    questObjective.currentAmount = newAmount;
                    
                }
            }
        }
        if (questUI != null)
            questUI.UpdateQuestUI();

    }

    public bool IsQuestCompleted(string questID)
    {
        if (string.IsNullOrEmpty(questID)) return false;

        QuestProgress quest = activeQuests.Find(q =>
            q != null &&
            q.quest != null &&
            q.quest.questID == questID
        );

        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }


    public void HandInQuest(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        if (quest == null) return;

        if (!RemoveRequiredItemsFrominventory(questID))
        {
            return;
        }

       
        RewardsController.Instance?.GiveQuestReWard(quest.quest);

        handinQuestIDs.Add(questID);
        activeQuests.Remove(quest);

        if (questUI != null)
            questUI.UpdateQuestUI();
    }


    public bool IsQuestHandedIn(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }

    public bool RemoveRequiredItemsFrominventory(string questID)
    {
        QuestProgress quest = activeQuests.Find(q => q.QuestID == questID);
        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();

        foreach(QuestObjective objective in quest.objectives)
        {
            if (objective.type == ObjectiveType.CollectItem && int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objective.requiredAmount; //add to required items
            }
        }
      
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        foreach(var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key)< item.Value)
            {
                return false; //not enough items
            }
        }

        foreach(var itemRequirement in requiredItems)
        {

            InventoryController.Instance.RemoveItemsFromInventory(itemRequirement.Key, itemRequirement.Value);


        }
        return true;


    }


    public void LoadQuestProgress(List<QuestProgress> saveQuests)
    {
        activeQuests = saveQuests ?? new();
        activeQuests.RemoveAll(q => q == null || q.quest == null);
        CheckInventoryForQuests();
        if (questUI != null)
            questUI.UpdateQuestUI();

    }
}





