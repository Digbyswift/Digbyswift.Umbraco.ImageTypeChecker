using Digbyswift.Umbraco.ImageTypeChecker.Notifications;
using Digbyswift.Umbraco.ImageTypeChecker.PropertyValueConverters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Notifications;
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
                var logger = provider.GetRequiredService<ILogger<ImageTypeCheckMediaWhenSavingAsyncHandler>>();

                return new ImageTypeCheckMediaWhenSavingAsyncHandler(mediaFileManager.FileSystem, logger);
            });

        builder.PropertyValueConverters().Insert<ImageTypeCheckerPropertyValueConverter>();
    }
}
