using UnityEngine;



[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public DialogueChoie[] choies;
    public bool[] endDialoguelines;
    public float autoProgressDelay = 1.5f;
    
    public float typingSpeed = 0.05f;
    public AudioClip voiceSound;
    public float voicePitch = 1.0f;

    public int questInProgressIndex;
    public int questCompletedIndex;
    public Quest quest; // quest NPC gives

}

[System.Serializable]
public class DialogueChoie
{
    public int dialogueIndex; //dialogue line index
    public string[] choices; //player responses
    public int[] nextDialogueIndexes; //where choices leads
    public bool[] givesQuest; //if choice gives a quest
}

