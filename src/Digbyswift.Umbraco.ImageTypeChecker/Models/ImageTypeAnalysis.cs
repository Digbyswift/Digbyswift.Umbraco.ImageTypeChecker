namespace Digbyswift.Umbraco.ImageTypeChecker.Models;

public struct ImageTypeAnalysis
{
    public static ImageTypeAnalysis Unknown => new() { ImageType = ImageType.Unknown };
    public static ImageTypeAnalysis Photo => new() { ImageType = ImageType.Photo };
    public static ImageTypeAnalysis Graphic => new() { ImageType = ImageType.Graphic };

    public ImageType ImageType { get; set; }
    public bool ManuallyOverridden { get; set; }
}
