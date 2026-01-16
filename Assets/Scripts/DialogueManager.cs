using UnityEngine;
using TMPro; // U¿ywamy TextMeshPro (jeœli u¿ywasz zwyk³ego Text, zmieñ na: using UnityEngine.UI;)
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; // Singleton - ï¿½atwy dostï¿½p zewszï¿½d

    [Header("UI Elements")]
    public GameObject dialoguePanel; // Caï¿½e okienko
    public TextMeshProUGUI nameText; // Pole imienia
    public TextMeshProUGUI dialogueText; // Pole treï¿½ci

    private Queue<string> sentences; // Kolejka zdaï¿½
    private bool isDialogueActive = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip typingSound; // Twï¿½j dï¿½wiï¿½k "blip"
    [Range(1, 5)][SerializeField] private int frequency = 3; // Graj dï¿½wiï¿½k co 2 literï¿½
    [SerializeField] private float minPitch = 0.7f; // Zmiennoï¿½ï¿½ gï¿½osu
    [SerializeField] private float maxPitch = 0.9f;

    private AudioSource source; // ï¿½rï¿½dï¿½o dï¿½wiï¿½ku

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
        dialoguePanel.SetActive(true); // Pokaï¿½ okno
        GameManager.instance.currentGameState = GameState.DIALOGUE;
        nameText.text = name;

        // Zatrzymaj gracza (Opcjonalne - odwoï¿½anie do Twojego PlayerController)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player) player.GetComponent<PlayerController>().enabled = false;
        // if (player) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Zatrzymaj fizykï¿½

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

<<<<<<< HEAD
        currentSentence = sentences.Dequeue();

        //string sentence = sentences.Dequeue();
        StopAllCoroutines(); // Zatrzymuje poprzednie pisanie, jeï¿½li gracz klika szybko
        StartCoroutine(TypeSentence(currentSentence));
=======
        string sentence = sentences.Dequeue();
        StopAllCoroutines(); // Zatrzymuje poprzednie pisanie, jeœli gracz klika szybko
        StartCoroutine(TypeSentence(sentence));
>>>>>>> 3b8118b3d6c8cf75ee511cf59f48b463c2c6f15c
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

            // Graj dï¿½wiï¿½k tylko co X liter i pomijaj spacje
            if (charCount % frequency == 0 && !char.IsWhiteSpace(letter))
            {
                if (typingSound != null && source != null)
                {
                    // Zmiana Pitcha (tonacji) dla efektu "gadania"
                    source.pitch = Random.Range(minPitch, maxPitch);
                    source.PlayOneShot(typingSound);
                }
            }

            yield return new WaitForSeconds(0.03f); // Szybkoï¿½ï¿½ pisania
        }
<<<<<<< HEAD
        isTyping = false; // --- ZMIANA 4: Skoï¿½czyliï¿½my pisaï¿½
=======
>>>>>>> 3b8118b3d6c8cf75ee511cf59f48b463c2c6f15c
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
        // Przewijanie dialogu spacjï¿½ lub myszkï¿½
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
<<<<<<< HEAD
            if (isTyping)
            {
                // Jeï¿½li tekst siï¿½ pisze -> Przerwij i pokaï¿½ caï¿½oï¿½ï¿½
                StopAllCoroutines();
                dialogueText.text = currentSentence;
                isTyping = false;
            }
            else
            {
                // Jeï¿½li tekst jest juï¿½ caï¿½y -> Pokaï¿½ nastï¿½pny
                DisplayNextSentence();
            }
=======
            DisplayNextSentence();
>>>>>>> 3b8118b3d6c8cf75ee511cf59f48b463c2c6f15c
        }
    }
}