using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetColorSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ScoreManager scoreManager;

    [Header("UI")]
    [SerializeField] private TMP_Text targetColorText;
    [SerializeField] private Image targetColorIcon;
    [SerializeField] private Image energyFillImage;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private Button colorBombButton;
    [SerializeField] private TMP_Text colorBombButtonText;

    [Header("Energy Settings")]
    [SerializeField] private int energyToColorBomb = 100;
    [SerializeField] private int energyGainPerTargetClear = 35;

    [Header("Area Bomb Score")]
    [SerializeField] private int scorePerBombClearedCell = 2;

    [Header("Target Settings")]
    [SerializeField] private bool updateTargetAfterEveryMove = true;

    private PaintColorState currentTargetColor = PaintColorState.Orange;
    private int currentEnergy;

    public PaintColorState CurrentTargetColor => currentTargetColor;
    public int CurrentEnergy => currentEnergy;
    public int EnergyToColorBomb => energyToColorBomb;
    public bool IsColorBombReady => currentEnergy >= energyToColorBomb;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
        }
    }

    private void Start()
    {
        UpdateTargetColorFromBoard();
        RefreshUI();
    }

    public void NotifyMoveEnded()
    {
        if (updateTargetAfterEveryMove)
        {
            UpdateTargetColorFromBoard();
        }

        RefreshUI();
    }

    public void AddTargetClearEnergy(int matchedLineCount = 1)
    {
        if (matchedLineCount <= 0)
            return;

        int energyGain = energyGainPerTargetClear * matchedLineCount;

        currentEnergy += energyGain;

        if (currentEnergy > energyToColorBomb)
        {
            currentEnergy = energyToColorBomb;
        }

        Debug.Log($"TargetColorSystem: Energy +{energyGain}. Current energy = {currentEnergy}/{energyToColorBomb}");

        RefreshUI();
    }

    public void UseColorBomb()
    {
        if (!IsColorBombReady)
        {
            Debug.Log("TargetColorSystem: Area Bomb chưa sẵn sàng.");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("TargetColorSystem: Chưa có GridManager.");
            return;
        }

        int clearedCount = gridManager.ClearBest3x3Area(out int centerRow, out int centerCol);

        if (clearedCount <= 0)
        {
            Debug.Log("TargetColorSystem: Area Bomb không có ô nào để nổ, không trừ energy.");
            RefreshUI();
            return;
        }

        GameAudioSystem.Instance?.PlayBomb();

        currentEnergy = 0;

        if (scoreManager != null)
        {
            scoreManager.AddBonusScore(clearedCount * scorePerBombClearedCell);
        }

        UpdateTargetColorFromBoard();
        RefreshUI();

        Debug.Log($"TargetColorSystem: Used Area Bomb at ({centerRow}, {centerCol}), cleared {clearedCount} cell(s).");
    }

    private void UpdateTargetColorFromBoard()
    {
        if (gridManager == null)
        {
            PickRandomTargetColor();
            return;
        }

        int orangeCount = CountColorOnBoard(PaintColorState.Orange);
        int greenCount = CountColorOnBoard(PaintColorState.Green);
        int purpleCount = CountColorOnBoard(PaintColorState.Purple);

        int maxCount = Mathf.Max(orangeCount, greenCount, purpleCount);

        if (maxCount <= 0)
        {
            PickRandomTargetColor();
            return;
        }

        bool orangeIsMax = orangeCount == maxCount;
        bool greenIsMax = greenCount == maxCount;
        bool purpleIsMax = purpleCount == maxCount;

        int tieCount = 0;

        if (orangeIsMax) tieCount++;
        if (greenIsMax) tieCount++;
        if (purpleIsMax) tieCount++;

        if (tieCount >= 2)
        {
            if (currentTargetColor == PaintColorState.Orange && orangeIsMax)
                return;

            if (currentTargetColor == PaintColorState.Green && greenIsMax)
                return;

            if (currentTargetColor == PaintColorState.Purple && purpleIsMax)
                return;
        }

        if (orangeIsMax)
        {
            currentTargetColor = PaintColorState.Orange;
        }
        else if (greenIsMax)
        {
            currentTargetColor = PaintColorState.Green;
        }
        else
        {
            currentTargetColor = PaintColorState.Purple;
        }

        Debug.Log($"TargetColorSystem: Target updated. Orange={orangeCount}, Green={greenCount}, Purple={purpleCount}. Target={currentTargetColor}");
    }

    private int CountColorOnBoard(PaintColorState colorState)
    {
        int count = 0;

        for (int row = 0; row < GridManager.Rows; row++)
        {
            for (int col = 0; col < GridManager.Columns; col++)
            {
                Cell cell = gridManager.GetCell(row, col);

                if (cell != null && !cell.IsEmpty && cell.ColorState == colorState)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void PickRandomTargetColor()
    {
        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                currentTargetColor = PaintColorState.Orange;
                break;

            case 1:
                currentTargetColor = PaintColorState.Green;
                break;

            default:
                currentTargetColor = PaintColorState.Purple;
                break;
        }

        Debug.Log($"TargetColorSystem: Random target color = {currentTargetColor}");
    }

    private void RefreshUI()
    {
        if (targetColorText != null)
        {
            targetColorText.text = $"Target:\n{GetColorName(currentTargetColor)}";
        }

        RefreshTargetIcon();

        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = currentEnergy / (float)energyToColorBomb;
        }

        if (energyText != null)
        {
            if (IsColorBombReady)
            {
                energyText.text = "Area Bomb\nREADY!";
            }
            else
            {
                energyText.text = $"Energy:\n{currentEnergy}/{energyToColorBomb}";
            }
        }

        if (colorBombButton != null)
        {
            colorBombButton.interactable = IsColorBombReady;
        }

        if (colorBombButtonText != null)
        {
            colorBombButtonText.text = IsColorBombReady ? "BOOM!" : "Bomb";
        }
    }

    private void RefreshTargetIcon()
    {
        if (targetColorIcon == null)
            return;

        Sprite targetSprite = null;

        if (PaintColorSpriteLibrary.Instance != null)
        {
            targetSprite = PaintColorSpriteLibrary.Instance.GetBlockSprite(currentTargetColor);
        }

        if (targetSprite != null)
        {
            targetColorIcon.sprite = targetSprite;
            targetColorIcon.color = Color.white;
            targetColorIcon.type = Image.Type.Simple;
            targetColorIcon.preserveAspect = true;
        }
        else
        {
            targetColorIcon.sprite = null;
            targetColorIcon.color = PaintColorRules.ToColor(currentTargetColor);
        }
    }

    private string GetColorName(PaintColorState colorState)
    {
        switch (colorState)
        {
            case PaintColorState.Orange:
                return "Cam";

            case PaintColorState.Green:
                return "Lục";

            case PaintColorState.Purple:
                return "Tím";

            default:
                return colorState.ToString();
        }
    }
}