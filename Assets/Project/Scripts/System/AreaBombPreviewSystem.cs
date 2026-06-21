using System.Collections.Generic;
using UnityEngine;

public class AreaBombPreviewSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TargetColorSystem targetColorSystem;

    [Header("Preview Settings")]
    [SerializeField] private bool showPreviewWhenBombReady = true;

    // Đổi bomb preview từ vàng sang xanh ngọc + trắng xanh nhạt để nhấp nháy
    [SerializeField] private Color previewColorA = new Color32(60, 245, 255, 255);
    [SerializeField] private Color previewColorB = new Color32(195, 255, 255, 255);

    [SerializeField] private float refreshInterval = 0.15f;

    private readonly List<Cell> previewCells = new List<Cell>();

    private float refreshTimer;
    private int lastCenterRow = -1;
    private int lastCenterCol = -1;
    private bool wasShowingPreview;
    private bool pulseToggle;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        if (targetColorSystem == null)
        {
            targetColorSystem = FindAnyObjectByType<TargetColorSystem>();
        }
    }

    private void Update()
    {
        if (!showPreviewWhenBombReady)
        {
            ClearPreview();
            return;
        }

        if (gridManager == null || targetColorSystem == null)
        {
            ClearPreview();
            return;
        }

        if (!targetColorSystem.IsColorBombReady)
        {
            ClearPreview();
            return;
        }

        refreshTimer += Time.deltaTime;

        if (refreshTimer < refreshInterval)
            return;

        refreshTimer = 0f;

        UpdateBombPreview();
    }

    private void UpdateBombPreview()
    {
        bool foundArea = TryFindBest3x3Area(out int centerRow, out int centerCol);

        if (!foundArea)
        {
            ClearPreview();
            return;
        }

        Color activePreviewColor = pulseToggle ? previewColorA : previewColorB;
        pulseToggle = !pulseToggle;

        ClearPreviewInternal();

        lastCenterRow = centerRow;
        lastCenterCol = centerCol;
        wasShowingPreview = true;

        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            for (int col = centerCol - 1; col <= centerCol + 1; col++)
            {
                if (!gridManager.IsInsideGrid(row, col))
                    continue;

                Cell cell = gridManager.GetCell(row, col);

                if (cell == null)
                    continue;

                cell.ShowPreviewColor(activePreviewColor);
                previewCells.Add(cell);
            }
        }
    }

    private bool TryFindBest3x3Area(out int bestCenterRow, out int bestCenterCol)
    {
        bestCenterRow = -1;
        bestCenterCol = -1;

        int bestFilledCount = 0;
        int bestSecondaryCount = 0;
        float bestCenterDistance = float.MaxValue;

        float boardCenterRow = (GridManager.Rows - 1) * 0.5f;
        float boardCenterCol = (GridManager.Columns - 1) * 0.5f;

        for (int row = 1; row < GridManager.Rows - 1; row++)
        {
            for (int col = 1; col < GridManager.Columns - 1; col++)
            {
                int filledCount = CountFilledIn3x3(row, col);
                int secondaryCount = CountSecondaryIn3x3(row, col);

                float centerDistance = Vector2.Distance(
                    new Vector2(row, col),
                    new Vector2(boardCenterRow, boardCenterCol)
                );

                if (filledCount > bestFilledCount)
                {
                    bestFilledCount = filledCount;
                    bestSecondaryCount = secondaryCount;
                    bestCenterDistance = centerDistance;
                    bestCenterRow = row;
                    bestCenterCol = col;
                }
                else if (filledCount == bestFilledCount && filledCount > 0)
                {
                    if (secondaryCount > bestSecondaryCount)
                    {
                        bestSecondaryCount = secondaryCount;
                        bestCenterDistance = centerDistance;
                        bestCenterRow = row;
                        bestCenterCol = col;
                    }
                    else if (secondaryCount == bestSecondaryCount && centerDistance < bestCenterDistance)
                    {
                        bestCenterDistance = centerDistance;
                        bestCenterRow = row;
                        bestCenterCol = col;
                    }
                }
            }
        }

        return bestFilledCount > 0;
    }

    private int CountFilledIn3x3(int centerRow, int centerCol)
    {
        int count = 0;

        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            for (int col = centerCol - 1; col <= centerCol + 1; col++)
            {
                if (!gridManager.IsInsideGrid(row, col))
                    continue;

                Cell cell = gridManager.GetCell(row, col);

                if (cell != null && !cell.IsEmpty)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private int CountSecondaryIn3x3(int centerRow, int centerCol)
    {
        int count = 0;

        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            for (int col = centerCol - 1; col <= centerCol + 1; col++)
            {
                if (!gridManager.IsInsideGrid(row, col))
                    continue;

                Cell cell = gridManager.GetCell(row, col);

                if (cell != null && !cell.IsEmpty && PaintColorRules.IsSecondary(cell.ColorState))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public void ClearPreview()
    {
        ClearPreviewInternal();

        lastCenterRow = -1;
        lastCenterCol = -1;
        wasShowingPreview = false;
    }

    private void ClearPreviewInternal()
    {
        if (previewCells.Count <= 0)
            return;

        for (int i = 0; i < previewCells.Count; i++)
        {
            if (previewCells[i] != null)
            {
                previewCells[i].HidePreview();
            }
        }

        previewCells.Clear();
    }
}