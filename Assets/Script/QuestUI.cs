using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{

    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;



    void Start()
    {
       
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        if (questListContent == null) return;
        if (QuestController.Instance == null) return;
        if (QuestController.Instance.activeQuests == null) return;

        foreach (Transform child in questListContent)
            Destroy(child.gameObject);

        foreach (var quest in QuestController.Instance.activeQuests)
        {
            if (quest == null || quest.quest == null || quest.objectives == null) continue;

            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            Transform questNameTf = entry.transform.Find("QuestNameText");
            Transform objectiveList = entry.transform.Find("ObjectiveList");
            if (questNameTf == null || objectiveList == null) continue;

            TMP_Text questNameText = questNameTf.GetComponent<TMP_Text>();
            questNameText.text = quest.quest.name;

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }





}
