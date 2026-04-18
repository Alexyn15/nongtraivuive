using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebtNPC : MonoBehaviour, Interactable
{
    [Header("Identity")]
    public string npcID = "debt_npc_01";
    public string npcName = "Ch? n?";
    public Sprite npcPortrait;

    [Header("Debt Levels (3 m?c)")]
    public List<int> debtLevels = new List<int> { 100, 300, 500 };

    [Header("Ending Choices (sau khi xong m?c 3)")]
    public string endingChoiceTextA = "L?a ch?n A";
    public string endingSceneA = "EndingA";
    public string endingChoiceTextB = "L?a ch?n B";
    public string endingSceneB = "EndingB";

    private DialogueController dialogueUI;
    private bool isDialogueActive;
    private int currentDebtLevelIndex; // 0,1,2 => chưa xong; >= debtLevels.Count => đ? xong

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
        if (PauseController.IsGamePaused && !isDialogueActive) return;
        OpenDebtDialogue();
    }

    private void OpenDebtDialogue()
    {
        if (dialogueUI == null) return;

        isDialogueActive = true;
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.SetNPCInfo(npcName, npcPortrait);
        dialogueUI.ClearChoices();
        PauseController.SetPause(true);

        if (currentDebtLevelIndex >= debtLevels.Count)
        {
            ShowEndingChoices();
            return;
        }

        int requiredGold = debtLevels[currentDebtLevelIndex];
        int currentGold = CurrencyController.Instance != null ? CurrencyController.Instance.GetGold() : 0;

        dialogueUI.SetDialogueText(
            $"Mức nợ {currentDebtLevelIndex + 1}/{debtLevels.Count}\n" +
            $"Cần trả: {requiredGold} vàng\n" +
            $"Bạn đang có: {currentGold} vàng"
        );

        dialogueUI.CreateChoiceButton("Trả nợ", TryPayCurrentDebtLevel);
        dialogueUI.CreateChoiceButton("Để sau", CloseDialogue);
    }

    private void TryPayCurrentDebtLevel()
    {
        if (currentDebtLevelIndex >= debtLevels.Count)
        {
            ShowEndingChoices();
            return;
        }

        int requiredGold = debtLevels[currentDebtLevelIndex];
        if (CurrencyController.Instance == null) return;

        bool paid = CurrencyController.Instance.SpendGold(requiredGold);
        if (!paid)
        {
            dialogueUI.SetDialogueText("Không đủ vàng để trả nợ.");
            dialogueUI.ClearChoices();
            dialogueUI.CreateChoiceButton("Đóng", CloseDialogue);
            return;
        }

        currentDebtLevelIndex++;

        if (currentDebtLevelIndex >= debtLevels.Count)
        {
            ShowEndingChoices();
            return;
        }

        OpenDebtDialogue();
    }

    private void ShowEndingChoices()
    {
        dialogueUI.SetDialogueText("Bạn đã trả xong 3 mức nợ. Hãy chọn kết thúc.");
        dialogueUI.ClearChoices();

        dialogueUI.CreateChoiceButton(endingChoiceTextA, () => ChooseEnding(endingSceneA));
        dialogueUI.CreateChoiceButton(endingChoiceTextB, () => ChooseEnding(endingSceneB));
    }

    private void ChooseEnding(string sceneName)
    {
        CloseDialogue();

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("DebtNPC: scene ending đang để trống.");
        }
    }

    private void CloseDialogue()
    {
        if (dialogueUI != null)
        {
            dialogueUI.ClearChoices();
            dialogueUI.SetDialogueText("");
            dialogueUI.ShowDialogueUI(false);
        }

        isDialogueActive = false;
        PauseController.SetPause(false);
    }

    public DebtNPCSaveData GetSaveData()
    {
        return new DebtNPCSaveData
        {
            npcID = npcID,
            currentDebtLevelIndex = currentDebtLevelIndex
        };
    }

    public void LoadFromSave(DebtNPCSaveData data)
    {
        if (data == null) return;
        if (data.npcID != npcID) return;

        currentDebtLevelIndex = Mathf.Clamp(data.currentDebtLevelIndex, 0, Mathf.Max(0, debtLevels.Count));
    }
}