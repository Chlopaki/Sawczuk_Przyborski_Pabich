using UnityEngine;
using TMPro; // U¿ywamy TextMeshPro (jeœli u¿ywasz zwyk³ego Text, zmieñ na: using UnityEngine.UI;)
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; // Singleton - ³atwy dostêp zewsz¹d

    [Header("UI Elements")]
    public GameObject dialoguePanel; // Ca³e okienko
    public TextMeshProUGUI nameText; // Pole imienia
    public TextMeshProUGUI dialogueText; // Pole treœci

    private Queue<string> sentences; // Kolejka zdañ
    private bool isDialogueActive = false;

    void Awake()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (instance == null) instance = this;
        sentences = new Queue<string>();
    }

    public void StartDialogue(string name, string[] lines)
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true); // Poka¿ okno
        GameManager.instance.currentGameState = GameState.DIALOGUE;
        nameText.text = name;

        // Zatrzymaj gracza (Opcjonalne - odwo³anie do Twojego PlayerController)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerController>().enabled = false;
        // if (player) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Zatrzymaj fizykê

        sentences.Clear();

        foreach (string line in lines)
        {
            sentences.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines(); // Zatrzymuje poprzednie pisanie, jeœli gracz klika szybko
        StartCoroutine(TypeSentence(sentence));
    }

    // Efekt pisania na maszynie
    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.02f); // Szybkoœæ pisania
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel.SetActive(false); // Ukryj okno

        // Odblokuj gracza
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerController>().enabled = true;

        GameManager.instance.currentGameState = GameState.GAME;
    }

    void Update()
    {
        // Przewijanie dialogu spacj¹ lub myszk¹
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            DisplayNextSentence();
        }
    }
}