using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Title.Models;
using OrchardExperiments.Api.Extensions;

namespace OrchardExperiments.Api.Controllers.Tags;

[IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public class Resolve(IAuthenticationService authenticationService, IAuthorizationService authorizationService, IContentManager contentManager, IContentHandleManager contentHandleManager) : ControllerBase
{
    [HttpPost("api/tags/resolve")]
    public async Task<IActionResult> HandleAsync(RequestModel request)
    {
        var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, Schemes.Api);
        if (authenticateResult.Succeeded)
            HttpContext.User = authenticateResult.Principal;

        if (!await authorizationService.AuthorizeAsync(User, Permissions.RestApiAccess))
            return this.ChallengeOrForbid(Schemes.Api);

        if (!await authorizationService.AuthorizeAsync(User, OrchardCore.Taxonomies.Permissions.ManageTaxonomies))
            return this.ChallengeOrForbid(Schemes.Api);

        // Get the Tags taxonomy content item.
        var tagsTaxonomyContentItemId = await contentHandleManager.GetContentItemIdAsync("alias:tags");
        var tagsTaxonomyContentItem = await contentManager.GetAsync(tagsTaxonomyContentItemId);
        
        if(tagsTaxonomyContentItem == null)
            throw new Exception("Could not find tags taxonomy");

        var taxonomyPart = tagsTaxonomyContentItem.As<TaxonomyPart>();
        var requestedTags = request.Tags;
        var existingTagTerms = taxonomyPart.Terms.Where(t => requestedTags.Contains(t.As<TitlePart>().Title, StringComparer.OrdinalIgnoreCase)).ToList();
        var existingTags = existingTagTerms.Select(t => t.As<TitlePart>().Title).ToList();
        var missingTagTerms = requestedTags.Except(existingTags).ToList();

        foreach (var missingTagTerm in missingTagTerms)
        {
            
        }
        
        

        //return new ObjectResult(contentItem);

        return Ok();
    }

    public record RequestModel(string[] Tags);
}