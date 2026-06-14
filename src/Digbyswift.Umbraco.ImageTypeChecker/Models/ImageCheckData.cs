namespace Digbyswift.Umbraco.ImageTypeChecker.Models;

public readonly struct ImageCheckData
{
    public readonly bool HasMetColourBucketThreshold;
    public readonly bool HasTransparency;
    public readonly int ColourBucketCount;
    public readonly double TransparencyRatio;

    private ImageCheckData(
        bool hasMetColourBucketThreshold = false,
        bool hasTransparency = false,
        int colourBucketCount = 0,
        double transparencyRatio = 0)
    {
        HasMetColourBucketThreshold = hasMetColourBucketThreshold;
        HasTransparency = hasTransparency;
        ColourBucketCount = colourBucketCount;
        TransparencyRatio = transparencyRatio;
    }

    public static ImageCheckData WithTransparency(int colourBucketCount, double transparencyRatio) => new(
        hasTransparency: true,
        colourBucketCount: colourBucketCount,
        transparencyRatio: transparencyRatio);

    public static ImageCheckData WithColourBucketThresholdMet(int colourBucketCount, double transparencyRatio) => new(
        hasMetColourBucketThreshold: true,
        colourBucketCount: colourBucketCount,
        transparencyRatio: transparencyRatio);

    public static ImageCheckData WithColourBuckets(int colourBucketCount, double transparencyRatio) => new(
        colourBucketCount: colourBucketCount,
        transparencyRatio: transparencyRatio);
}
