using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : Menu
{
    public void QuitGame()
    {
        Application.Quit();
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
