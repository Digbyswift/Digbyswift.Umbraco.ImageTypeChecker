namespace Digbyswift.Umbraco.ImageTypeChecker.Models;

public enum ImageType
{
    /// <summary>
    /// An unknown image type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// A photo image type, e.g. one with a large number of colours.
    /// </summary>
    Photo,

    /// <summary>
    /// A graphic image, e.g. one with a low number of colours.
    /// </summary>
    Graphic
}