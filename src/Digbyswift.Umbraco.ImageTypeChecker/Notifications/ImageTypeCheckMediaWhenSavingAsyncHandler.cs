#pragma warning disable CS0618 // Type or member is obsolete

using System.Text.Json;
using System.Text.Json.Serialization;
using Digbyswift.Core.Extensions;
using Digbyswift.Umbraco.ImageTypeChecker.Services;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;
using uConstants = Umbraco.Cms.Core.Constants;

namespace Digbyswift.Umbraco.ImageTypeChecker.Notifications;

public sealed class ImageTypeCheckMediaWhenSavingAsyncHandler : INotificationAsyncHandler<MediaSavingNotification>
{
    private static readonly string[] SupportedTypes = [".jpeg", ".jpg", ".gif", ".webp", ".bmp", ".png", ".tiff", ".tif"];

    private readonly ILogger _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ImageTypeAnalyser _imageTypeAnalyser;

    public ImageTypeCheckMediaWhenSavingAsyncHandler(
        IJsonSerializer jsonSerializer,
        ImageTypeAnalyser imageTypeAnalyser,
        ILogger<ImageTypeCheckMediaWhenSavingAsyncHandler> logger)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _imageTypeAnalyser = imageTypeAnalyser;
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
                var isDirty = media.IsPropertyDirty(uConstants.Conventions.Media.File);
                if (value != null && !isDirty)
                    continue;

                if (!TryGetFilePath(media, out var relativeImagePath))
                {
                    // Skip analysis and nullify analysis value if there is no file set.
                    if (String.IsNullOrWhiteSpace(relativeImagePath))
                        UnsetValue(media, analysisPropertyAlias);

                    continue;
                }

                var analysis = await _imageTypeAnalyser.AnalyseAsync(relativeImagePath!, cancellationToken);
                SetJsonValue(media, analysisPropertyAlias, analysis);
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

    private bool TryGetFilePath(IMedia media, out string? relativeImagePath)
    {
        relativeImagePath = null;

        if (!media.HasProperty(uConstants.Conventions.Media.File))
            return false;

        var umbracoFileValue = media.GetValue<string>(uConstants.Conventions.Media.File);
        if (String.IsNullOrWhiteSpace(umbracoFileValue))
            return false;

        if (umbracoFileValue.DetectIsJson())
        {
            var imageCropperValue = _jsonSerializer.Deserialize<ImageCropperValue>(umbracoFileValue);

            relativeImagePath = imageCropperValue?.Src;
        }
        else
        {
            relativeImagePath = umbracoFileValue;
        }

        if (String.IsNullOrWhiteSpace(relativeImagePath))
            return false;

        var extension = Path.GetExtension(relativeImagePath);
        return SupportedTypes.ContainsIgnoreCase(extension);
    }
}
