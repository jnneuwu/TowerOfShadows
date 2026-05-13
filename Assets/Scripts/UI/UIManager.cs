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
    public Text weaponNameText;
    public Text interactPromptText;

    [Header("State Debug Panel (top-left)")]
    public Text stateText;       // State: Running
    public Text speedText;       // Speed: 1.0
    public Text directionText;   // Direction: Right
    public Text aimText;         // e.g. "Aim: 45 deg"
    public Text groundedText;    // Grounded: True

    [Header("Sound Toggle Indicators")]
    public Text musicIndicatorText; // M: Music ON
    public Text sfxIndicatorText;   // N: SFX ON

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
        HideInteractPrompt();
        UpdateFloor();
        RefreshSoundIndicators();
    }

    void Update()
    {
        // Refresh the M / N toggle indicators every frame so changes show immediately.
        RefreshSoundIndicators();
    }

    /// <summary>Update the top-left state debug panel.</summary>
    public void UpdateStateDebug(string state, float speed, string direction, float aimAngle, bool grounded)
    {
        if (stateText != null)     stateText.text     = "State: "     + state;
        if (speedText != null)     speedText.text     = "Speed: "     + speed.ToString("F1");
        if (directionText != null) directionText.text = "Direction: " + direction;
        if (aimText != null)       aimText.text       = "Aim: "       + aimAngle.ToString("F0") + " deg";
        if (groundedText != null)  groundedText.text  = "Grounded: "  + grounded;
    }

    /// <summary>Sync the music / SFX toggle indicators with the AudioManager.</summary>
    public void RefreshSoundIndicators()
    {
        if (AudioManager.Instance == null) return;
        if (musicIndicatorText != null)
        {
            bool on = AudioManager.Instance.IsMusicOn;
            musicIndicatorText.text  = "[M] Music: " + (on ? "ON" : "OFF");
            musicIndicatorText.color = on ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.45f, 0.45f);
        }
        if (sfxIndicatorText != null)
        {
            bool on = AudioManager.Instance.IsSFXOn;
            sfxIndicatorText.text  = "[N] SFX: " + (on ? "ON" : "OFF");
            sfxIndicatorText.color = on ? new Color(0.4f, 1f, 0.5f) : new Color(1f, 0.45f, 0.45f);
        }
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

    public void UpdateWeaponName(string name)
    {
        if (weaponNameText != null) weaponNameText.text = name;
    }

    public void UpdateFloor()
    {
        if (floorText != null && GameManager.Instance != null)
            floorText.text = "FLOOR " + GameManager.Instance.currentFloor + " / " + GameManager.Instance.totalFloors;
    }

    public void ShowInteractPrompt(string text)
    {
        if (interactPromptText != null)
        {
            interactPromptText.text = text;
            interactPromptText.gameObject.SetActive(true);
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);
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
