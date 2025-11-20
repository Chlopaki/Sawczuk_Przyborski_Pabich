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
    private int keysFound = 0;
    public static int maxKeys = 3; //iloœæ kluczy na planszy
    public int livesNum = 3; // iloœæ ¿yæ start - 3
    public bool keysCompleted = false;
    public Canvas gameCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    /* void Update()
     {
         if (Input.GetKeyDown(KeyCode.Escape))
         {
             if (currentGameState == GameState.PAUSE_MENU) currentGameState = GameState.GAME;
             else currentGameState = GameState.PAUSE_MENU;
         }
     }*/

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentGameState == GameState.PAUSE_MENU)
            {
                SetGameState(GameState.GAME); // U¿ywamy metody, aby odœwie¿yæ Canvas
            }
            else
            {
                SetGameState(GameState.PAUSE_MENU); // U¿ywamy metody, aby ukryæ Canvas
            }
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

    public void AddKeys()
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
    }

    public void AddLife(int liveParam)
    {
        livesNum += liveParam;
    }

    /*void SetGameState(GameState newGameState)
    {
        currentGameState = newGameState;
    }*/

    void SetGameState(GameState newGameState)
    {
        currentGameState = newGameState;

        // Sprawdzamy, czy gameCanvas zosta³ przypisany w inspektorze, aby unikn¹æ b³êdów
        if (gameCanvas != null)
        {
            // Canvas jest w³¹czony (enabled = true) TYLKO wtedy, gdy stan to GAME.
            // W ka¿dym innym przypadku (PAUSE_MENU, LEVEL_COMPLETED) bêdzie false.
            gameCanvas.enabled = (currentGameState == GameState.GAME);
        }
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

