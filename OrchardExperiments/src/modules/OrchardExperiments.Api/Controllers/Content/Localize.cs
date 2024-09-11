using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentLocalization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;

namespace OrchardExperiments.Api.Controllers.Content;

[IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public class Localize(
    IContentManager contentManager,
    IContentLocalizationManager localizationManager,
    IAuthenticationService authenticationService, 
    IAuthorizationService authorizationService)
    : ControllerBase
{
    [HttpPost("api/content-items/{id}/localize")]
    public async Task<IActionResult> HandleAsync(string id, RequestModel request)
    {
        var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, Schemes.Api);
        if (authenticateResult.Succeeded) 
            HttpContext.User = authenticateResult.Principal;
    
        if (!await authorizationService.AuthorizeAsync(User, Permissions.RestApiAccess))
            return this.ChallengeOrForbid(Schemes.Api);
        
        var targetCulture = request.CultureCode ?? string.Empty; // Empty is invariant culture.
        var contentItem = await contentManager.GetAsync(id, VersionOptions.Latest);

        if (contentItem == null)
            return NotFound();

        if (!await authorizationService.AuthorizeAsync(User, OrchardCore.ContentLocalization.Permissions.LocalizeContent, contentItem))
            return this.ChallengeOrForbid(Schemes.Api);

        var checkContentItem = await contentManager.NewAsync(contentItem.ContentType);

        // Set the current user as the owner to check for ownership permissions on creation
        checkContentItem.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!await authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, checkContentItem))
            return this.ChallengeOrForbid(Schemes.Api);

        var part = contentItem.As<LocalizationPart>();

        if (part == null)
        {
            ModelState.AddModelError("", "The specified content item's ContentDefinition requires the LocalizationPart");
            return BadRequest(ModelState);
        }

        var alreadyLocalizedContent = await localizationManager.GetContentItemAsync(part.LocalizationSet, targetCulture);

        if (alreadyLocalizedContent != null)
            return new ObjectResult(alreadyLocalizedContent);

        try
        {
            var newContent = await localizationManager.LocalizeAsync(contentItem, targetCulture);
            return new ObjectResult(newContent);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return BadRequest(ModelState);
        }
    }

    public record RequestModel(string? CultureCode);
}