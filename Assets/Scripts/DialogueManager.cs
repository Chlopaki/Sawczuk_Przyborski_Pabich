using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; // Singleton - �atwy dost�p zewsz�d

    [Header("UI Elements")]
    public GameObject dialoguePanel; // Ca�e okienko
    public TextMeshProUGUI nameText; // Pole imienia
    public TextMeshProUGUI dialogueText; // Pole tre�ci

    private Queue<string> sentences; // Kolejka zda�
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private string currentSentence = "";

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typingSound; // Tw�j d�wi�k "blip"
    [Range(1, 5)][SerializeField] private int frequency = 3; // Graj d�wi�k co 2 liter�
    [SerializeField] private float minPitch = 0.7f; // Zmienno�� g�osu
    [SerializeField] private float maxPitch = 0.9f;

    private AudioSource source; // �r�d�o d�wi�ku

    void Awake()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (instance == null) instance = this;
        sentences = new Queue<string>();

        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
    }

    public void StartDialogue(string name, string[] lines)
    {
        isDialogueActive = true;
        dialoguePanel.SetActive(true); // Poka� okno
        GameManager.instance.currentGameState = GameState.DIALOGUE;
        nameText.text = name;

        // Zatrzymaj gracza (Opcjonalne - odwo�anie do Twojego PlayerController)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerController>().enabled = false;
        // if (player) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Zatrzymaj fizyk�

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

        currentSentence = sentences.Dequeue();

        //string sentence = sentences.Dequeue();
        StopAllCoroutines(); // Zatrzymuje poprzednie pisanie, je�li gracz klika szybko
        StartCoroutine(TypeSentence(currentSentence));
    }

    // Efekt pisania na maszynie
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        int charCount = 0; // Licznik liter

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            charCount++;

            // Graj d�wi�k tylko co X liter i pomijaj spacje
            if (charCount % frequency == 0 && !char.IsWhiteSpace(letter))
            {
                if (typingSound != null && source != null)
                {
                    // Zmiana Pitcha (tonacji) dla efektu "gadania"
                    source.pitch = Random.Range(minPitch, maxPitch);
                    source.PlayOneShot(typingSound);
                }
            }

            yield return new WaitForSeconds(0.03f); // Szybko�� pisania
        }
        isTyping = false; // --- ZMIANA 4: Sko�czyli�my pisa�
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
        // Przewijanie dialogu spacj� lub myszk�
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            if (isTyping)
            {
                // Je�li tekst si� pisze -> Przerwij i poka� ca�o��
                StopAllCoroutines();
                dialogueText.text = currentSentence;
                isTyping = false;
            }
            else
            {
                // Je�li tekst jest ju� ca�y -> Poka� nast�pny
                DisplayNextSentence();
            }
        }
    }
}