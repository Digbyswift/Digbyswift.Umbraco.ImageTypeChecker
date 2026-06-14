using System.Text.Json;
using Digbyswift.Umbraco.ImageTypeChecker.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Digbyswift.Umbraco.ImageTypeChecker.PropertyValueConverters;

public class ImageTypeCheckerPropertyValueConverter : PropertyValueConverterBase
{
    public override bool IsConverter(IPublishedPropertyType propertyType)
    {
        return String.Equals(propertyType.EditorAlias, Constants.PropertyEditorAlias, StringComparison.OrdinalIgnoreCase);
    }

    public override Type GetPropertyValueType(IPublishedPropertyType propertyType) => typeof(ImageTypeAnalysis);

    public override object? ConvertIntermediateToObject(IPublishedElement owner, IPublishedPropertyType propertyType, PropertyCacheLevel referenceCacheLevel, object? inter, bool preview)
    {
        if (inter is not JsonDocument jsonDocument)
            return null;

        return jsonDocument.Deserialize<ImageTypeAnalysis>();
    }
}