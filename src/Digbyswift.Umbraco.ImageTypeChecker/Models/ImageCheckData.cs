namespace Digbyswift.Umbraco.ImageTypeChecker.Models;

public readonly struct ImageCheckData
{
    public readonly bool HasMetColourCountThreshold;
    public readonly bool HasTransparency;

    private ImageCheckData(bool hasMetColourCountThreshold = false, bool hasTransparency = false)
    {
        HasMetColourCountThreshold = hasMetColourCountThreshold;
        HasTransparency = hasTransparency;
    }

    public static ImageCheckData WithTransparency => new(hasTransparency: true);
    public static ImageCheckData WithColourCountThresholdMet => new(hasMetColourCountThreshold: true);
}