using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameOverChecker gameOverChecker;

    [Header("Block Pool")]
    [SerializeField] private List<BlockData> blockPool = new List<BlockData>();

    [Header("Tray")]
    [SerializeField] private List<Transform> traySlots = new List<Transform>();

    [Header("Prefab")]
    [SerializeField] private BlockView blockViewPrefab;

    [Header("Spawn On Start")]
    [SerializeField] private bool spawnOnStart = true;

    [Header("Smart Spawn Rules")]
    [SerializeField] private bool useSmartSpawn = true;
    [SerializeField] private bool ensureOneEasyBlock = true;
    [SerializeField] private bool ensureOneClearBlockWhenPossible = true;
    [SerializeField] private bool limitBigBlockPerSet = true;
    [SerializeField] private int maxBigBlocksPerSet = 2;

    [Header("Open Board Bias")]
    [SerializeField] private float openBoardFillRatioThreshold = 0.35f;
    [SerializeField] private float openBoardBigBlockChance = 0.9f;
    [SerializeField] private float openBoardRectangleChance = 0.85f;
    [SerializeField] private int bigBlockCellCountThreshold = 5;

    [Header("Feel Good Assist")]
    [SerializeField] private float clearAssistChance = 0.85f;
    [SerializeField] private float comboAssistChance = 0.95f;
    [SerializeField] private int topCandidatePoolSize = 10;
    [SerializeField] private int easyBlockMinValidPlacements = 5;

    [Header("Paint Color Spawn")]
    [SerializeField] private int redWeight = 34;
    [SerializeField] private int yellowWeight = 33;
    [SerializeField] private int blueWeight = 33;

    private readonly List<BlockView> currentBlocks = new List<BlockView>();

    private void Awake()
    {
        if (rootCanvas == null)
        {
            rootCanvas = FindAnyObjectByType<Canvas>();
        }

        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        if (gameOverChecker == null)
        {
            gameOverChecker = FindAnyObjectByType<GameOverChecker>();
        }
    }

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnNewSet();
        }
    }

    public void SpawnNewSet()
    {
        ClearTray();

        if (!ValidateReferences())
            return;

        List<BlockData> selectedBlocks = useSmartSpawn
            ? SelectSmartBlockSet()
            : SelectBasicRandomBlockSet();

        if (selectedBlocks.Count == 0)
        {
            Debug.LogWarning("BlockSpawner: Không còn block nào đặt được.");

            if (gameOverChecker != null)
            {
                gameOverChecker.TriggerGameOver();
            }

            return;
        }

        for (int i = 0; i < traySlots.Count; i++)
        {
            BlockData data = selectedBlocks[Mathf.Min(i, selectedBlocks.Count - 1)];

            BlockView blockView = Instantiate(blockViewPrefab, traySlots[i]);
            blockView.name = $"Block_{data.blockName}";

            RectTransform rt = blockView.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;

            PaintColorState colorState = PickPrimaryColorForBlock();
            Color blockColor = GetColorForPaintState(colorState);

            blockView.Build(data, colorState, blockColor);

            DraggableBlock draggable = blockView.GetComponent<DraggableBlock>();
            if (draggable == null)
            {
                draggable = blockView.gameObject.AddComponent<DraggableBlock>();
            }

            draggable.Init(this, gridManager, rootCanvas);

            currentBlocks.Add(blockView);
        }

        Debug.Log("BlockSpawner: Spawned smart set: " +
                  string.Join(", ", currentBlocks.Select(b =>
                      $"{b.Data.blockName}({b.Data.role}) cells={GetCellCount(b.Data)}, color={b.SpawnedColorState}, bestClear={gridManager.GetBestPotentialClearCount(b.Data)}, placements={gridManager.GetValidPlacementCount(b.Data)}")));
    }

    public void NotifyBlockPlaced(BlockView placedBlock)
    {
        if (placedBlock == null)
            return;

        currentBlocks.Remove(placedBlock);
        Destroy(placedBlock.gameObject);

        if (currentBlocks.Count <= 0)
        {
            SpawnNewSet();
            return;
        }

        if (!HasAnyCurrentBlockValidPlacement())
        {
            Debug.LogWarning("BlockSpawner: Các block còn lại không đặt được nữa.");

            if (gameOverChecker != null)
            {
                gameOverChecker.TriggerGameOver();
            }
        }
    }

    private List<BlockData> SelectSmartBlockSet()
    {
        List<BlockData> validBlocks = GetValidBlocks();

        if (validBlocks.Count == 0)
            return new List<BlockData>();

        int slotCount = Mathf.Max(1, traySlots.Count);
        List<BlockData> selected = new List<BlockData>();

        float fillRatio = gridManager.GetFillRatio();
        int bigBlockCount = 0;

        bool boardIsOpen = fillRatio <= openBoardFillRatioThreshold;

        if (boardIsOpen && selected.Count < slotCount)
        {
            BlockData openBoardBlock = PickOpenBoardBlock(validBlocks, selected);

            if (openBoardBlock != null)
            {
                selected.Add(openBoardBlock);

                if (IsBigBlock(openBoardBlock))
                {
                    bigBlockCount++;
                }

                Debug.Log($"BlockSpawner: Open-board bias selected {openBoardBlock.blockName}({openBoardBlock.role}), cells={GetCellCount(openBoardBlock)}");
            }
        }

        BlockData bestClearBlock = GetBestClearBlock(validBlocks, out int bestClearCount);

        if (ensureOneClearBlockWhenPossible && bestClearBlock != null && bestClearCount > 0 && selected.Count < slotCount)
        {
            float assistChance = bestClearCount >= 2 ? comboAssistChance : clearAssistChance;

            if (Random.value <= assistChance && CanPickBlock(bestClearBlock, selected, false))
            {
                selected.Add(bestClearBlock);

                if (IsBigBlock(bestClearBlock))
                {
                    bigBlockCount++;
                }

                Debug.Log($"BlockSpawner: Feel-good assist selected {bestClearBlock.blockName}, bestClear={bestClearCount}");
            }
        }

        if (ensureOneEasyBlock && selected.Count < slotCount && !boardIsOpen)
        {
            BlockData easyBlock = PickEasyBlock(validBlocks, selected, fillRatio);

            if (easyBlock != null)
            {
                selected.Add(easyBlock);

                if (IsBigBlock(easyBlock))
                {
                    bigBlockCount++;
                }
            }
        }

        while (selected.Count < slotCount)
        {
            BlockData nextBlock = PickWeightedSmartBlock(validBlocks, selected, bigBlockCount, fillRatio);

            if (nextBlock == null)
                break;

            selected.Add(nextBlock);

            if (IsBigBlock(nextBlock))
            {
                bigBlockCount++;
            }
        }

        while (selected.Count < slotCount && validBlocks.Count > 0)
        {
            BlockData fallback = validBlocks[Random.Range(0, validBlocks.Count)];
            selected.Add(fallback);
        }

        return selected;
    }

    private BlockData PickOpenBoardBlock(List<BlockData> validBlocks, List<BlockData> selected)
    {
        List<BlockData> rectangleBlocks = validBlocks
            .Where(block => CanPickBlock(block, selected, false))
            .Where(block => block.role == BlockRole.Rectangle)
            .OrderByDescending(CalculateOpenBoardBlockScore)
            .Take(6)
            .ToList();

        if (rectangleBlocks.Count > 0 && Random.value <= openBoardRectangleChance)
        {
            return PickFromTopRandom(rectangleBlocks);
        }

        List<BlockData> bigBlocks = validBlocks
            .Where(block => CanPickBlock(block, selected, false))
            .Where(block => IsBigBlock(block))
            .OrderByDescending(CalculateOpenBoardBlockScore)
            .Take(8)
            .ToList();

        if (bigBlocks.Count > 0 && Random.value <= openBoardBigBlockChance)
        {
            return PickFromTopRandom(bigBlocks);
        }

        List<BlockData> largeNormalBlocks = validBlocks
            .Where(block => CanPickBlock(block, selected, false))
            .Where(block => GetCellCount(block) >= 4)
            .OrderByDescending(CalculateOpenBoardBlockScore)
            .Take(8)
            .ToList();

        if (largeNormalBlocks.Count > 0)
        {
            return PickFromTopRandom(largeNormalBlocks);
        }

        return null;
    }

    private float CalculateOpenBoardBlockScore(BlockData data)
    {
        if (data == null)
            return 0f;

        int placements = gridManager.GetValidPlacementCount(data);
        int cellCount = GetCellCount(data);
        int bestClear = gridManager.GetBestPotentialClearCount(data);

        float score = 0f;

        score += placements * 4f;
        score += cellCount * 35f;
        score += bestClear * 180f;

        if (data.role == BlockRole.Rectangle)
        {
            score += 260f;
        }

        if (data.role == BlockRole.Big)
        {
            score += 180f;
        }

        if (data.role == BlockRole.Square)
        {
            score += 90f;
        }

        if (data.role == BlockRole.Line)
        {
            score += 70f;
        }

        if (cellCount >= 5)
        {
            score += 160f;
        }

        if (cellCount >= 6)
        {
            score += 120f;
        }

        return score;
    }

    private BlockData PickFromTopRandom(List<BlockData> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return null;

        int maxIndex = Mathf.Min(3, candidates.Count);
        int index = Random.Range(0, maxIndex);

        return candidates[index];
    }

    private List<BlockData> SelectBasicRandomBlockSet()
    {
        List<BlockData> validBlocks = GetValidBlocks();
        List<BlockData> result = new List<BlockData>();

        if (validBlocks.Count == 0)
            return result;

        int slotCount = Mathf.Max(1, traySlots.Count);

        for (int i = 0; i < slotCount; i++)
        {
            BlockData randomBlock = validBlocks[Random.Range(0, validBlocks.Count)];
            result.Add(randomBlock);
        }

        return result;
    }

    private List<BlockData> GetValidBlocks()
    {
        List<BlockData> validBlocks = new List<BlockData>();

        foreach (BlockData data in blockPool)
        {
            if (data == null)
                continue;

            if (data.cells == null || data.cells.Length == 0)
                continue;

            if (gridManager.HasValidPlacement(data))
            {
                validBlocks.Add(data);
            }
        }

        return validBlocks;
    }

    private BlockData GetBestClearBlock(List<BlockData> validBlocks, out int bestClearCount)
    {
        bestClearCount = 0;
        BlockData bestBlock = null;

        foreach (BlockData data in validBlocks)
        {
            int clearCount = gridManager.GetBestPotentialClearCount(data);

            if (clearCount > bestClearCount)
            {
                bestClearCount = clearCount;
                bestBlock = data;
            }
            else if (clearCount == bestClearCount && clearCount > 0)
            {
                int currentPlacements = bestBlock == null ? 0 : gridManager.GetValidPlacementCount(bestBlock);
                int newPlacements = gridManager.GetValidPlacementCount(data);

                if (newPlacements > currentPlacements)
                {
                    bestBlock = data;
                }
            }
        }

        return bestBlock;
    }

    private BlockData PickEasyBlock(List<BlockData> validBlocks, List<BlockData> selected, float fillRatio)
    {
        List<BlockData> candidates = validBlocks
            .Where(block => CanPickBlock(block, selected, false))
            .Where(block => gridManager.GetValidPlacementCount(block) >= easyBlockMinValidPlacements || GetCellCount(block) <= 3)
            .OrderByDescending(block => CalculateEasyBlockScore(block, fillRatio))
            .Take(5)
            .ToList();

        if (candidates.Count == 0)
        {
            candidates = validBlocks
                .Where(block => CanPickBlock(block, selected, false))
                .OrderByDescending(block => CalculateEasyBlockScore(block, fillRatio))
                .Take(5)
                .ToList();
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private float CalculateEasyBlockScore(BlockData data, float fillRatio)
    {
        int placements = gridManager.GetValidPlacementCount(data);
        int cellCount = GetCellCount(data);
        int bestClear = gridManager.GetBestPotentialClearCount(data);

        float score = 0f;

        score += placements * 8f;
        score += bestClear * 60f;

        if (cellCount <= 1)
            score += 60f;
        else if (cellCount == 2)
            score += 50f;
        else if (cellCount == 3)
            score += 35f;
        else if (cellCount == 4)
            score += 15f;
        else
            score -= 20f;

        if (fillRatio >= 0.65f)
        {
            score += Mathf.Max(0, 6 - cellCount) * 20f;
        }

        if (data.role == BlockRole.Small)
            score += 30f;

        if (data.role == BlockRole.Line)
            score += 15f;

        return score;
    }

    private BlockData PickWeightedSmartBlock(
        List<BlockData> validBlocks,
        List<BlockData> selected,
        int currentBigBlockCount,
        float fillRatio
    )
    {
        List<BlockData> candidates = validBlocks
            .Where(block => CanPickBlock(block, selected, false))
            .ToList();

        if (candidates.Count == 0)
        {
            candidates = new List<BlockData>(validBlocks);
        }

        List<WeightedBlockCandidate> weightedCandidates = new List<WeightedBlockCandidate>();

        foreach (BlockData data in candidates)
        {
            float weight = CalculateSmartWeight(data, currentBigBlockCount, fillRatio);

            if (weight <= 0f)
                continue;

            weightedCandidates.Add(new WeightedBlockCandidate(data, weight));
        }

        if (weightedCandidates.Count == 0)
            return null;

        weightedCandidates = weightedCandidates
            .OrderByDescending(candidate => candidate.weight)
            .Take(Mathf.Max(1, topCandidatePoolSize))
            .ToList();

        return PickByWeight(weightedCandidates);
    }

    private float CalculateSmartWeight(BlockData data, int currentBigBlockCount, float fillRatio)
    {
        int placements = gridManager.GetValidPlacementCount(data);
        int bestClear = gridManager.GetBestPotentialClearCount(data);
        int cellCount = GetCellCount(data);

        if (placements <= 0)
            return 0f;

        float weight = 10f;

        weight += placements * 5f;

        if (bestClear > 0)
        {
            weight += 120f * bestClear;
            weight += 80f * bestClear * bestClear;
        }

        if (bestClear >= 2)
        {
            weight += 250f;
        }

        if (bestClear >= 3)
        {
            weight += 450f;
        }

        weight += GetRoleWeight(data.role, fillRatio, cellCount);

        if (fillRatio < 0.25f)
        {
            weight += cellCount * 35f;

            if (cellCount >= 5)
            {
                weight += 220f;
            }

            if (data.role == BlockRole.Rectangle)
            {
                weight += 320f;
            }

            if (data.role == BlockRole.Big)
            {
                weight += 220f;
            }
        }
        else if (fillRatio < 0.45f)
        {
            weight += cellCount * 22f;

            if (cellCount >= 5)
            {
                weight += 120f;
            }

            if (data.role == BlockRole.Rectangle)
            {
                weight += 220f;
            }
        }
        else if (fillRatio < 0.65f)
        {
            weight += Mathf.Clamp(cellCount, 2, 5) * 8f;

            if (data.role == BlockRole.Rectangle)
            {
                weight += 60f;
            }
        }
        else if (fillRatio < 0.78f)
        {
            weight += Mathf.Max(0, 6 - cellCount) * 12f;

            if (cellCount >= 6)
            {
                weight *= 0.55f;
            }
        }
        else
        {
            weight += Mathf.Max(0, 7 - cellCount) * 25f;

            if (cellCount >= 5)
            {
                weight *= 0.25f;
            }
        }

        if (limitBigBlockPerSet && currentBigBlockCount >= maxBigBlocksPerSet && IsBigBlock(data))
        {
            weight *= 0.15f;
        }

        if (gridManager.IsBoardEmpty())
        {
            if (data.role == BlockRole.Rectangle)
            {
                weight += 400f;
            }

            if (cellCount >= 5)
            {
                weight += 300f;
            }

            if (data.role == BlockRole.Big)
            {
                weight += 250f;
            }
        }

        weight *= Random.Range(0.85f, 1.15f);

        return Mathf.Max(1f, weight);
    }

    private float GetRoleWeight(BlockRole role, float fillRatio, int cellCount)
    {
        switch (role)
        {
            case BlockRole.Small:
                if (fillRatio >= 0.70f)
                    return 90f;

                if (fillRatio >= 0.50f)
                    return 40f;

                if (fillRatio <= 0.30f)
                    return -40f;

                return 10f;

            case BlockRole.Line:
                if (fillRatio <= 0.35f)
                    return 80f;

                if (fillRatio >= 0.35f)
                    return 55f;

                return 30f;

            case BlockRole.Square:
                if (fillRatio <= 0.45f)
                    return 90f;

                if (fillRatio <= 0.60f)
                    return 35f;

                return 10f;

            case BlockRole.Rectangle:
                if (fillRatio <= 0.25f)
                    return 420f;

                if (fillRatio <= 0.40f)
                    return 320f;

                if (fillRatio <= 0.60f)
                    return 120f;

                if (fillRatio >= 0.75f)
                    return -80f;

                return 40f;

            case BlockRole.LShape:
                if (fillRatio >= 0.25f && fillRatio <= 0.65f)
                    return 45f;

                return 20f;

            case BlockRole.TShape:
                if (fillRatio <= 0.45f)
                    return 80f;

                if (fillRatio >= 0.30f && fillRatio <= 0.65f)
                    return 45f;

                if (fillRatio >= 0.75f)
                    return -45f;

                return 15f;

            case BlockRole.Zigzag:
                if (fillRatio <= 0.45f)
                    return 80f;

                if (fillRatio >= 0.30f && fillRatio <= 0.65f)
                    return 45f;

                if (fillRatio >= 0.75f)
                    return -45f;

                return 10f;

            case BlockRole.Big:
                if (fillRatio <= 0.25f)
                    return 380f;

                if (fillRatio <= 0.40f)
                    return 260f;

                if (fillRatio <= 0.55f)
                    return 80f;

                if (fillRatio >= 0.65f)
                    return -100f;

                return 10f;

            default:
                if (cellCount >= 5 && fillRatio <= 0.35f)
                    return 220f;

                if (cellCount <= 3 && fillRatio >= 0.60f)
                    return 35f;

                return 10f;
        }
    }

    private bool CanPickBlock(BlockData block, List<BlockData> selected, bool allowDuplicateIfNeeded)
    {
        if (block == null)
            return false;

        if (allowDuplicateIfNeeded)
            return true;

        if (selected.Count == 0)
            return true;

        if (blockPool.Count <= traySlots.Count)
            return true;

        return !selected.Contains(block);
    }

    private BlockData PickByWeight(List<WeightedBlockCandidate> candidates)
    {
        float totalWeight = 0f;

        foreach (WeightedBlockCandidate candidate in candidates)
        {
            totalWeight += candidate.weight;
        }

        if (totalWeight <= 0f)
        {
            return candidates[Random.Range(0, candidates.Count)].block;
        }

        float roll = Random.Range(0f, totalWeight);

        foreach (WeightedBlockCandidate candidate in candidates)
        {
            if (roll <= candidate.weight)
            {
                return candidate.block;
            }

            roll -= candidate.weight;
        }

        return candidates[candidates.Count - 1].block;
    }

    private bool HasAnyCurrentBlockValidPlacement()
    {
        foreach (BlockView blockView in currentBlocks)
        {
            if (blockView == null || blockView.Data == null)
                continue;

            if (gridManager.HasValidPlacement(blockView.Data))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBigBlock(BlockData data)
    {
        if (data == null)
            return false;

        if (data.role == BlockRole.Big)
            return true;

        return GetCellCount(data) >= bigBlockCellCountThreshold;
    }

    private int GetCellCount(BlockData data)
    {
        if (data == null || data.cells == null)
            return 0;

        return data.cells.Length;
    }

    private PaintColorState PickPrimaryColorForBlock()
    {
        int totalWeight = redWeight + yellowWeight + blueWeight;

        if (totalWeight <= 0)
            return PaintColorState.Red;

        int roll = Random.Range(0, totalWeight);

        if (roll < redWeight)
            return PaintColorState.Red;

        roll -= redWeight;

        if (roll < yellowWeight)
            return PaintColorState.Yellow;

        return PaintColorState.Blue;
    }

    private Color GetColorForPaintState(PaintColorState colorState)
    {
        return PaintColorRules.ToColor(colorState);
    }

    private bool ValidateReferences()
    {
        if (rootCanvas == null)
        {
            Debug.LogError("BlockSpawner: Chưa có Root Canvas.");
            return false;
        }

        if (gridManager == null)
        {
            Debug.LogError("BlockSpawner: Chưa có GridManager.");
            return false;
        }

        if (blockViewPrefab == null)
        {
            Debug.LogError("BlockSpawner: Chưa gán BlockView Prefab.");
            return false;
        }

        if (traySlots == null || traySlots.Count == 0)
        {
            Debug.LogError("BlockSpawner: Chưa gán Tray Slots.");
            return false;
        }

        if (blockPool == null || blockPool.Count == 0)
        {
            Debug.LogError("BlockSpawner: Block Pool đang trống.");
            return false;
        }

        return true;
    }

    private void ClearTray()
    {
        foreach (BlockView blockView in currentBlocks)
        {
            if (blockView != null)
            {
                Destroy(blockView.gameObject);
            }
        }

        currentBlocks.Clear();

        if (traySlots == null)
            return;

        foreach (Transform slot in traySlots)
        {
            if (slot == null)
                continue;

            for (int i = slot.childCount - 1; i >= 0; i--)
            {
                Transform child = slot.GetChild(i);

                if (child.GetComponent<BlockView>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private struct WeightedBlockCandidate
    {
        public BlockData block;
        public float weight;

        public WeightedBlockCandidate(BlockData block, float weight)
        {
            this.block = block;
            this.weight = weight;
        }
    }
}