using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public const int Rows = 8;
    public const int Columns = 8;

    [Header("References")]
    [SerializeField] private Transform gridPanel;

    [Header("Systems")]
    [SerializeField] private ClearSystem clearSystem;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TargetColorSystem targetColorSystem;

    private readonly Cell[,] cells = new Cell[Rows, Columns];
    private readonly List<Cell> previewCells = new List<Cell>();

    private bool hasPlayedMixSfxThisPlacement;
    private bool hasPlayedAshSfxThisPlacement;

    private struct PreviewCellColor
    {
        public int row;
        public int col;
        public PaintColorState colorState;

        public PreviewCellColor(int row, int col, PaintColorState colorState)
        {
            this.row = row;
            this.col = col;
            this.colorState = colorState;
        }
    }

    private void Awake()
    {
        if (clearSystem == null)
        {
            clearSystem = FindAnyObjectByType<ClearSystem>();
        }

        if (scoreManager == null)
        {
            scoreManager = FindAnyObjectByType<ScoreManager>();
        }

        if (targetColorSystem == null)
        {
            targetColorSystem = FindAnyObjectByType<TargetColorSystem>();
        }

        BuildGridFromChildren();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            DebugFillTest();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearAll();
        }
    }
#endif

    private void BuildGridFromChildren()
    {
        if (gridPanel == null)
        {
            Debug.LogError("GridManager: Chưa gán GridPanel vào Inspector.");
            return;
        }

        if (gridPanel.childCount < Rows * Columns)
        {
            Debug.LogError($"GridManager: GridPanel cần {Rows * Columns} Cell, hiện tại chỉ có {gridPanel.childCount}.");
            return;
        }

        for (int i = 0; i < Rows * Columns; i++)
        {
            int row = i / Columns;
            int col = i % Columns;

            Transform child = gridPanel.GetChild(i);

            Cell cell = child.GetComponent<Cell>();
            if (cell == null)
            {
                cell = child.gameObject.AddComponent<Cell>();
            }

            CellEffect cellEffect = child.GetComponent<CellEffect>();
            if (cellEffect == null)
            {
                child.gameObject.AddComponent<CellEffect>();
            }

            cell.Init(row, col);
            child.name = $"Cell_{row}_{col}";

            cells[row, col] = cell;
        }

        Debug.Log("GridManager: Build grid 8x8 thành công.");
    }

    public bool IsInsideGrid(int row, int col)
    {
        return row >= 0 && row < Rows && col >= 0 && col < Columns;
    }

    public Cell GetCell(int row, int col)
    {
        if (!IsInsideGrid(row, col))
            return null;

        return cells[row, col];
    }

    public bool CanPlaceAt(int row, int col)
    {
        if (!IsInsideGrid(row, col))
            return false;

        return cells[row, col].CanPlace;
    }

    public bool CanPlaceBlock(BlockData data, int originRow, int originCol)
    {
        if (data == null || data.cells == null)
            return false;

        foreach (Vector2Int offset in data.cells)
        {
            int row = originRow + offset.y;
            int col = originCol + offset.x;

            if (!IsInsideGrid(row, col))
                return false;

            if (!cells[row, col].CanPlace)
                return false;
        }

        return true;
    }

    public void PlaceBlock(BlockData data, PaintColorState colorState, int originRow, int originCol)
    {
        if (!CanPlaceBlock(data, originRow, originCol))
            return;

        hasPlayedMixSfxThisPlacement = false;
        hasPlayedAshSfxThisPlacement = false;

        List<Cell> placedCells = new List<Cell>();

        foreach (Vector2Int offset in data.cells)
        {
            int row = originRow + offset.y;
            int col = originCol + offset.x;

            cells[row, col].SetFilled(colorState);
            placedCells.Add(cells[row, col]);
        }

        GameAudioSystem.Instance?.PlayPlaceBlock();

        ApplyColorMixing(placedCells);

        ClearResult clearResult = new ClearResult();

        if (clearSystem != null)
        {
            clearResult = clearSystem.CheckAndClear();
        }

        if (scoreManager != null)
        {
            scoreManager.AddClearScore(clearResult.clearedLineCount, clearResult.targetMatchedLineCount);
        }

        if (targetColorSystem != null)
        {
            if (clearResult.targetMatchedLineCount > 0)
            {
                targetColorSystem.AddTargetClearEnergy(clearResult.targetMatchedLineCount);
            }

            targetColorSystem.NotifyMoveEnded();
        }

        Debug.Log($"GridManager: Placed block {data.blockName} with {colorState} at row {originRow}, col {originCol}.");
    }

    private void ApplyColorMixing(List<Cell> placedCells)
    {
        if (placedCells == null || placedCells.Count == 0)
            return;

        foreach (Cell placedCell in placedCells)
        {
            if (placedCell == null || placedCell.IsEmpty)
                continue;

            if (!PaintColorRules.IsPrimary(placedCell.ColorState))
                continue;

            bool reacted = false;

            reacted = TryReactPlacedCellWithNeighbor(placedCell, placedCell.Row - 1, placedCell.Col);
            if (reacted) continue;

            reacted = TryReactPlacedCellWithNeighbor(placedCell, placedCell.Row + 1, placedCell.Col);
            if (reacted) continue;

            reacted = TryReactPlacedCellWithNeighbor(placedCell, placedCell.Row, placedCell.Col - 1);
            if (reacted) continue;

            TryReactPlacedCellWithNeighbor(placedCell, placedCell.Row, placedCell.Col + 1);
        }
    }

    private bool TryReactPlacedCellWithNeighbor(Cell placedCell, int neighborRow, int neighborCol)
    {
        if (!IsInsideGrid(neighborRow, neighborCol))
            return false;

        Cell neighbor = cells[neighborRow, neighborCol];

        if (neighbor == null || neighbor.IsEmpty)
            return false;

        PaintColorState placedColor = placedCell.ColorState;
        PaintColorState neighborColor = neighbor.ColorState;

        if (!PaintColorRules.IsPrimary(placedColor))
            return false;

        if (PaintColorRules.IsPrimary(neighborColor))
        {
            if (placedColor == neighborColor)
                return false;

            PaintColorState mixedColor = PaintColorRules.MixTwoPrimaryColors(placedColor, neighborColor);

            if (mixedColor == PaintColorState.None)
                return false;

            placedCell.SetColorState(mixedColor);
            neighbor.SetColorState(mixedColor);

            PlayMixEffect(placedCell);
            PlayMixEffect(neighbor);
            PlayMixSfxOnce();

            return true;
        }

        if (PaintColorRules.IsSecondary(neighborColor))
        {
            if (PaintColorRules.IsThirdPrimaryAgainstSecondary(placedColor, neighborColor))
            {
                placedCell.SetColorState(PaintColorState.Ash);

                PlayAshEffect(placedCell);
                PlayAshSfxOnce();

                return true;
            }

            return false;
        }

        return false;
    }

    private void PlayMixSfxOnce()
    {
        if (hasPlayedMixSfxThisPlacement)
            return;

        hasPlayedMixSfxThisPlacement = true;
        GameAudioSystem.Instance?.PlayMix();
    }

    private void PlayAshSfxOnce()
    {
        if (hasPlayedAshSfxThisPlacement)
            return;

        hasPlayedAshSfxThisPlacement = true;
        GameAudioSystem.Instance?.PlayAsh();
    }

    private void PlayMixEffect(Cell cell)
    {
        if (cell == null)
            return;

        CellEffect effect = cell.GetComponent<CellEffect>();

        if (effect != null)
        {
            effect.PlayMixEffect();
        }
    }

    private void PlayAshEffect(Cell cell)
    {
        if (cell == null)
            return;

        CellEffect effect = cell.GetComponent<CellEffect>();

        if (effect != null)
        {
            effect.PlayAshEffect();
        }
    }

    public void ShowPlacementPreview(BlockData data, PaintColorState colorState, int originRow, int originCol)
    {
        ClearPlacementPreview();

        if (data == null || data.cells == null)
            return;

        bool canPlace = CanPlaceBlock(data, originRow, originCol);

        if (!canPlace)
        {
            ShowInvalidPlacementPreview(data, originRow, originCol);
            return;
        }

        List<PreviewCellColor> previewColors = CalculatePlacementColorPreview(data, colorState, originRow, originCol);

        foreach (PreviewCellColor preview in previewColors)
        {
            if (!IsInsideGrid(preview.row, preview.col))
                continue;

            Cell cell = cells[preview.row, preview.col];

            if (cell == null)
                continue;

            cell.ShowPreviewColor(PaintColorRules.ToColor(preview.colorState));

            if (!previewCells.Contains(cell))
            {
                previewCells.Add(cell);
            }
        }
    }

    private void ShowInvalidPlacementPreview(BlockData data, int originRow, int originCol)
    {
        foreach (Vector2Int offset in data.cells)
        {
            int row = originRow + offset.y;
            int col = originCol + offset.x;

            if (!IsInsideGrid(row, col))
                continue;

            Cell cell = cells[row, col];

            if (cell == null)
                continue;

            cell.ShowPreview(false);

            if (!previewCells.Contains(cell))
            {
                previewCells.Add(cell);
            }
        }
    }

    private List<PreviewCellColor> CalculatePlacementColorPreview(
        BlockData data,
        PaintColorState placedColor,
        int originRow,
        int originCol
    )
    {
        Dictionary<Vector2Int, PaintColorState> previewStates = new Dictionary<Vector2Int, PaintColorState>();
        List<Vector2Int> placedPositions = new List<Vector2Int>();

        foreach (Vector2Int offset in data.cells)
        {
            int row = originRow + offset.y;
            int col = originCol + offset.x;

            Vector2Int position = new Vector2Int(row, col);

            previewStates[position] = placedColor;
            placedPositions.Add(position);
        }

        foreach (Vector2Int placedPosition in placedPositions)
        {
            if (!previewStates.TryGetValue(placedPosition, out PaintColorState currentColor))
                continue;

            if (!PaintColorRules.IsPrimary(currentColor))
                continue;

            bool reacted = false;

            reacted = TryPreviewReaction(placedPosition, new Vector2Int(-1, 0), previewStates);
            if (reacted) continue;

            reacted = TryPreviewReaction(placedPosition, new Vector2Int(1, 0), previewStates);
            if (reacted) continue;

            reacted = TryPreviewReaction(placedPosition, new Vector2Int(0, -1), previewStates);
            if (reacted) continue;

            TryPreviewReaction(placedPosition, new Vector2Int(0, 1), previewStates);
        }

        List<PreviewCellColor> result = new List<PreviewCellColor>();

        foreach (KeyValuePair<Vector2Int, PaintColorState> pair in previewStates)
        {
            Vector2Int position = pair.Key;

            if (!IsInsideGrid(position.x, position.y))
                continue;

            result.Add(new PreviewCellColor(position.x, position.y, pair.Value));
        }

        return result;
    }

    private bool TryPreviewReaction(
        Vector2Int placedPosition,
        Vector2Int direction,
        Dictionary<Vector2Int, PaintColorState> previewStates
    )
    {
        if (!previewStates.TryGetValue(placedPosition, out PaintColorState placedColor))
            return false;

        if (!PaintColorRules.IsPrimary(placedColor))
            return false;

        Vector2Int neighborPosition = placedPosition + direction;

        if (!IsInsideGrid(neighborPosition.x, neighborPosition.y))
            return false;

        if (!TryGetPreviewColor(neighborPosition, previewStates, out PaintColorState neighborColor))
            return false;

        if (PaintColorRules.IsPrimary(neighborColor))
        {
            if (placedColor == neighborColor)
                return false;

            PaintColorState mixedColor = PaintColorRules.MixTwoPrimaryColors(placedColor, neighborColor);

            if (mixedColor == PaintColorState.None)
                return false;

            previewStates[placedPosition] = mixedColor;
            previewStates[neighborPosition] = mixedColor;

            return true;
        }

        if (PaintColorRules.IsSecondary(neighborColor))
        {
            if (PaintColorRules.IsThirdPrimaryAgainstSecondary(placedColor, neighborColor))
            {
                previewStates[placedPosition] = PaintColorState.Ash;
                return true;
            }

            return false;
        }

        return false;
    }

    private bool TryGetPreviewColor(
        Vector2Int position,
        Dictionary<Vector2Int, PaintColorState> previewStates,
        out PaintColorState colorState
    )
    {
        if (previewStates.TryGetValue(position, out colorState))
        {
            return true;
        }

        if (!IsInsideGrid(position.x, position.y))
        {
            colorState = PaintColorState.None;
            return false;
        }

        Cell cell = cells[position.x, position.y];

        if (cell == null || cell.IsEmpty)
        {
            colorState = PaintColorState.None;
            return false;
        }

        colorState = cell.ColorState;
        return true;
    }

    public void ClearPlacementPreview()
    {
        for (int i = 0; i < previewCells.Count; i++)
        {
            if (previewCells[i] != null)
            {
                previewCells[i].HidePreview();
            }
        }

        previewCells.Clear();
    }

    public bool TryGetNearestCellFromScreenPosition(
        Vector2 screenPosition,
        Camera eventCamera,
        float maxDistance,
        out int row,
        out int col
    )
    {
        row = -1;
        col = -1;

        float bestDistanceSqr = maxDistance * maxDistance;

        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                RectTransform cellRect = cells[r, c].GetComponent<RectTransform>();

                Vector2 cellScreenPosition = RectTransformUtility.WorldToScreenPoint(
                    eventCamera,
                    cellRect.position
                );

                float distanceSqr = (screenPosition - cellScreenPosition).sqrMagnitude;

                if (distanceSqr <= bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    row = r;
                    col = c;
                }
            }
        }

        return row != -1 && col != -1;
    }

    public bool TryGetPlacementOriginFromBlockPiece(
        BlockData data,
        Vector2Int referenceOffset,
        Vector2 referenceScreenPosition,
        Camera eventCamera,
        float maxDistance,
        out int originRow,
        out int originCol
    )
    {
        originRow = -1;
        originCol = -1;

        bool isNearCell = TryGetNearestCellFromScreenPosition(
            referenceScreenPosition,
            eventCamera,
            maxDistance,
            out int hitRow,
            out int hitCol
        );

        if (!isNearCell)
        {
            return false;
        }

        originRow = hitRow - referenceOffset.y;
        originCol = hitCol - referenceOffset.x;

        return true;
    }

    public void SetCellFilled(int row, int col, PaintColorState colorState = PaintColorState.Red)
    {
        if (!IsInsideGrid(row, col))
            return;

        cells[row, col].SetFilled(colorState);
    }

    public void ClearCell(int row, int col)
    {
        if (!IsInsideGrid(row, col))
            return;

        cells[row, col].Clear();
    }

    public void ClearAll()
    {
        ClearPlacementPreview();

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                cells[row, col].Clear();
            }
        }

        Debug.Log("GridManager: Clear all cells.");
    }

    public int ClearCellsByColor(PaintColorState colorState)
    {
        if (colorState == PaintColorState.None)
            return 0;

        int clearedCount = 0;

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                Cell cell = cells[row, col];

                if (cell == null || cell.IsEmpty)
                    continue;

                if (cell.ColorState == colorState)
                {
                    ClearCellWithBombEffect(cell);
                    clearedCount++;
                }
            }
        }

        Debug.Log($"GridManager: Color Bomb cleared {clearedCount} cell(s) of {colorState}.");

        return clearedCount;
    }

    public int ClearBest3x3Area(out int centerRow, out int centerCol)
    {
        centerRow = -1;
        centerCol = -1;

        int bestFilledCount = 0;
        int bestSecondaryCount = 0;

        for (int row = 1; row < Rows - 1; row++)
        {
            for (int col = 1; col < Columns - 1; col++)
            {
                int filledCount = CountFilledIn3x3(row, col);
                int secondaryCount = CountSecondaryIn3x3(row, col);

                if (filledCount > bestFilledCount)
                {
                    bestFilledCount = filledCount;
                    bestSecondaryCount = secondaryCount;
                    centerRow = row;
                    centerCol = col;
                }
                else if (filledCount == bestFilledCount && filledCount > 0)
                {
                    if (secondaryCount > bestSecondaryCount)
                    {
                        bestSecondaryCount = secondaryCount;
                        centerRow = row;
                        centerCol = col;
                    }
                }
            }
        }

        if (bestFilledCount <= 0)
        {
            Debug.Log("GridManager: Area Bomb không tìm thấy ô nào để nổ.");
            return 0;
        }

        int clearedCount = Clear3x3Area(centerRow, centerCol);

        Debug.Log($"GridManager: Area Bomb exploded 3x3 at center ({centerRow}, {centerCol}), cleared {clearedCount} cell(s).");

        return clearedCount;
    }

    private int CountFilledIn3x3(int centerRow, int centerCol)
    {
        int count = 0;

        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            for (int col = centerCol - 1; col <= centerCol + 1; col++)
            {
                if (!IsInsideGrid(row, col))
                    continue;

                Cell cell = cells[row, col];

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
                if (!IsInsideGrid(row, col))
                    continue;

                Cell cell = cells[row, col];

                if (cell != null && !cell.IsEmpty && PaintColorRules.IsSecondary(cell.ColorState))
                {
                    count++;
                }
            }
        }

        return count;
    }

    private int Clear3x3Area(int centerRow, int centerCol)
    {
        int clearedCount = 0;

        for (int row = centerRow - 1; row <= centerRow + 1; row++)
        {
            for (int col = centerCol - 1; col <= centerCol + 1; col++)
            {
                if (!IsInsideGrid(row, col))
                    continue;

                Cell cell = cells[row, col];

                if (cell == null || cell.IsEmpty)
                    continue;

                ClearCellWithBombEffect(cell);
                clearedCount++;
            }
        }

        return clearedCount;
    }

    private void ClearCellWithBombEffect(Cell cell)
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

    public int CountFilledCells()
    {
        int count = 0;

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (!cells[row, col].IsEmpty)
                {
                    count++;
                }
            }
        }

        return count;
    }

    public bool IsBoardEmpty()
    {
        return CountFilledCells() == 0;
    }

    public float GetFillRatio()
    {
        return CountFilledCells() / (float)(Rows * Columns);
    }

    public bool HasValidPlacement(BlockData data)
    {
        return GetValidPlacementCount(data) > 0;
    }

    public int GetValidPlacementCount(BlockData data)
    {
        if (data == null || data.cells == null)
            return 0;

        int count = 0;

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (CanPlaceBlock(data, row, col))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public int GetBestPotentialClearCount(BlockData data)
    {
        if (data == null || data.cells == null)
            return 0;

        int bestClearCount = 0;

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                if (!CanPlaceBlock(data, row, col))
                    continue;

                int clearCount = CountLinesClearedIfPlaced(data, row, col);

                if (clearCount > bestClearCount)
                {
                    bestClearCount = clearCount;
                }
            }
        }

        return bestClearCount;
    }

    private int CountLinesClearedIfPlaced(BlockData data, int originRow, int originCol)
    {
        int clearCount = 0;

        for (int row = 0; row < Rows; row++)
        {
            bool rowFull = true;

            for (int col = 0; col < Columns; col++)
            {
                bool alreadyFilled = !cells[row, col].IsEmpty;
                bool willBeFilled = IsCoveredByBlock(data, originRow, originCol, row, col);

                if (!alreadyFilled && !willBeFilled)
                {
                    rowFull = false;
                    break;
                }
            }

            if (rowFull)
            {
                clearCount++;
            }
        }

        for (int col = 0; col < Columns; col++)
        {
            bool columnFull = true;

            for (int row = 0; row < Rows; row++)
            {
                bool alreadyFilled = !cells[row, col].IsEmpty;
                bool willBeFilled = IsCoveredByBlock(data, originRow, originCol, row, col);

                if (!alreadyFilled && !willBeFilled)
                {
                    columnFull = false;
                    break;
                }
            }

            if (columnFull)
            {
                clearCount++;
            }
        }

        return clearCount;
    }

    private bool IsCoveredByBlock(BlockData data, int originRow, int originCol, int targetRow, int targetCol)
    {
        foreach (Vector2Int offset in data.cells)
        {
            int row = originRow + offset.y;
            int col = originCol + offset.x;

            if (row == targetRow && col == targetCol)
            {
                return true;
            }
        }

        return false;
    }

    public void DebugFillTest()
    {
        ClearAll();

        for (int col = 0; col < Columns; col++)
        {
            SetCellFilled(7, col, PaintColorState.Red);
        }

        SetCellFilled(6, 0, PaintColorState.Red);
        SetCellFilled(6, 1, PaintColorState.Yellow);
        SetCellFilled(6, 2, PaintColorState.Blue);
        SetCellFilled(6, 3, PaintColorState.Orange);
        SetCellFilled(6, 4, PaintColorState.Green);
        SetCellFilled(6, 5, PaintColorState.Purple);
        SetCellFilled(6, 6, PaintColorState.Ash);

        SetCellFilled(3, 3, PaintColorState.Orange);
        SetCellFilled(3, 4, PaintColorState.Green);
        SetCellFilled(3, 5, PaintColorState.Purple);
        SetCellFilled(4, 3, PaintColorState.Red);
        SetCellFilled(4, 4, PaintColorState.Yellow);
        SetCellFilled(4, 5, PaintColorState.Blue);
        SetCellFilled(5, 3, PaintColorState.Ash);
        SetCellFilled(5, 4, PaintColorState.Orange);
        SetCellFilled(5, 5, PaintColorState.Green);

        Debug.Log("GridManager: Debug fill paint colors + 3x3 bomb test area. Nhấn R để reset.");
    }
}