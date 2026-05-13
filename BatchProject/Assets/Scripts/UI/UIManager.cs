using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public Slider hpSlider;
    public Text hpText;
    public Text ammoText;
    public Text floorText;

    [Header("Boss")]
    public GameObject bossHPPanel;
    public Slider bossHPSlider;
    public Text bossNameText;

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject deathPanel;

    void Awake() { Instance = this; }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        if (bossHPPanel != null) bossHPPanel.SetActive(false);
        UpdateFloor();
    }

    public void UpdateHP(int current, int max)
    {
        if (hpSlider != null) hpSlider.value = (float)current / max;
        if (hpText != null) hpText.text = current + " / " + max;
    }

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null) ammoText.text = "AMMO: " + current + " / " + max;
    }

    public void UpdateFloor()
    {
        if (floorText != null && GameManager.Instance != null)
            floorText.text = "FLOOR " + GameManager.Instance.currentFloor + " / " + GameManager.Instance.totalFloors;
    }

    public void ShowBossHP(string name, float pct)
    {
        if (bossHPPanel != null) bossHPPanel.SetActive(true);
        if (bossNameText != null) bossNameText.text = name;
        if (bossHPSlider != null) bossHPSlider.value = pct;
    }

    public void HideBossHP()
    {
        if (bossHPPanel != null) bossHPPanel.SetActive(false);
    }

    public void ShowPauseMenu(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
    }

    public void ShowDeathScreen()
    {
        if (deathPanel != null) deathPanel.SetActive(true);
    }

    public void OnResumeClick()
    {
        if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
    }

    public void OnRestartClick()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartGame();
    }

    public void OnMainMenuClick()
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
    }
}
