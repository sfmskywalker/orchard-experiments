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
    [HttpPost("api/content")]
    public async Task<IActionResult> HandleAsync(RequestModel request)
    {
        var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, Schemes.Api);
        if (authenticateResult.Succeeded)
            HttpContext.User = authenticateResult.Principal;

        if (!await authorizationService.AuthorizeAsync(User, Permissions.RestApiAccess))
            return this.ChallengeOrForbid(Schemes.Api);

        var contentItem = await contentManager.NewAsync(request.ContentType);

        if (!await authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            return this.ChallengeOrForbid(Schemes.Api);

        var properties = request.Properties;
        contentItem.Merge(properties, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });

        if (request.Publish)
            await contentManager.PublishAsync(contentItem);
        else
            await contentManager.SaveDraftAsync(contentItem);

        return new ObjectResult(contentItem);
    }

    public record RequestModel(string ContentType, JsonNode Properties, bool Publish);
}