using UnityEngine.SceneManagement;

public class DeathMenu : Menu
{
    public void ContinueGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
