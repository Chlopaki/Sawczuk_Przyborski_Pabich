using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
//kupa
// CHYBA TWOJA DZIADZIE!!!
public enum GameState
{
    [InspectorName("Gameplay")] GAME,
    [InspectorName("Pause")] PAUSE_MENU,
    [InspectorName("Level completed")] LEVEL_COMPLETED,
    [InspectorName("Options")] GS_OPTIONS, // Dodano stan opcji
    [InspectorName("Dialogue")] DIALOGUE
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game State")]
    [SerializeField] public GameState currentGameState = GameState.GAME;

    [Header("Game Stats")]
    public int score = 0;
    public int livesNum = 3;
    public int keyNum = 0;
    private int defeatedEnemies = 0;
    private float gameTime = 0f;

    [Header("UI Objects (Canvas)")]
    [SerializeField] public Canvas gameCanvas;
    [SerializeField] public Canvas pauseMenuCanvas;
    [SerializeField] public Canvas levelCompleted;

    [Header("UI References")]
    [SerializeField] private Image[] keyIcons;

    [Header("Settings")]
    [SerializeField] private Color lockedColor = Color.black;
    [SerializeField] private Color unlockedColor = Color.white;

    [Header("UI Text References")]
    public TMP_Text scoreLabel;       // Wynik w trakcie gry
    public TMP_Text finalScoreLabel;  // Wynik na ekranie koñcowym
    public TMP_Text livesText;        // Licznik serc
    public TMP_Text timeText;         // Licznik czasu
    public TMP_Text enemiesText;      // Licznik wrogów
    public TMP_Text keyText;          // Licznik kluczy


    // Tablica booli dla konkretnych kluczy 
    private bool[] collectedKeys = new bool[3];

    [Header("Options UI")]
    [SerializeField] public Canvas optionsCanvas; // Referencja do Canvasu opcji

