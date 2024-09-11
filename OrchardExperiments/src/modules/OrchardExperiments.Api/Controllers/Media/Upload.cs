using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media;
using OrchardCore.Media.Services;

namespace OrchardExperiments.Api.Controllers.Media;

[IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public class Upload(
    IAuthenticationService authenticationService,
    IAuthorizationService authorizationService,
    IMediaNameNormalizerService mediaNameNormalizerService,
    IMediaFileStore mediaFileStore,
    IContentTypeProvider mediaContentTypeProvider,
    IFileVersionProvider fileVersionProvider,
    IOptions<MediaOptions> options,
    ILogger<Upload> logger) : ControllerBase
{
    private static readonly char[] ExtensionSeparator = [' ', ','];

    [HttpPost("api/media/upload")]
    [MediaSizeLimit]
    public async Task<IActionResult> HandleAsync(string? path)
    {
        var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, Schemes.Api);
        if (authenticateResult.Succeeded)
            HttpContext.User = authenticateResult.Principal;

        if (!await authorizationService.AuthorizeAsync(User, OrchardCore.Media.Permissions.ManageMedia))
            return this.ChallengeOrForbid(Schemes.Api);

        var allowedExtensions = options.Value.AllowedFileExtensions;
        if (string.IsNullOrEmpty(path)) path = string.Empty;
        var files = Request.Form.Files;
        var result = new List<object>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            var fileName = Path.GetFileName(file.FileName);

            if (allowedExtensions == null || !allowedExtensions.Contains(extension))
            {
                result.Add(new
                {
                    name = file.FileName,
                    size = file.Length,
                    folder = path,
                    error = $"This file extension is not allowed: {extension}"
                });
                continue;
            }

            var normalizedFileName = mediaNameNormalizerService.NormalizeFileName(file.FileName);

            try
            {
                var mediaFilePath = mediaFileStore.Combine(path, normalizedFileName);
                await using var stream = file.OpenReadStream();
                mediaFilePath = await mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, stream);
                var mediaFile = await mediaFileStore.GetFileInfoAsync(mediaFilePath);
                result.Add(CreateFileResult(mediaFile));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while uploading a file");
                result.Add(new
                {
                    name = fileName,
                    size = file.Length,
                    folder = path,
                    error = ex.Message
                });
            }
        }

        return Ok(new { files = result.ToArray() });
    }

    private object CreateFileResult(IFileStoreEntry mediaFile)
    {
        mediaContentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        return new
        {
            name = mediaFile.Name,
            size = mediaFile.Length,
            lastModify = mediaFile.LastModifiedUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
            folder = mediaFile.DirectoryPath,
            url = GetCacheBustingMediaPublicUrl(mediaFile.Path),
            mediaPath = mediaFile.Path,
            mime = contentType ?? "application/octet-stream",
            mediaText = string.Empty,
            anchor = new { x = 0.5f, y = 0.5f },
            attachedFileName = string.Empty
        };
    }

    private string GetCacheBustingMediaPublicUrl(string path)
    {
        return fileVersionProvider.AddFileVersionToPath(HttpContext.Request.PathBase, mediaFileStore.MapPathToPublicUrl(path));
    }
}