using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLevel1ButtonPressed()
    {
        SceneManager.LoadScene("118");
    }

    public void OnLevel2ButtonPressed()
    {
        SceneManager.LoadScene("Level2");
    }

    public void OnLevel3ButtonPressed()
    {
        SceneManager.LoadScene("Level 3");
    }

    public void OnLevel4ButtonPressed()
    {
        SceneManager.LoadScene("Level 4");
    }



    public void OnExitButtonPressed()
    {

#if UNITY_EDITOR
UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}
