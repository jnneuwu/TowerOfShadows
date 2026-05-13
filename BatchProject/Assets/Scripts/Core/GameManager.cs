using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentFloor = 1;
    public int totalFloors = 3;
    public bool isPaused = false;
    public bool isGameOver = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (UIManager.Instance != null) UIManager.Instance.ShowPauseMenu(false);
    }

    public void LoadFloor(int floor)
    {
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        currentFloor = floor;
        SceneManager.LoadScene("Floor" + floor);
    }

    public void NextFloor()
    {
        if (currentFloor < totalFloors)
        {
            LoadFloor(currentFloor + 1);
        }
        else
        {
            SceneManager.LoadScene("Victory");
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;
        if (UIManager.Instance != null) UIManager.Instance.ShowDeathScreen();
    }

    public void RestartGame()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        currentFloor = 1;
        SceneManager.LoadScene("Floor1");
    }

    public void LoadMainMenu()
    {
        isGameOver = false;
        Time.timeScale = 1f;
        currentFloor = 1;
        SceneManager.LoadScene("MainMenu");
    }
}
