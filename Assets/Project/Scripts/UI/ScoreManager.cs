using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    [Header("Effects")]
    [SerializeField] private FloatingTextSystem floatingTextSystem;

    [Header("Clear Score")]
    [SerializeField] private int scorePerLine = 10;

    [Header("Combo Settings")]
    [SerializeField] private int multiLineBonusPerExtraLine = 10;
    [SerializeField] private int comboStreakBonus = 10;

    private int score;
    private int comboStreak;

    public int CurrentScore => score;
    public int CurrentComboStreak => comboStreak;

    private void Awake()
    {
        if (floatingTextSystem == null)
        {
            floatingTextSystem = FindAnyObjectByType<FloatingTextSystem>();
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void AddClearScore(int clearedLineCount, int targetMatchedLineCount)
    {
        if (clearedLineCount <= 0)
        {
            comboStreak = 0;
            RefreshUI();
            return;
        }

        comboStreak++;

        int safeTargetMatchedLineCount = Mathf.Clamp(targetMatchedLineCount, 0, clearedLineCount);
        int normalLineCount = clearedLineCount - safeTargetMatchedLineCount;

        int normalScore = normalLineCount * scorePerLine;
        int targetScore = safeTargetMatchedLineCount * scorePerLine * 2;

        int multiLineBonus = 0;
        if (clearedLineCount >= 2)
        {
            multiLineBonus = (clearedLineCount - 1) * multiLineBonusPerExtraLine;
        }

        int comboBonus = 0;
        if (comboStreak >= 2)
        {
            comboBonus = (comboStreak - 1) * comboStreakBonus;
        }

        int addScore = normalScore + targetScore + multiLineBonus + comboBonus;

        score += addScore;
        RefreshUI();

        if (floatingTextSystem != null)
        {
            bool hasTargetClear = safeTargetMatchedLineCount > 0;

            floatingTextSystem.ShowScoreText(addScore, hasTargetClear);

            if (comboStreak >= 2)
            {
                floatingTextSystem.ShowComboText(comboStreak);
            }
        }

        Debug.Log($"ScoreManager: Cleared {clearedLineCount} line(s), target matched {safeTargetMatchedLineCount}, combo {comboStreak}, +{addScore}");
    }

    public void AddBonusScore(int amount)
    {
        if (amount <= 0)
            return;

        score += amount;
        RefreshUI();

        if (floatingTextSystem != null)
        {
            floatingTextSystem.ShowBombText(amount);
        }

        Debug.Log($"ScoreManager: Bonus +{amount}");
    }

    public void ResetScore()
    {
        score = 0;
        comboStreak = 0;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (comboText != null)
        {
            bool showCombo = comboStreak >= 2;
            comboText.gameObject.SetActive(showCombo);

            if (showCombo)
            {
                comboText.text = $"Combo x{comboStreak}";
            }
        }
    }
}