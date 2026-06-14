using Umbraco.Cms.Core.PropertyEditors;

namespace Digbyswift.Umbraco.ImageTypeChecker.PropertyEditors;

[DataEditor(Constants.PropertyEditorAlias, ValueType = ValueTypes.Json)]
public sealed class ImageTypeCheckerDataEditor(IDataValueEditorFactory dataValueEditorFactory) : DataEditor(dataValueEditorFactory);
