using SixLabors.ImageSharp.PixelFormats;

namespace Digbyswift.Umbraco.ImageTypeChecker.Models;

public static class Rgb32Extensions
{
    public static bool IsWhite(this Rgba32 pixel, byte? byteTolerance = null)
    {
        if (byteTolerance == null && pixel.Rgb is { R: Byte.MaxValue, G: Byte.MaxValue, B: Byte.MaxValue })
            return true;

        if (byteTolerance == null)
            return false;

        var maxThreshold = Byte.MaxValue - byteTolerance.Value;

        return pixel.Rgb.R >= maxThreshold && pixel.Rgb.G >= maxThreshold && pixel.Rgb.B >= maxThreshold;
    }

    public static bool IsBlack(this Rgba32 pixel, byte? byteTolerance = null)
    {
        if (byteTolerance == null && pixel.Rgb is { R: Byte.MinValue, G: Byte.MinValue, B: Byte.MinValue })
            return true;

        if (byteTolerance == null)
            return false;

        var minThreshold = Byte.MinValue + byteTolerance.Value;

        return pixel.Rgb.R <= minThreshold && pixel.Rgb.G <= minThreshold && pixel.Rgb.B <= minThreshold;
    }
}
