using UnityEngine;

[CreateAssetMenu(menuName = "Elemental Grid/Block Data")]
public class BlockData : ScriptableObject
{
    [Header("Basic Info")]
    public string blockName;

    [Header("Shape Type")]
    public BlockRole role = BlockRole.Small;

    [Header("Shape Cells")]
    public Vector2Int[] cells;

    [Header("Visual")]
    public Color defaultColor = new Color32(90, 150, 255, 255);

    public int CellCount
    {
        get
        {
            if (cells == null)
                return 0;

            return cells.Length;
        }
    }
}