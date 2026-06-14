using Digbyswift.Umbraco.ImageTypeChecker.Notifications;
using Digbyswift.Umbraco.ImageTypeChecker.PropertyValueConverters;
using Digbyswift.Umbraco.ImageTypeChecker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Manifest;

namespace Digbyswift.Umbraco.ImageTypeChecker.Composing;

public sealed class ImageTypeCheckerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<IPackageManifestReader, PackageManifestReader>();

        // Register the notification with the relevant file manager dependency. Because of this we can't
        // use the standard `AddNotificationAsyncHandler<,>() method.
        builder.Services.AddTransient<INotificationAsyncHandler<MediaSavingNotification>>(provider =>
            {
                var mediaFileManager = provider.GetRequiredService<MediaFileManager>();
                var jsonSerializer = provider.GetRequiredService<IJsonSerializer>();
                var imageTypeAnalyserLogger = provider.GetRequiredService<ILogger<ImageTypeAnalyser>>();
                var logger = provider.GetRequiredService<ILogger<ImageTypeCheckMediaWhenSavingAsyncHandler>>();
                var imageTypeAnalyser = new ImageTypeAnalyser(mediaFileManager.FileSystem, imageTypeAnalyserLogger);

                return new ImageTypeCheckMediaWhenSavingAsyncHandler(jsonSerializer, imageTypeAnalyser, logger);
            });

        builder.PropertyValueConverters().Insert<ImageTypeCheckerPropertyValueConverter>();
    }
}
