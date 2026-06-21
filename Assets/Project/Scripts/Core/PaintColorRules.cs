using UnityEngine;

public static class PaintColorRules
{
    public static bool IsPrimary(PaintColorState colorState)
    {
        return colorState == PaintColorState.Red
            || colorState == PaintColorState.Yellow
            || colorState == PaintColorState.Blue;
    }

    public static bool IsSecondary(PaintColorState colorState)
    {
        return colorState == PaintColorState.Orange
            || colorState == PaintColorState.Green
            || colorState == PaintColorState.Purple;
    }

    public static bool IsLocked(PaintColorState colorState)
    {
        return IsSecondary(colorState) || colorState == PaintColorState.Ash;
    }

    public static PaintColorState MixTwoPrimaryColors(PaintColorState a, PaintColorState b)
    {
        if (!IsPrimary(a) || !IsPrimary(b))
            return PaintColorState.None;

        if (a == b)
            return a;

        if (IsRedYellow(a, b))
            return PaintColorState.Orange;

        if (IsYellowBlue(a, b))
            return PaintColorState.Green;

        if (IsBlueRed(a, b))
            return PaintColorState.Purple;

        return PaintColorState.None;
    }

    public static bool IsThirdPrimaryAgainstSecondary(PaintColorState primary, PaintColorState secondary)
    {
        if (!IsPrimary(primary) || !IsSecondary(secondary))
            return false;

        switch (secondary)
        {
            case PaintColorState.Orange:
                // Cam = Đỏ + Vàng, nên Lam là màu thứ ba
                return primary == PaintColorState.Blue;

            case PaintColorState.Green:
                // Lục = Vàng + Lam, nên Đỏ là màu thứ ba
                return primary == PaintColorState.Red;

            case PaintColorState.Purple:
                // Tím = Lam + Đỏ, nên Vàng là màu thứ ba
                return primary == PaintColorState.Yellow;

            default:
                return false;
        }
    }

    public static bool IsPrimaryContainedInSecondary(PaintColorState primary, PaintColorState secondary)
    {
        if (!IsPrimary(primary) || !IsSecondary(secondary))
            return false;

        switch (secondary)
        {
            case PaintColorState.Orange:
                return primary == PaintColorState.Red || primary == PaintColorState.Yellow;

            case PaintColorState.Green:
                return primary == PaintColorState.Yellow || primary == PaintColorState.Blue;

            case PaintColorState.Purple:
                return primary == PaintColorState.Blue || primary == PaintColorState.Red;

            default:
                return false;
        }
    }

    private static bool IsRedYellow(PaintColorState a, PaintColorState b)
    {
        return (a == PaintColorState.Red && b == PaintColorState.Yellow)
            || (a == PaintColorState.Yellow && b == PaintColorState.Red);
    }

    private static bool IsYellowBlue(PaintColorState a, PaintColorState b)
    {
        return (a == PaintColorState.Yellow && b == PaintColorState.Blue)
            || (a == PaintColorState.Blue && b == PaintColorState.Yellow);
    }

    private static bool IsBlueRed(PaintColorState a, PaintColorState b)
    {
        return (a == PaintColorState.Blue && b == PaintColorState.Red)
            || (a == PaintColorState.Red && b == PaintColorState.Blue);
    }

    public static Color ToColor(PaintColorState colorState)
    {
        switch (colorState)
        {
            case PaintColorState.Red:
                return new Color32(235, 55, 70, 255);

            case PaintColorState.Yellow:
                return new Color32(250, 200, 45, 255);

            case PaintColorState.Blue:
                return new Color32(55, 110, 220, 255);

            case PaintColorState.Orange:
                return new Color32(245, 125, 30, 255);

            case PaintColorState.Green:
                return new Color32(75, 170, 80, 255);

            case PaintColorState.Purple:
                return new Color32(145, 90, 170, 255);

            case PaintColorState.Ash:
                return new Color32(105, 95, 85, 255);

            default:
                return new Color32(70, 70, 80, 255);
        }
    }
}