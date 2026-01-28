using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{   

    public static DialogueController Instance { get; private set; }


    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;   
    void Awake()
    {
        //make sure only one instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show); //UI viability

    }
    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        portraitImage.sprite = portrait;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }


    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public GameObject CreateChoiceButton(string choiceText,UnityEngine.Events.UnityAction onClick)
    {
        GameObject choicebutton = Instantiate(choiceButtonPrefab, choiceContainer);
        choicebutton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choicebutton.GetComponent<Button>().onClick.AddListener(onClick);
        return choicebutton;
    }
}
