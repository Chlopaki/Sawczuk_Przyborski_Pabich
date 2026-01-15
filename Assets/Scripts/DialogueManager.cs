using UnityEngine;
using TMPro; // Uøywamy TextMeshPro (jeúli uøywasz zwyk≥ego Text, zmieÒ na: using UnityEngine.UI;)
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; // Singleton - ≥atwy dostÍp zewszπd

    [Header("UI Elements")]
    public GameObject dialoguePanel; // Ca≥e okienko
    public TextMeshProUGUI nameText; // Pole imienia
    public TextMeshProUGUI dialogueText; // Pole treúci

    private Queue<string> sentences; // Kolejka zdaÒ
    private bool isDialogueActive = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typingSound; // TwÛj düwiÍk "blip"
    [Range(1, 5)][SerializeField] private int frequency = 3; // Graj düwiÍk co 2 literÍ
    [SerializeField] private float minPitch = 0.7f; // ZmiennoúÊ g≥osu
    [SerializeField] private float maxPitch = 0.9f;

    private AudioSource source; // èrÛd≥o düwiÍku

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
        dialoguePanel.SetActive(true); // Pokaø okno
        GameManager.instance.currentGameState = GameState.DIALOGUE;
        nameText.text = name;

        // Zatrzymaj gracza (Opcjonalne - odwo≥anie do Twojego PlayerController)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerController>().enabled = false;
        // if (player) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Zatrzymaj fizykÍ

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
        StopAllCoroutines(); // Zatrzymuje poprzednie pisanie, jeúli gracz klika szybko
        StartCoroutine(TypeSentence(sentence));
    }

    // Efekt pisania na maszynie
    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        int charCount = 0; // Licznik liter

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            charCount++;

            // Graj düwiÍk tylko co X liter i pomijaj spacje
            if (charCount % frequency == 0 && !char.IsWhiteSpace(letter))
            {
                if (typingSound != null && source != null)
                {
                    // Zmiana Pitcha (tonacji) dla efektu "gadania"
                    source.pitch = Random.Range(minPitch, maxPitch);
                    source.PlayOneShot(typingSound);
                }
            }

            yield return new WaitForSeconds(0.03f); // SzybkoúÊ pisania
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
        // Przewijanie dialogu spacjπ lub myszkπ
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            DisplayNextSentence();
        }
    }
}