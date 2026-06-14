using Digbyswift.Umbraco.ImageTypeChecker.Models;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Umbraco.Cms.Core.IO;

namespace Digbyswift.Umbraco.ImageTypeChecker.Services;

public sealed class ImageTypeAnalyser(IFileSystem fileSystemProvider, ILogger<ImageTypeAnalyser> logger)
{
    private const int DefaultColourBucketThreshold = 200;
    private const int ColourBucketSize = 16;
    private const int SampleGridSize = 32;
    private const double TransparencyRatioThreshold = 0.10;
    private const long MaxPixelCount = 25_000_000;

    private readonly ILogger _logger = logger;

    public async Task<ImageTypeAnalysis> AnalyseAsync(string relativeImagePath, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(relativeImagePath);
        if (extension == ".gif")
        {
            _logger.LogInformation("Media {path} has .gif extension, so assuming is graphical #media", relativeImagePath);
            return ImageTypeAnalysis.Graphic;
        }

        var imageHasMetColourBucketThreshold = false;
        var imageHasTransparency = false;
        var imageCheckData = default(ImageCheckData);

        await using (var stream = fileSystemProvider.OpenFile(relativeImagePath))
        {
            var imageInfo = await Image.IdentifyAsync(stream, cancellationToken);
            if (imageInfo == null)
            {
                _logger.LogInformation("Media {path} could not be identified. Image type is unknown #media", relativeImagePath);
                return ImageTypeAnalysis.Unknown;
            }

            var pixelCount = (long)imageInfo.Width * imageInfo.Height;
            if (pixelCount > MaxPixelCount)
            {
                _logger.LogInformation("Media {path} has {pixelCount} pixels, which exceeds the maximum of {maxPixelCount}. Image type is unknown #media", relativeImagePath, pixelCount, MaxPixelCount);
                return ImageTypeAnalysis.Unknown;
            }

            if (IsJpeg(extension) && (imageInfo.Metadata.ExifProfile?.Values.Any() ?? false))
            {
                _logger.LogInformation("Media {path} sampled has exif data, so can safely considered photographic #media", relativeImagePath);
                return ImageTypeAnalysis.Photo;
            }

            using (var image = await LoadImageAsync(relativeImagePath, stream, cancellationToken))
            {
                // https://docs.sixlabors.com/articles/imagesharp/pixelbuffers.html
                image.ProcessPixelRows(accessor =>
                {
                    imageCheckData = CheckImageColours(accessor);

                    imageHasMetColourBucketThreshold = imageCheckData.HasMetColourBucketThreshold;
                    imageHasTransparency = imageCheckData.HasTransparency;
                });
            }
        }

        if (imageHasMetColourBucketThreshold && !imageHasTransparency)
        {
            _logger.LogInformation("Media {path} sampled has more than {maxColours} colour buckets, so cannot be considered graphical #media", DefaultColourBucketThreshold, relativeImagePath);
            return ImageTypeAnalysis.Photo;
        }

        _logger.LogInformation(imageHasTransparency
            ? "Media {path} sampled has transparency ratio {transparencyRatio:P2} with {colourBucketCount} colour buckets, so is considered graphical #media"
            : "Media {path} sampled has transparency ratio {transparencyRatio:P2} with {colourBucketCount} colour buckets, so is considered a graphic #media",
            relativeImagePath,
            imageCheckData.TransparencyRatio,
            imageCheckData.ColourBucketCount);

        return ImageTypeAnalysis.Graphic;
    }

    private static ImageCheckData CheckImageColours(PixelAccessor<Rgba32> accessor)
    {
        var colourBuckets = new HashSet<int>();
        var transparentSampleCount = 0;
        var sampleCount = 0;
        var gridWidth = Math.Min(SampleGridSize, accessor.Width);
        var gridHeight = Math.Min(SampleGridSize, accessor.Height);

        for (var gridY = 0; gridY < gridHeight; gridY++)
        {
            var y = GetSampleCoordinate(gridY, gridHeight, accessor.Height);
            var pixelRow = accessor.GetRowSpan(y);

            for (var gridX = 0; gridX < gridWidth; gridX++)
            {
                var x = GetSampleCoordinate(gridX, gridWidth, accessor.Width);

                // Get a reference to the pixel at position x
                ref var pixel = ref pixelRow[x];
                sampleCount++;

                if (pixel.A == 0)
                {
                    transparentSampleCount++;
                    continue;
                }

                // Allow for a tolerance of black and white pixels
                var packedColour = pixel.IsWhite(byteTolerance: 5) || pixel.IsBlack(byteTolerance: 5)
                    ? new Rgba32(255, 255, 255)
                    : pixel;

                colourBuckets.Add(GetColourBucketKey(packedColour));
            }
        }

        var transparencyRatio = sampleCount == 0
            ? 0
            : transparentSampleCount / (double)sampleCount;

        if (transparencyRatio > TransparencyRatioThreshold)
            return ImageCheckData.WithTransparency(colourBuckets.Count, transparencyRatio);

        if (colourBuckets.Count >= DefaultColourBucketThreshold)
            return ImageCheckData.WithColourBucketThresholdMet(colourBuckets.Count, transparencyRatio);

        return ImageCheckData.WithColourBuckets(colourBuckets.Count, transparencyRatio);
    }

    private static bool IsJpeg(string extension)
    {
        return String.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
            || String.Equals(extension, ".jpeg", StringComparison.OrdinalIgnoreCase);
    }

    private static int GetSampleCoordinate(int index, int sampleCount, int length)
    {
        if (sampleCount <= 1)
            return 0;

        return (int)Math.Round(index * (length - 1) / (double)(sampleCount - 1));
    }

    private static int GetColourBucketKey(Rgba32 pixel)
    {
        var r = pixel.R / ColourBucketSize;
        var g = pixel.G / ColourBucketSize;
        var b = pixel.B / ColourBucketSize;

        return (r << 8) | (g << 4) | b;
    }

    private async Task<Image<Rgba32>> LoadImageAsync(string relativeImagePath, Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;

            var decoderOptions = new DecoderOptions
            {
                TargetSize = new Size(256, 256),
                SkipMetadata = false
            };

            return await Image.LoadAsync<Rgba32>(decoderOptions, stream, cancellationToken);
        }

        await using var imageStream = fileSystemProvider.OpenFile(relativeImagePath);
        return await Image.LoadAsync<Rgba32>(imageStream, cancellationToken);
    }
}
