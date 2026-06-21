using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button homeButton;

    [Header("Text References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text pauseButtonText;
    [SerializeField] private TMP_Text resumeButtonText;
    [SerializeField] private TMP_Text restartButtonText;
    [SerializeField] private TMP_Text homeButtonText;

    [Header("Scene Settings")]
    [SerializeField] private string homeSceneName = "MainMenu";
    [SerializeField] private bool useHomeButton = true;

    [Header("Audio Settings")]
    [SerializeField] private bool pauseBackgroundMusicWhilePaused = false;

    [Header("Keyboard Test")]
    [SerializeField] private bool enableEscapeKey = true;

    private bool isPaused;
    private float previousTimeScale = 1f;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        SetupButtons();
        ApplyDefaultText();
    }

    private void Start()
    {
        HidePausePanelInstant();
    }

    private void Update()
    {
        if (!enableEscapeKey)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void SetupButtons()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
            pauseButton.onClick.AddListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(GoHome);
            homeButton.onClick.AddListener(GoHome);
            homeButton.gameObject.SetActive(useHomeButton);
        }
    }

    private void ApplyDefaultText()
    {
        if (titleText != null)
        {
            titleText.text = "Tạm dừng";
        }

        if (pauseButtonText != null)
        {
            pauseButtonText.text = "II";
        }

        if (resumeButtonText != null)
        {
            resumeButtonText.text = "Tiếp tục";
        }

        if (restartButtonText != null)
        {
            restartButtonText.text = "Chơi lại";
        }

        if (homeButtonText != null)
        {
            homeButtonText.text = "Trang chủ";
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        GameAudioSystem.Instance?.PlayButtonClick();

        isPaused = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (pauseBackgroundMusicWhilePaused)
        {
            GameAudioSystem.Instance?.PauseBackgroundMusic();
        }
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        GameAudioSystem.Instance?.PlayButtonClick();

        isPaused = false;

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseBackgroundMusicWhilePaused)
        {
            GameAudioSystem.Instance?.ResumeBackgroundMusic();
        }
    }

    public void RestartGame()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        Time.timeScale = 1f;
        isPaused = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void GoHome()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        if (string.IsNullOrWhiteSpace(homeSceneName))
        {
            Debug.LogWarning("PauseMenuSystem: Home Scene Name đang trống.");
            return;
        }

        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene(homeSceneName);
    }

    private void HidePausePanelInstant()
    {
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(GoHome);
        }
    }
}