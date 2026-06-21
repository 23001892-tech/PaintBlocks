using UnityEngine;

public class PaintColorSpriteLibrary : MonoBehaviour
{
    public static PaintColorSpriteLibrary Instance { get; private set; }

    [Header("Block Color Sprites")]
    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite yellowSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite orangeSprite;
    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite purpleSprite;
    [SerializeField] private Sprite ashSprite;

    [Header("Board Sprites")]
    [SerializeField] private Sprite emptyCellSprite;

    [Header("Preview Fallback")]
    [SerializeField] private Sprite previewBaseSprite;

    private void Awake()
    {
        Instance = this;
    }

    public Sprite GetBlockSprite(PaintColorState colorState)
    {
        switch (colorState)
        {
            case PaintColorState.Red:
                return redSprite;

            case PaintColorState.Yellow:
                return yellowSprite;

            case PaintColorState.Blue:
                return blueSprite;

            case PaintColorState.Orange:
                return orangeSprite;

            case PaintColorState.Green:
                return greenSprite;

            case PaintColorState.Purple:
                return purpleSprite;

            case PaintColorState.Ash:
                return ashSprite;

            default:
                return null;
        }
    }

    public Sprite GetEmptyCellSprite()
    {
        return emptyCellSprite;
    }

    public Sprite GetPreviewBaseSprite()
    {
        if (previewBaseSprite != null)
            return previewBaseSprite;

        if (ashSprite != null)
            return ashSprite;

        if (redSprite != null)
            return redSprite;

        return null;
    }
}