using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;
//kupa
public enum GameState
{
    [InspectorName("Gameplay")] GAME,
    [InspectorName("Pause")] PAUSE_MENU,
    [InspectorName("Level completed")] LEVEL_COMPLETED
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Game State")]
    [SerializeField] public GameState currentGameState = GameState.GAME;

    [Header("Game Stats")]
    public int score = 0;
    public int livesNum = 3;    // G³ówna zmienna ¿yæ (zmieniona z 3 na startow¹ wartoœæ)
    private int defeatedEnemies = 0;
    private float gameTime = 0f;

    [Header("UI Objects (Canvas)")]
    [SerializeField] public Canvas gameCanvas;
    [SerializeField] public Canvas pauseMenuCanvas;
    [SerializeField] public Canvas levelCompleted;

    [Header("UI Text References")]
    public TMP_Text scoreLabel;       // Wynik w trakcie gry
    public TMP_Text finalScoreLabel;  // Wynik na ekranie koñcowym
    public TMP_Text livesText;        // Licznik serc
    public TMP_Text timeText;         // Licznik czasu
    public TMP_Text enemiesText;      // Licznik wrogów

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
    }

    // Metoda do odœwie¿ania liczników (¯ycia i Wrogowie)
    void UpdateUI()
    {
        if (livesText != null)
            livesText.text = livesNum.ToString();

        if (enemiesText != null)
            enemiesText.text = defeatedEnemies.ToString();
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

        if (levelCompleted != null)
        {
            // Jeœli ukoñczono poziom, w³¹cz ekran koñcowy
            if (newGameState == GameState.LEVEL_COMPLETED)
            {
                levelCompleted.enabled = true;
                if (finalScoreLabel != null)
                {
                    finalScoreLabel.text = "Twój wynik: " + score.ToString();
                }
            }
            else
            {
                levelCompleted.enabled = false;
            }
        }
    }

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