using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState
{
    [InspectorName("Gameplay")] GAME,
    [InspectorName("Pause")] PAUSE_MENU,
    [InspectorName("Level completed (either successfully or failed)")] LEVEL_COMPLETED
}




public class GameManager : MonoBehaviour
{
    [SerializeField] public GameState currentGameState = GameState.GAME;
    public static GameManager instance;
    private int score = 0;
    //private int keysFound = 0;
    //public static int maxKeys = 3; //iloœæ kluczy na planszy
    public int livesNum = 3; // iloœæ ¿yæ start - 3
    //public bool keysCompleted = false;
    [SerializeField] public Canvas gameCanvas;
    public TMP_Text scoreLabel;
    [SerializeField] public Canvas pauseMenuCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
     void Update()
     {
         if (Input.GetKeyDown(KeyCode.Escape))
         {
             if (currentGameState == GameState.PAUSE_MENU) currentGameState = GameState.GAME;
             else currentGameState = GameState.PAUSE_MENU;
         }
        pauseMenuCanvas.enabled = (currentGameState == GameState.PAUSE_MENU);
        gameCanvas.enabled = (currentGameState == GameState.GAME);
    }



    void Awake()
    {
        // Jeœli NIE MA instancji ? przypisz tê
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // Jeœli instancja ju¿ istnieje ? b³¹d i zniszczenie duplikatu
            Debug.LogError("Duplicated Game Manager", gameObject);
            Destroy(gameObject);
        }
        scoreLabel.text = score.ToString();
    }

    public void AddPoints(int points)
    {
        score += points;
        Debug.Log("Aktualny Wynik: " + score);
        scoreLabel.text = score.ToString();
    }

    /*public void AddKeys()
    {
        keysFound = keysFound + 1;
        if (maxKeys == keysFound)
        {
            keysCompleted = true;
            Debug.Log("Zebrano Wszystkie klucze");
        }
        else
        {
            Debug.Log("Zebrano: " + keysFound + " kluczy");
        }
    }*/

    public void AddLife(int liveParam)
    {
        livesNum += liveParam;
    }


    void SetGameState(GameState newGameState)
    {
        currentGameState = newGameState;
        pauseMenuCanvas.enabled = (currentGameState == GameState.PAUSE_MENU);

    }

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
        pauseMenuCanvas.enabled = false;
        SceneManager.LoadScene("MainMenu");
    }

    void PauseMenu()
    {
        SetGameState(GameState.PAUSE_MENU);
    }
    void InGame()
    {
        SetGameState(GameState.GAME);

        if (gameCanvas != null)
        {
            gameCanvas.enabled = (currentGameState == GameState.GAME);
        }
    }
    void LevelCompleted()
    {
        SetGameState(GameState.LEVEL_COMPLETED);
    }
    void GameOver()
    {
        SetGameState(GameState.LEVEL_COMPLETED);
    }
}

