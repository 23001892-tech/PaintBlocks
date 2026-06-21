using System.Collections.Generic;
using UnityEngine;

public struct ClearResult
{
    public int clearedLineCount;
    public int targetMatchedLineCount;
}

public class ClearSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private TargetColorSystem targetColorSystem;

    [Header("Target Clear Rule")]
    [SerializeField] private int targetColorRequirement = 4;

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

    public ClearResult CheckAndClear()
    {
        ClearResult result = new ClearResult();

        if (gridManager == null)
        {
            Debug.LogError("ClearSystem: Chưa có GridManager.");
            return result;
        }

        List<int> fullRows = new List<int>();
        List<int> fullColumns = new List<int>();

        for (int row = 0; row < GridManager.Rows; row++)
        {
            if (IsRowFull(row))
            {
                fullRows.Add(row);
            }
        }

        for (int col = 0; col < GridManager.Columns; col++)
        {
            if (IsColumnFull(col))
            {
                fullColumns.Add(col);
            }
        }

        result.clearedLineCount = fullRows.Count + fullColumns.Count;

        if (result.clearedLineCount == 0)
        {
            return result;
        }

        GameAudioSystem.Instance?.PlayClearLine();

        PaintColorState targetColor = PaintColorState.None;

        if (targetColorSystem != null)
        {
            targetColor = targetColorSystem.CurrentTargetColor;
        }

        foreach (int row in fullRows)
        {
            if (IsRowMajorityTargetColor(row, targetColor))
            {
                result.targetMatchedLineCount++;
            }
        }

        foreach (int col in fullColumns)
        {
            if (IsColumnMajorityTargetColor(col, targetColor))
            {
                result.targetMatchedLineCount++;
            }
        }

        HashSet<Cell> cellsToClear = new HashSet<Cell>();

        foreach (int row in fullRows)
        {
            for (int col = 0; col < GridManager.Columns; col++)
            {
                Cell cell = gridManager.GetCell(row, col);

                if (cell != null)
                {
                    cellsToClear.Add(cell);
                }
            }
        }

        foreach (int col in fullColumns)
        {
            for (int row = 0; row < GridManager.Rows; row++)
            {
                Cell cell = gridManager.GetCell(row, col);

                if (cell != null)
                {
                    cellsToClear.Add(cell);
                }
            }
        }

        foreach (Cell cell in cellsToClear)
        {
            ClearCellWithEffect(cell);
        }

        Debug.Log($"ClearSystem: Cleared {result.clearedLineCount} line(s), target matched {result.targetMatchedLineCount}, cells {cellsToClear.Count}.");

        return result;
    }

    private void ClearCellWithEffect(Cell cell)
    {
        if (cell == null || cell.IsEmpty)
            return;

        Color fromColor = cell.GetCurrentVisualColor();

        CellEffect effect = cell.GetComponent<CellEffect>();

        cell.Clear();

        Color toColor = cell.GetCurrentVisualColor();

        if (effect != null)
        {
            effect.PlayClearEffect(fromColor, toColor);
        }
    }

    private bool IsRowFull(int row)
    {
        for (int col = 0; col < GridManager.Columns; col++)
        {
            Cell cell = gridManager.GetCell(row, col);

            if (cell == null || cell.IsEmpty)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsColumnFull(int col)
    {
        for (int row = 0; row < GridManager.Rows; row++)
        {
            Cell cell = gridManager.GetCell(row, col);

            if (cell == null || cell.IsEmpty)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsRowMajorityTargetColor(int row, PaintColorState targetColor)
    {
        if (targetColor == PaintColorState.None)
            return false;

        int targetCount = 0;

        for (int col = 0; col < GridManager.Columns; col++)
        {
            Cell cell = gridManager.GetCell(row, col);

            if (cell != null && !cell.IsEmpty && cell.ColorState == targetColor)
            {
                targetCount++;
            }
        }

        return targetCount >= targetColorRequirement;
    }

    private bool IsColumnMajorityTargetColor(int col, PaintColorState targetColor)
    {
        if (targetColor == PaintColorState.None)
            return false;

        int targetCount = 0;

        for (int row = 0; row < GridManager.Rows; row++)
        {
            Cell cell = gridManager.GetCell(row, col);

            if (cell != null && !cell.IsEmpty && cell.ColorState == targetColor)
            {
                targetCount++;
            }
        }

        return targetCount >= targetColorRequirement;
    }
}