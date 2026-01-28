using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
public class NPC : MonoBehaviour, Interactable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }




    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {

        if(dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            return;



        }
        if (isDialogueActive)
        {
            NextLine();
        }
        else 
        {
            StartDialogue();
        }

    }
    void StartDialogue()
    {
        if (dialogueUI == null) return;

        //sync data
        SyncQuestState();
        // set dialogue line based on quest state
        if (questState == QuestState.NotStarted)
        {
            dialogueIndex = 0;

        }
        else if (questState == QuestState.InProgress)
        {
            dialogueIndex = dialogueData.questInProgressIndex;
        }
        else if(questState == QuestState.Completed)
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }


        isDialogueActive = true;
       

        
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        
        PauseController.SetPause(true);

        //type line
        
        DisplayCurrentLine();

    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null)
        {
            return;
        }

        if (QuestController.Instance == null) return;

        string questID = dialogueData.quest.questID;


        //Future update add completing quest and handing in
        if(QuestController.Instance.IsQuestCompleted(questID) || QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else 
        {
            questState = QuestState.NotStarted;
        }
       
    }




    void NextLine()
    {
        if (isTyping)
        {
            //skip to full line
            StopAllCoroutines();
            
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            
        }


        //clear choices
        dialogueUI.ClearChoices();
        //check for choices at this line

        if (dialogueData.endDialoguelines.Length > dialogueIndex && dialogueData.endDialoguelines[dialogueIndex])
        {
            EndDialogue();
            return;
        }
        //see if any choices exist for this dialogue index
        foreach (DialogueChoie dialogueChoie in dialogueData.choies)
        {
            if (dialogueChoie.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoie);

                return; //exit to wait for player choice
            }
        }

         if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            //if anorther line , type next line
            DisplayCurrentLine();
            
        }
        else
        {
            //end dialogue
            EndDialogue();
        }

    }




    IEnumerator TypeLine()
     {
        if (dialogueData.dialogueLines == null) yield break;
        if (dialogueIndex >= dialogueData.dialogueLines.Length) yield break;

        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;


        if (dialogueData.autoProgressLines.Length>dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);

            //DisplayNextLine();
            NextLine();

        }
    }


    void DisplayChoices(DialogueChoie choie)
    {
        for (int i = 0; i < choie.choices.Length; i++)
        {
            int nextIndex = choie.nextDialogueIndexes[i];
            bool givesQuest = choie.givesQuest[i];
            dialogueUI.CreateChoiceButton(choie.choices[i], () => ChooseOption(nextIndex, givesQuest));
        }
    }

    void ChooseOption(int nextIndex,bool givesQuest)
    {
        if (givesQuest)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }



        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();

        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }



    public void EndDialogue()
    {
        if (dialogueData.quest != null &&
        questState == QuestState.Completed &&
        QuestController.Instance != null &&
        !QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            HandlequestCompletion(dialogueData.quest);
        }
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }

    void HandlequestCompletion(Quest quest)
    {
       
        QuestController.Instance.HandInQuest(quest.questID);
    }

}
