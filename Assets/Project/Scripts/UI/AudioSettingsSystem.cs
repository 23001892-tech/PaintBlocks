using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsSystem : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private Button closeSettingsButton;

    [Header("Music UI")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicValueText;

    [Header("SFX UI")]
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text sfxValueText;

    [Header("Text UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text musicLabelText;
    [SerializeField] private TMP_Text sfxLabelText;
    [SerializeField] private TMP_Text closeButtonText;

    [Header("Default Values")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSfxVolume = 0.85f;

    [Header("PlayerPrefs Keys")]
    [SerializeField] private string musicVolumeKey = "PAINT_BLOCK_MUSIC_VOLUME";
    [SerializeField] private string sfxVolumeKey = "PAINT_BLOCK_SFX_VOLUME";
    [SerializeField] private string musicEnabledKey = "PAINT_BLOCK_MUSIC_ENABLED";
    [SerializeField] private string sfxEnabledKey = "PAINT_BLOCK_SFX_ENABLED";

    private float currentMusicVolume;
    private float currentSfxVolume;
    private bool musicEnabled;
    private bool sfxEnabled;

    private bool isApplyingSavedSettings;

    private void Awake()
    {
        LoadSettings();
        SetupUI();
        SetupButtons();
        ApplyDefaultText();
    }

    private void Start()
    {
        HideSettingsPanelInstant();
        ApplySettingsToAudioSystem();
        RefreshUI();
    }

    private void SetupButtons()
    {
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.RemoveListener(OpenSettings);
            openSettingsButton.onClick.AddListener(OpenSettings);
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
            closeSettingsButton.onClick.AddListener(CloseSettings);
        }

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
            musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
        }

        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChanged);
            sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void SetupUI()
    {
        isApplyingSavedSettings = true;

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = currentMusicVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = currentSfxVolume;
        }

        if (musicToggle != null)
        {
            musicToggle.isOn = musicEnabled;
        }

        if (sfxToggle != null)
        {
            sfxToggle.isOn = sfxEnabled;
        }

        isApplyingSavedSettings = false;
    }

    private void ApplyDefaultText()
    {
        if (titleText != null)
        {
            titleText.text = "Cài đặt âm thanh";
        }

        if (musicLabelText != null)
        {
            musicLabelText.text = "Nhạc nền";
        }

        if (sfxLabelText != null)
        {
            sfxLabelText.text = "Hiệu ứng";
        }

        if (closeButtonText != null)
        {
            closeButtonText.text = "Đóng";
        }
    }

    public void OpenSettings()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        RefreshUI();
    }

    public void CloseSettings()
    {
        GameAudioSystem.Instance?.PlayButtonClick();

        SaveSettings();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void HideSettingsPanelInstant()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void OnMusicToggleChanged(bool value)
    {
        if (isApplyingSavedSettings)
            return;

        musicEnabled = value;

        ApplySettingsToAudioSystem();
        SaveSettings();
        RefreshUI();

        GameAudioSystem.Instance?.PlayButtonClick();
    }

    private void OnSfxToggleChanged(bool value)
    {
        if (isApplyingSavedSettings)
            return;

        sfxEnabled = value;

        ApplySettingsToAudioSystem();
        SaveSettings();
        RefreshUI();

        if (sfxEnabled)
        {
            GameAudioSystem.Instance?.PlayButtonClick();
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        if (isApplyingSavedSettings)
            return;

        currentMusicVolume = Mathf.Clamp01(value);

        if (currentMusicVolume > 0f)
        {
            musicEnabled = true;

            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(true);
            }
        }

        ApplySettingsToAudioSystem();
        SaveSettings();
        RefreshUI();
    }

    private void OnSfxSliderChanged(float value)
    {
        if (isApplyingSavedSettings)
            return;

        currentSfxVolume = Mathf.Clamp01(value);

        if (currentSfxVolume > 0f)
        {
            sfxEnabled = true;

            if (sfxToggle != null)
            {
                sfxToggle.SetIsOnWithoutNotify(true);
            }
        }

        ApplySettingsToAudioSystem();
        SaveSettings();
        RefreshUI();
    }

    private void ApplySettingsToAudioSystem()
    {
        if (GameAudioSystem.Instance == null)
            return;

        float appliedMusicVolume = musicEnabled ? currentMusicVolume : 0f;
        float appliedSfxVolume = sfxEnabled ? currentSfxVolume : 0f;

        GameAudioSystem.Instance.SetMusicVolume(appliedMusicVolume);
        GameAudioSystem.Instance.SetSfxVolume(appliedSfxVolume);
    }

    private void RefreshUI()
    {
        if (musicValueText != null)
        {
            int percent = Mathf.RoundToInt(currentMusicVolume * 100f);
            musicValueText.text = musicEnabled ? $"{percent}%" : "Tắt";
        }

        if (sfxValueText != null)
        {
            int percent = Mathf.RoundToInt(currentSfxVolume * 100f);
            sfxValueText.text = sfxEnabled ? $"{percent}%" : "Tắt";
        }

        if (musicSlider != null)
        {
            musicSlider.interactable = musicEnabled;
        }

        if (sfxSlider != null)
        {
            sfxSlider.interactable = sfxEnabled;
        }
    }

    private void LoadSettings()
    {
        currentMusicVolume = PlayerPrefs.GetFloat(musicVolumeKey, defaultMusicVolume);
        currentSfxVolume = PlayerPrefs.GetFloat(sfxVolumeKey, defaultSfxVolume);

        musicEnabled = PlayerPrefs.GetInt(musicEnabledKey, 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt(sfxEnabledKey, 1) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(musicVolumeKey, currentMusicVolume);
        PlayerPrefs.SetFloat(sfxVolumeKey, currentSfxVolume);

        PlayerPrefs.SetInt(musicEnabledKey, musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt(sfxEnabledKey, sfxEnabled ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void ResetAudioSettings()
    {
        currentMusicVolume = defaultMusicVolume;
        currentSfxVolume = defaultSfxVolume;
        musicEnabled = true;
        sfxEnabled = true;

        SetupUI();
        ApplySettingsToAudioSystem();
        SaveSettings();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (openSettingsButton != null)
        {
            openSettingsButton.onClick.RemoveListener(OpenSettings);
        }

        if (closeSettingsButton != null)
        {
            closeSettingsButton.onClick.RemoveListener(CloseSettings);
        }

        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
        }

        if (sfxToggle != null)
        {
            sfxToggle.onValueChanged.RemoveListener(OnSfxToggleChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }
    }
}