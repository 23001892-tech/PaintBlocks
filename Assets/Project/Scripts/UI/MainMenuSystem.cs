using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuSystem : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToButton;
    [SerializeField] private Button quitButton;

    [Header("How To Panel")]
    [SerializeField] private GameObject howToPanel;
    [SerializeField] private Button closeHowToButton;
    [SerializeField] private TMP_Text howToTitleText;
    [SerializeField] private TMP_Text howToContentText;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Title Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("How To Content")]
    [TextArea(8, 20)]
    [SerializeField] private string howToContent =
        "Ghép 2 màu gốc để tạo màu mới:\n\n" +
        "Đỏ + Vàng = Cam\n" +
        "Vàng + Lam = Lục\n" +
        "Lam + Đỏ = Tím\n\n" +
        "Tránh màu thứ ba:\n\n" +
        "Lam cạnh Cam = Tro\n" +
        "Đỏ cạnh Lục = Tro\n" +
        "Vàng cạnh Tím = Tro\n\n" +
        "Clear hàng hoặc cột để ghi điểm.\n" +
        "Clear đúng màu mục tiêu để nạp Energy.\n" +
        "Đầy Energy thì dùng Bomb nổ vùng 3x3.";

    private void Awake()
    {
        Time.timeScale = 1f;

        SetupButtons();
        ApplyText();
        HidePanelsInstant();
    }

    private void Update()
    {
        // Android Back Button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
            playButton.onClick.AddListener(PlayGame);
        }

        if (howToButton != null)
        {
            howToButton.onClick.RemoveListener(OpenHowTo);
            howToButton.onClick.AddListener(OpenHowTo);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        if (closeHowToButton != null)
        {
            closeHowToButton.onClick.RemoveListener(CloseHowTo);
            closeHowToButton.onClick.AddListener(CloseHowTo);
        }
    }

    private void ApplyText()
    {
        if (titleText != null)
        {
            titleText.text = "Paint Blocks";
        }

        if (subtitleText != null)
        {
            subtitleText.text = "Pha màu, xếp khối, phá bảng.";
        }

        if (howToTitleText != null)
        {
            howToTitleText.text = "Cách chơi";
        }

        if (howToContentText != null)
        {
            howToContentText.text = howToContent;
        }
    }

    public void PlayGame()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogWarning("MainMenuSystem: Game Scene Name đang trống.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenHowTo()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        if (howToPanel != null)
        {
            howToPanel.SetActive(true);
        }
    }

    public void CloseHowTo()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        if (howToPanel != null)
        {
            howToPanel.SetActive(false);
        }
    }

    private void HandleBackButton()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            GameAudioSystem.Instance?.PlayButtonClick();
            settingsPanel.SetActive(false);
            return;
        }

        if (howToPanel != null && howToPanel.activeSelf)
        {
            GameAudioSystem.Instance?.PlayButtonClick();
            howToPanel.SetActive(false);
            return;
        }

        QuitGame();
    }

    public void QuitGame()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        Debug.Log("MainMenuSystem: Quit Game");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HidePanelsInstant()
    {
        if (howToPanel != null)
        {
            howToPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
        }

        if (howToButton != null)
        {
            howToButton.onClick.RemoveListener(OpenHowTo);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }

        if (closeHowToButton != null)
        {
            closeHowToButton.onClick.RemoveListener(CloseHowTo);
        }
    }
}