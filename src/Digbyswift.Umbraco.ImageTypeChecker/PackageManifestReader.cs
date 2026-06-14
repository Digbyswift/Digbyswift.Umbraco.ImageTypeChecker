using System.Diagnostics;
using System.Reflection;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace Digbyswift.Umbraco.ImageTypeChecker;

public class PackageManifestReader : IPackageManifestReader
{
    public Task<IEnumerable<PackageManifest>> ReadPackageManifestsAsync()
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        var version = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}";

        IEnumerable<PackageManifest> manifests =
        [
            new()
            {
                Name = versionInfo.ProductName ?? versionInfo.FileName,
                Version = version,
                AllowTelemetry = true,
                Extensions =
                [
                    new
                    {
                        name = Constants.ProjectName,
                        alias = Constants.ProjectNamespace,
                        type = "bundle",
                        js = $"{Constants.PackagePathRoot}manifests.js?version={version}",
                    },
                ],
            }
        ];

        return Task.FromResult(manifests);
    }
}
