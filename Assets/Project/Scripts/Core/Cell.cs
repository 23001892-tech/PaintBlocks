using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    [Header("Cell Info")]
    public int Row { get; private set; }
    public int Col { get; private set; }

    public CellState State { get; private set; } = CellState.Empty;
    public PaintColorState ColorState { get; private set; } = PaintColorState.None;

    [Header("Visual")]
    [SerializeField] private Image cellImage;

    [Header("Preview Sprite")]
    [SerializeField] private Sprite previewBaseSprite;

    public bool IsEmpty => State == CellState.Empty;
    public bool CanPlace => State == CellState.Empty;

    private bool isPreviewing;

    private static Sprite runtimeEmptySprite;

    // Màu ô rỗng cố định: xám đậm, không xanh.
    private static readonly Color32 EmptyCellColor = new Color32(42, 43, 52, 255);

    private static readonly Color32 ValidPreviewColor = new Color32(180, 210, 255, 180);
    private static readonly Color32 InvalidPreviewColor = new Color32(235, 245, 255, 215);

    private void Awake()
    {
        if (cellImage == null)
        {
            cellImage = GetComponent<Image>();
        }

        EnsureRuntimeEmptySprite();
    }

    private void OnEnable()
    {
        if (State == CellState.Empty)
        {
            RefreshVisual();
        }
    }

    public void Init(int row, int col)
    {
        Row = row;
        Col = col;
        Clear();
    }

    public void SetFilled(PaintColorState colorState)
    {
        State = CellState.Filled;
        ColorState = colorState;
        isPreviewing = false;

        RefreshVisual();
    }

    public void SetColorState(PaintColorState colorState)
    {
        if (State == CellState.Empty)
            return;

        ColorState = colorState;
        isPreviewing = false;

        RefreshVisual();
    }

    public void Clear()
    {
        State = CellState.Empty;
        ColorState = PaintColorState.None;
        isPreviewing = false;

        RefreshVisual();
    }

    public void ForceRefreshVisual()
    {
        isPreviewing = false;
        RefreshVisual();
    }

    public void ShowPreview(bool isValid)
    {
        if (cellImage == null)
            return;

        isPreviewing = true;

        cellImage.enabled = true;
        cellImage.sprite = GetEmptyRuntimeSprite();
        cellImage.type = Image.Type.Simple;
        cellImage.preserveAspect = false;
        cellImage.color = isValid ? ValidPreviewColor : InvalidPreviewColor;
    }

    public void ShowPreviewColor(Color previewColor)
    {
        if (cellImage == null)
            return;

        isPreviewing = true;

        Sprite previewSprite = GetPreviewSprite();

        cellImage.enabled = true;
        cellImage.sprite = previewSprite != null ? previewSprite : GetEmptyRuntimeSprite();
        cellImage.type = Image.Type.Simple;
        cellImage.preserveAspect = false;

        Color color = previewColor;
        color.a = 0.78f;

        cellImage.color = color;
    }

    public void ShowPreviewColorState(PaintColorState previewColorState)
    {
        if (cellImage == null)
            return;

        isPreviewing = true;

        Sprite blockSprite = GetBlockSprite(previewColorState);

        cellImage.enabled = true;
        cellImage.type = Image.Type.Simple;
        cellImage.preserveAspect = false;

        if (blockSprite != null)
        {
            cellImage.sprite = blockSprite;
            cellImage.color = Color.white;
            return;
        }

        cellImage.sprite = GetEmptyRuntimeSprite();

        Color color = PaintColorRules.ToColor(previewColorState);
        color.a = 0.78f;

        cellImage.color = color;
    }

    public void HidePreview()
    {
        if (!isPreviewing)
            return;

        isPreviewing = false;
        RefreshVisual();
    }

    public Color GetCurrentVisualColor()
    {
        if (State == CellState.Filled && ColorState != PaintColorState.None)
        {
            return PaintColorRules.ToColor(ColorState);
        }

        if (cellImage == null)
            return Color.white;

        return cellImage.color;
    }

    private void RefreshVisual()
    {
        if (cellImage == null)
            return;

        cellImage.enabled = true;
        cellImage.type = Image.Type.Simple;
        cellImage.preserveAspect = false;

        if (State == CellState.Empty)
        {
            cellImage.sprite = GetEmptyRuntimeSprite();
            cellImage.color = EmptyCellColor;
            return;
        }

        Sprite blockSprite = GetBlockSprite(ColorState);

        if (blockSprite != null)
        {
            cellImage.sprite = blockSprite;
            cellImage.color = Color.white;
            return;
        }

        cellImage.sprite = GetEmptyRuntimeSprite();
        cellImage.color = PaintColorRules.ToColor(ColorState);
    }

    private Sprite GetBlockSprite(PaintColorState colorState)
    {
        if (PaintColorSpriteLibrary.Instance == null)
            return null;

        return PaintColorSpriteLibrary.Instance.GetBlockSprite(colorState);
    }

    private Sprite GetPreviewSprite()
    {
        if (previewBaseSprite != null)
            return previewBaseSprite;

        if (PaintColorSpriteLibrary.Instance != null)
        {
            return PaintColorSpriteLibrary.Instance.GetPreviewBaseSprite();
        }

        return null;
    }

    private Sprite GetEmptyRuntimeSprite()
    {
        EnsureRuntimeEmptySprite();
        return runtimeEmptySprite;
    }

    private static void EnsureRuntimeEmptySprite()
    {
        if (runtimeEmptySprite != null)
            return;

        Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color32 white = new Color32(255, 255, 255, 255);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, white);
            }
        }

        texture.Apply();

        runtimeEmptySprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f
        );

        runtimeEmptySprite.name = "Runtime_Empty_Cell_Sprite";
    }
}