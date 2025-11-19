using UnityEngine;

public enum GameState
{
    [InspectorName("Gameplay")] GAME,
    [InspectorName("Pause")] PAUSE_MENU,
    [InspectorName("Level completed (either successfully or failed)")] LEVEL_COMPLETED
}




public class GameManager : MonoBehaviour
{
    public GameState currentGameState = GameState.PAUSE_MENU;
    public static GameManager instance;
    private int score = 0;

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
    }

    public void AddPoints(int points)
    {
        score += points;
        Debug.Log("Aktualny Wynik: " + score);
    }

    void SetGameState(GameState newGameState)
    {
        currentGameState = newGameState;
    }
    void PauseMenu()
    {
        SetGameState(GameState.PAUSE_MENU);
    }
    void InGame()
    {
        SetGameState(GameState.GAME);
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

