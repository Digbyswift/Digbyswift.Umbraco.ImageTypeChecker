#pragma warning disable CS0618 // Type or member is obsolete

using System.Text.Json;
using System.Text.Json.Serialization;
using Digbyswift.Core.Extensions;
using Digbyswift.Umbraco.ImageTypeChecker.Models;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Extensions;
using uConstants = Umbraco.Cms.Core.Constants;

namespace Digbyswift.Umbraco.ImageTypeChecker.Notifications;

public sealed class ImageTypeCheckMediaWhenSavingAsyncHandler : INotificationAsyncHandler<MediaSavingNotification>
{
    private const int DefaultColourCountThreshold = 500;
    private static readonly string[] SupportedTypes = [".jpeg", ".jpg", ".gif", ".webp", ".bmp", ".png", ".tiff", ".tif"];

    private readonly ILogger _logger;
    private readonly IFileSystem _fileSystemProvider;

    public ImageTypeCheckMediaWhenSavingAsyncHandler(IFileSystem fileSystemProvider, ILogger<ImageTypeCheckMediaWhenSavingAsyncHandler> logger)
    {
        _logger = logger;
        _fileSystemProvider = fileSystemProvider;
    }

    public async Task HandleAsync(MediaSavingNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var media in notification.SavedEntities)
            {
                // Skip analysis if there is no analysis property defined
                var analysisPropertyAlias = GetAnalysisPropertyAlias(media);
                if (analysisPropertyAlias == null)
                    continue;

                // Skip image analysis if the analysis property has a value and
                // the file property isn't dirty.
                var value = media.GetValue(analysisPropertyAlias);
                if (value != null && !media.IsPropertyDirty(uConstants.Conventions.Media.File))
                    continue;

                // Skip analysis and nullify analysis value if there is no file set.
                if (TryGetFilePath(media, out var relativeImagePath) && String.IsNullOrWhiteSpace(relativeImagePath))
                {
                    UnsetValue(media, analysisPropertyAlias);
                    continue;
                }

                // Default the analysis value to Graphic if the file is a GIF format
                var extension = Path.GetExtension(relativeImagePath);
                if (extension == ".gif")
                {
                    _logger.LogInformation("Media {path} has .gif extension, so assuming is graphical #media", relativeImagePath);
                    SetJsonValue(media, analysisPropertyAlias, ImageTypeAnalysis.Graphic);
                    continue;
                }

                var imageHasMetColourCountThreshold = false;
                var imageHasTransparency = false;

                await using (var stream = _fileSystemProvider.OpenFile(relativeImagePath!))
                {
                    using (var image = await Image.LoadAsync<Rgba32>(stream, cancellationToken))
                    {
                        if (image.Metadata.ExifProfile != null && image.Metadata.ExifProfile.Values.Any())
                        {
                            _logger.LogInformation("Media {path} sampled has exif data, so cannot be considered graphical #media", relativeImagePath);
                            SetJsonValue(media, analysisPropertyAlias, ImageTypeAnalysis.Photo);
                            continue;
                        }

                        // https://docs.sixlabors.com/articles/imagesharp/pixelbuffers.html
                        image.ProcessPixelRows(accessor =>
                        {
                            var colourHashSet = new HashSet<Rgb>();

                            const int sampleSize = 4;

                            var rowCount = accessor.Height / sampleSize;

                            for (var i = 0; i < sampleSize; i++)
                            {
                                // Never index initial row as this may
                                // be a solid colour or transparent.
                                var rowIndex = i == 0 ? 5 : i * rowCount;

                                var imageRowCheckData = CheckRowColours(colourHashSet, accessor.GetRowSpan(rowIndex));
                                if (imageRowCheckData.HasTransparency || imageRowCheckData.HasMetColourCountThreshold)
                                {
                                    imageHasMetColourCountThreshold = imageRowCheckData.HasMetColourCountThreshold;
                                    imageHasTransparency = imageRowCheckData.HasTransparency;
                                    return;
                                }
                            }
                        });
                    }
                }

                if (imageHasMetColourCountThreshold && !imageHasTransparency)
                {
                    SetJsonValue(media, analysisPropertyAlias, ImageTypeAnalysis.Photo);
                    _logger.LogInformation("Media {path} sampled has more than {maxColours} colours, so cannot be considered graphical #media", DefaultColourCountThreshold, relativeImagePath);
                }
                else
                {
                    SetJsonValue(media, analysisPropertyAlias, ImageTypeAnalysis.Graphic);

                    _logger.LogInformation(imageHasTransparency
                        ? "Media {path} sampled has transparency, so is considered graphical #media"
                        : "Media {path} sampled has a low colour count, so is considered a graphic #media", relativeImagePath);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Media colour count failed #media");
        }
    }

    private static string? GetAnalysisPropertyAlias(IMedia media)
    {
        return media.Properties
            .FirstOrDefault(x => String.Equals(
                x.PropertyType.PropertyEditorAlias,
                Constants.PropertyEditorAlias,
                StringComparison.OrdinalIgnoreCase)
            )?
            .Alias;
    }

    private static bool TryGetFilePath(IMedia media, out string? relativeImagePath)
    {
        relativeImagePath = null;

        if (!media.HasProperty(uConstants.Conventions.Media.File))
            return false;

        var umbracoFileValue = media.GetValue<string>(uConstants.Conventions.Media.File);
        if (String.IsNullOrWhiteSpace(umbracoFileValue))
            return false;

        if (umbracoFileValue.DetectIsJson())
        {
            var cropObject = JsonSerializer.Deserialize<ImageCropperValue>(umbracoFileValue);
            if (cropObject == null)
                return false;

            relativeImagePath = cropObject.Src;
        }
        else
        {
            relativeImagePath = umbracoFileValue;
        }

        if (String.IsNullOrWhiteSpace(relativeImagePath))
            return false;

        var extension = Path.GetExtension(relativeImagePath);
        if (!SupportedTypes.ContainsIgnoreCase(extension))
            return false;

        return true;
    }

    private static ImageCheckData CheckRowColours(HashSet<Rgb> colourHashSet, Span<Rgba32> pixelRow)
    {
        for (var x = 0; x < pixelRow.Length - 1; x++)
        {
            // Get a reference to the pixel at position x
            ref var pixel = ref pixelRow[x];

            if (pixel.A == 0)
                return ImageCheckData.WithTransparency;

            // Allow for a tolerance of black and white pixels
            var packedColour = pixel.IsWhite(byteTolerance: 5) || pixel.IsBlack(byteTolerance: 5)
                ? new Rgba32(255, 255, 255)
                : pixel;

            if (colourHashSet.Add(packedColour) && colourHashSet.Count >= DefaultColourCountThreshold)
                return ImageCheckData.WithColourCountThresholdMet;
        }

        return default;
    }

    private static void SetJsonValue<T>(IContentBase contentBase, string propertyAlias, T objectValue, JsonSerializerOptions? serializerOptions = null)
    {
        var jsonValue = JsonSerializer.Serialize(objectValue, serializerOptions ?? new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        });

        contentBase.SetValue(propertyAlias, value: jsonValue);
    }

    private static void UnsetValue(IContentBase contentBase, string propertyAlias)
    {
        contentBase.SetValue(propertyAlias, null);
    }
}