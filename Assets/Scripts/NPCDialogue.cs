using UnityEngine;

public class NPC_Dialogue : MonoBehaviour
{
    [Header("Ustawienia Dialogu")]
    public string npcName = "Gepard Kierowca";
    [TextArea(3, 10)] // Powiêksza pole w Inspectorze
    public string[] sentences;

    [Header("Interakcja")]
    public GameObject pressE_Prompt; // Dymek "Wciœnij E" nad g³ow¹

    private bool isPlayerClose = false;

    void Start()
    {
        if (pressE_Prompt != null) pressE_Prompt.SetActive(false);
    }

    void Update()
    {
        // Jeœli gracz jest blisko i wciœnie E
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E))
        {
            // Sprawdzamy czy dialog ju¿ nie trwa (¿eby nie restartowaæ w kó³ko)
            if (!DialogueManager.instance.dialoguePanel.activeInHierarchy)
            {
                TriggerDialogue();
            }
        }
    }

    public void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(npcName, sentences);
    }

    // Wykrywanie gracza
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            if (pressE_Prompt != null) pressE_Prompt.SetActive(true); // Poka¿ "E"
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            if (pressE_Prompt != null) pressE_Prompt.SetActive(false); // Ukryj "E"
        }
    }
}