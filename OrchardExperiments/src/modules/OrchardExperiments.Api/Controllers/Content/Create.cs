using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardExperiments.Api.Extensions;

namespace OrchardExperiments.Api.Controllers.Content;

[IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public class Create(IAuthenticationService authenticationService, IAuthorizationService authorizationService, IContentManager contentManager) : ControllerBase
{
    [HttpPost("api/content-items")]
    public async Task<IActionResult> HandleAsync(RequestModel request)
    {
        var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, Schemes.Api);
        if (authenticateResult.Succeeded)
            HttpContext.User = authenticateResult.Principal;

        if (!await authorizationService.AuthorizeAsync(User, Permissions.RestApiAccess))
            return this.ChallengeOrForbid(Schemes.Api);

        var contentItem = await CreateContentItemOwnedByCurrentUserAsync(request.ContentType);

        if (!await authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            return this.ChallengeOrForbid(Schemes.Api);

        var properties = request.Properties;
        contentItem.Merge(properties, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        
        await contentManager.UpdateAsync(contentItem);
        await contentManager.CreateAsync(contentItem, VersionOptions.Draft);

        if (request.Publish)
            await contentManager.PublishAsync(contentItem);
        else
            await contentManager.SaveDraftAsync(contentItem);

        return new ObjectResult(contentItem);
    }
    
    private async Task<ContentItem> CreateContentItemOwnedByCurrentUserAsync(string contentType)
    {
        var contentItem = await contentManager.NewAsync(contentType);
        contentItem.Owner = CurrentUserId();

        return contentItem;
    }

    

    private string CurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }

    public record RequestModel(string ContentType, JsonNode Properties, bool Publish);
}