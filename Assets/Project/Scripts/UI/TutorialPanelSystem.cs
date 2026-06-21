using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanelSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text contentText;

    [Header("Settings")]
    [SerializeField] private bool showOnFirstLaunchOnly = true;
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private string tutorialSeenKey = "PAINT_BLOCK_TUTORIAL_SEEN";

    [Header("Content")]
    [TextArea(8, 20)]
    [SerializeField] private string tutorialContent =
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

    private bool isOpen;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveListener(OpenTutorial);
            tutorialButton.onClick.AddListener(OpenTutorial);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTutorial);
            closeButton.onClick.AddListener(CloseTutorial);
        }

        ApplyText();
    }

    private void Start()
    {
        bool shouldShowTutorial = ShouldShowTutorialOnStart();

        if (shouldShowTutorial)
        {
            OpenTutorial();
        }
        else
        {
            HidePanelInstant();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            OpenTutorial();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            ResetTutorialSeen();
        }
    }
#endif

    public void OpenTutorial()
    {
        if (tutorialPanel == null)
        {
            Debug.LogWarning("TutorialPanelSystem: Chưa gán TutorialPanel.");
            return;
        }

        GameAudioSystem.Instance?.PlayButtonClick();

        isOpen = true;

        tutorialPanel.SetActive(true);

        if (pauseGameWhileOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        ApplyText();
    }

    public void CloseTutorial()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        isOpen = false;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (pauseGameWhileOpen)
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }

        PlayerPrefs.SetInt(tutorialSeenKey, 1);
        PlayerPrefs.Save();
    }

    public void ResetTutorialSeen()
    {
        PlayerPrefs.DeleteKey(tutorialSeenKey);
        PlayerPrefs.Save();

        Debug.Log("TutorialPanelSystem: Đã reset trạng thái xem tutorial. Lần Play sau tutorial sẽ hiện lại.");
    }

    private bool ShouldShowTutorialOnStart()
    {
        if (!showOnFirstLaunchOnly)
            return true;

        return PlayerPrefs.GetInt(tutorialSeenKey, 0) == 0;
    }

    private void HidePanelInstant()
    {
        isOpen = false;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    private void ApplyText()
    {
        if (titleText != null)
        {
            titleText.text = "Cách chơi";
        }

        if (contentText != null)
        {
            contentText.text = tutorialContent;
        }
    }

    private void OnDestroy()
    {
        if (isOpen && pauseGameWhileOpen)
        {
            Time.timeScale = 1f;
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.RemoveListener(OpenTutorial);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTutorial);
        }
    }
}