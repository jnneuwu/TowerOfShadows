using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public void StartGame()
    {
        EnsureGameManager();
        GameManager.Instance.currentFloor = 1;
        GameManager.Instance.totalFloors = 3;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Floor1");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
            return;
        }

        SceneManager.LoadScene("Floor1");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void EnsureGameManager()
    {
        if (GameManager.Instance != null) return;

        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
    }
}