    [Header("NPC Moving")]
    [SerializeField] public GameObject gepardNPC; // Przeci¹gniesz tu obiekt Geparda
    [SerializeField] public Transform gepardDestination; // Przeci¹gniesz tu ten pusty punkt w gara¿u
    [TextArea(3, 10)] // To sprawia, ¿e w Inspectorze bêdzie du¿e pole do pisania
    [SerializeField] public string[] victoryDialogue;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Duplicated Game Manager", gameObject);
            Destroy(gameObject);
        }

        // Inicjalizacja tekstu wyniku na starcie
        if (scoreLabel != null) scoreLabel.text = score.ToString();
    }

    void Start()
    {
        SetGameState(GameState.GAME);
        UpdateUI(); // Wa¿ne: Odœwie¿ UI na starcie, ¿eby pokazaæ pocz¹tkowe ¿ycia i wrogów
        
    }

    void Update()
    {
        // Obs³uga pauzy (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.PAUSE_MENU)
                SetGameState(GameState.GAME);
            else if (currentGameState == GameState.GAME)
                SetGameState(GameState.PAUSE_MENU);
        }

        // Obs³uga Czasu (tylko podczas gry)
        if (currentGameState == GameState.GAME)
        {
            gameTime += Time.deltaTime;

            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60F);
                int seconds = Mathf.FloorToInt(gameTime % 60F);
                timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    void UpdateKeyUI()
    {
        for (int i = 0; i < keyIcons.Length; i++)
        {
            // Sprawdzamy konkretny indeks: Czy mamy klucz nr "i"?
            if (collectedKeys[i] == true)
            {
                keyIcons[i].color = unlockedColor; // Poka¿ kolor
            }
            else
            {
                keyIcons[i].color = lockedColor; // Poka¿ cieñ
            }
        }
    }

    // Metoda do odœwie¿ania liczników (¯ycia i Wrogowie)
    void UpdateUI()
    {
        if (livesText != null)
            livesText.text = livesNum.ToString();

        if (enemiesText != null)
            enemiesText.text = defeatedEnemies.ToString();

        UpdateKeyUI();
        
    }

    public void AddPoints(int points)
    {
        score += points;
        Debug.Log("Aktualny Wynik: " + score);
        if (scoreLabel != null) scoreLabel.text = score.ToString();
    }

    public void AddLife(int value)
    {
        livesNum += value;

        // Zabezpieczenie przed œmierci¹
        if (livesNum <= 0)
        {
            livesNum = 0;
            GameOver();
        }

        UpdateUI(); // Odœwie¿ licznik serc
    }

    public void AddKey(KeyColor color)
    {
        int keyIndex = (int)color;
        collectedKeys[keyIndex] = true;
        keyNum ++;
        UpdateUI();
    }

    public void AddEnemyKill()
    {
        defeatedEnemies++;
        UpdateUI(); // Odœwie¿ licznik wrogów
    }

    // Zarz¹dzanie stanami gry
    void SetGameState(GameState newGameState)
    {
        currentGameState = newGameState;

        // W³¹cz/Wy³¹cz Canvasy w zale¿noœci od stanu
        if (gameCanvas != null)
            gameCanvas.enabled = (currentGameState == GameState.GAME);

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.enabled = (currentGameState == GameState.PAUSE_MENU);
        if (optionsCanvas != null) optionsCanvas.enabled = (currentGameState == GameState.GS_OPTIONS); //

        // Zamro¿enie czasu w menu
        Time.timeScale = (currentGameState == GameState.PAUSE_MENU || currentGameState == GameState.GS_OPTIONS) ? 0f : 1f;

        if (levelCompleted != null)
        {
            // Jeœli ukoñczono poziom, w³¹cz ekran koñcowy
            if (newGameState == GameState.LEVEL_COMPLETED)
            {
                levelCompleted.enabled = true;
                if (finalScoreLabel != null)
                {
                    finalScoreLabel.text = "SCORE: " + score.ToString();
                }
            }
            else
            {
                levelCompleted.enabled = false;
            }
        }
    }

    public void MoveGepard()
    {
        if (gepardNPC != null && gepardDestination != null)
        {
            Debug.Log("Przenoszenie Geparda do gara¿u...");
            gepardNPC.transform.position = gepardDestination.position;
            NPC_Dialogue npcScript = gepardNPC.GetComponent<NPC_Dialogue>();

            if (npcScript != null)
            {
                npcScript.sentences = victoryDialogue;

                Debug.Log("Dialog Geparda zosta³ zaktualizowany!");
            }
        }
    }

    public void Options() => SetGameState(GameState.GS_OPTIONS);

    public void SetVolume(float vol) => AudioListener.volume = vol; // Ustawienie g³oœnoœci

    public void IncreaseQuality() => QualitySettings.IncreaseLevel(); // Zwiêkszenie jakoœci
    public void DecreaseQuality() => QualitySettings.DecreaseLevel(); // Zmniejszenie jakoœci

    public string GetQualityName() => QualitySettings.names[QualitySettings.GetQualityLevel()]; // Nazwa jakoœci

    // --- Metody dla Przycisków UI ---
    public void OnResumeButtonClick()
    {
        InGame();
    }

    public void OnRestartButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnMenuButtonClick()
    {
        Time.timeScale = 1f; // Upewnij siê, ¿e czas p³ynie po wyjœciu do menu
        SceneManager.LoadScene("MainMenu");
    }

    // --- Metody Pomocnicze ---
    public void LevelCompleted()
    {
        SetGameState(GameState.LEVEL_COMPLETED);
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        // Tutaj mo¿na dodaæ osobny ekran Game Over, na razie u¿ywamy LevelCompleted lub restartu
        SetGameState(GameState.LEVEL_COMPLETED);
    }

    void PauseMenu()
    {
        SetGameState(GameState.PAUSE_MENU);
    }

    void InGame()
    {
        SetGameState(GameState.GAME);
    }
}