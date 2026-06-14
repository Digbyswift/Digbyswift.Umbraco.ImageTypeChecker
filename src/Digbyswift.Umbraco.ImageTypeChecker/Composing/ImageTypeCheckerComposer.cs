using Digbyswift.Umbraco.ImageTypeChecker.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Manifest;

namespace Digbyswift.Umbraco.ImageTypeChecker.Composing;

public sealed class ImageTypeCheckerComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services
            .AddSingleton<IPackageManifestReader, PackageManifestReader>();

        builder.AddNotificationAsyncHandler<MediaSavingNotification, ImageTypeCheckMediaWhenSavingAsyncHandler>();
    }
}
