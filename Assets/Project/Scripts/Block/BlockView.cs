using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform piecesRoot;
    [SerializeField] private GameObject piecePrefab;

    [Header("Layout")]
    [SerializeField] private float pieceSize = 52f;
    [SerializeField] private float pieceSpacing = 3f;

    [Header("Invalid Drag Visual")]
    [SerializeField] private Color invalidDragColor = new Color32(235, 245, 255, 230);

    public BlockData Data { get; private set; }
    public PaintColorState SpawnedColorState { get; private set; }
    public Color SpawnedColor { get; private set; }

    private readonly List<PieceInfo> pieceInfos = new List<PieceInfo>();

    private bool isShowingInvalidVisual;

    private struct PieceInfo
    {
        public Vector2Int offset;
        public RectTransform rectTransform;
        public Image image;

        public PieceInfo(Vector2Int offset, RectTransform rectTransform, Image image)
        {
            this.offset = offset;
            this.rectTransform = rectTransform;
            this.image = image;
        }
    }

    private void Awake()
    {
        if (piecesRoot == null)
        {
            piecesRoot = transform as RectTransform;
        }
    }

    public void Build(BlockData data, PaintColorState colorState, Color fallbackColor)
    {
        Data = data;
        SpawnedColorState = colorState;
        SpawnedColor = fallbackColor;

        isShowingInvalidVisual = false;

        ClearPieces();

        if (Data == null || Data.cells == null || Data.cells.Length == 0)
        {
            Debug.LogWarning("BlockView: BlockData rỗng.");
            return;
        }

        if (piecePrefab == null)
        {
            Debug.LogError("BlockView: Chưa gán Piece Prefab.");
            return;
        }

        BuildPieces();
    }

    private void BuildPieces()
    {
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (Vector2Int cell in Data.cells)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        int widthCellCount = maxX - minX + 1;
        int heightCellCount = maxY - minY + 1;

        float step = pieceSize + pieceSpacing;

        float shapeWidth = widthCellCount * pieceSize + (widthCellCount - 1) * pieceSpacing;
        float shapeHeight = heightCellCount * pieceSize + (heightCellCount - 1) * pieceSpacing;

        for (int i = 0; i < Data.cells.Length; i++)
        {
            Vector2Int offset = Data.cells[i];

            GameObject pieceObject = Instantiate(piecePrefab, piecesRoot);
            pieceObject.name = $"Piece_{offset.x}_{offset.y}";

            RectTransform pieceRect = pieceObject.GetComponent<RectTransform>();
            if (pieceRect == null)
            {
                pieceRect = pieceObject.AddComponent<RectTransform>();
            }

            pieceRect.anchorMin = new Vector2(0.5f, 0.5f);
            pieceRect.anchorMax = new Vector2(0.5f, 0.5f);
            pieceRect.pivot = new Vector2(0.5f, 0.5f);
            pieceRect.sizeDelta = new Vector2(pieceSize, pieceSize);

            float x = (offset.x - minX) * step - shapeWidth * 0.5f + pieceSize * 0.5f;
            float y = -((offset.y - minY) * step) + shapeHeight * 0.5f - pieceSize * 0.5f;

            pieceRect.anchoredPosition = new Vector2(x, y);

            Image image = pieceObject.GetComponent<Image>();

            if (image == null)
            {
                image = pieceObject.AddComponent<Image>();
            }

            image.raycastTarget = false;

            pieceInfos.Add(new PieceInfo(offset, pieceRect, image));
        }

        RefreshPieceVisuals();
    }

    public void SetInvalidDragVisual(bool showInvalid)
    {
        if (isShowingInvalidVisual == showInvalid)
            return;

        isShowingInvalidVisual = showInvalid;

        if (showInvalid)
        {
            ApplyInvalidVisual();
        }
        else
        {
            RefreshPieceVisuals();
        }
    }

    private void ApplyInvalidVisual()
    {
        for (int i = 0; i < pieceInfos.Count; i++)
        {
            Image image = pieceInfos[i].image;

            if (image == null)
                continue;

            // Không dùng asset block màu khi vị trí không đặt được.
            // Đổi thành ô trắng/xanh nhạt đơn giản.
            image.sprite = null;
            image.color = invalidDragColor;
        }
    }

    private void RefreshPieceVisuals()
    {
        Sprite sprite = null;

        if (PaintColorSpriteLibrary.Instance != null)
        {
            sprite = PaintColorSpriteLibrary.Instance.GetBlockSprite(SpawnedColorState);
        }

        for (int i = 0; i < pieceInfos.Count; i++)
        {
            Image image = pieceInfos[i].image;

            if (image == null)
                continue;

            if (sprite != null)
            {
                image.sprite = sprite;
                image.color = Color.white;
            }
            else
            {
                image.sprite = null;
                image.color = SpawnedColor;
            }

            image.raycastTarget = false;
        }
    }

    public bool TryGetReferencePiece(out Vector2Int referenceOffset, out RectTransform referencePiece)
    {
        referenceOffset = Vector2Int.zero;
        referencePiece = null;

        if (pieceInfos.Count == 0)
            return false;

        PieceInfo info = pieceInfos[0];

        referenceOffset = info.offset;
        referencePiece = info.rectTransform;

        return referencePiece != null;
    }

    public void ClearPieces()
    {
        pieceInfos.Clear();

        if (piecesRoot == null)
            return;

        for (int i = piecesRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = piecesRoot.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}