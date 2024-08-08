using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Modules;
using OrchardCore.Taxonomies.Models;
using OrchardCore.Title.Models;
using OrchardExperiments.Api.Extensions;

namespace OrchardExperiments.Api.Controllers.Tags;

[IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public class Resolve(
    IAuthenticationService authenticationService,
    IAuthorizationService authorizationService,
    IContentManager contentManager,
    IContentHandleManager contentHandleManager,
    IContentDefinitionManager contentDefinitionManager,
    IEnumerable<IContentHandler> contentHandlers,
    ILogger<Resolve> logger) : ControllerBase
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

        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync("Taxonomy");
        var versionOptions = contentTypeDefinition.IsDraftable() ? VersionOptions.DraftRequired : VersionOptions.Latest;
        var tagsTaxonomyContentItemId = await contentHandleManager.GetContentItemIdAsync("alias:tags");
        var tagsTaxonomyContentItem = await contentManager.GetAsync(tagsTaxonomyContentItemId, versionOptions);

        if (tagsTaxonomyContentItem == null)
            throw new Exception("Could not find tags taxonomy");

        var taxonomyPart = tagsTaxonomyContentItem.As<TaxonomyPart>();
        var requestedTags = request.Tags;
        var existingTagTerms = taxonomyPart.Terms.Where(t => requestedTags.Contains(t.As<TitlePart>().Title, StringComparer.OrdinalIgnoreCase)).ToList();
        var allTagTerms = existingTagTerms.ToList();
        var existingTags = existingTagTerms.Select(t => t.As<TitlePart>().Title).ToList();
        var missingTagTerms = requestedTags.Except(existingTags).ToList();

        foreach (var missingTagTerm in missingTagTerms)
        {
            var newTagTerm = await CreateTagTermContentItem(tagsTaxonomyContentItem, missingTagTerm);
            allTagTerms.Add(newTagTerm);
        }

        if (missingTagTerms.Any())
        {
            if (contentTypeDefinition.IsDraftable())
                await contentManager.PublishAsync(tagsTaxonomyContentItem);
            else
                await contentManager.SaveDraftAsync(tagsTaxonomyContentItem);
        }

        var response = new
        {
            Tags = allTagTerms.Select(x => new
            {
                Tag = x.As<TitlePart>().Title,
                ContentItemId = x.ContentItemId,
            }).ToList()
        };
        return new ObjectResult(response);
    }

    private async Task<ContentItem> CreateTagTermContentItem(ContentItem tagsTaxonomy, string tagName)
    {
        // Create a tag term but only run content handlers, not content item display manager update editor.
        // This creates empty parts if parts are attached to the tag term, with empty data.
        // But still generates valid auto-route paths from the handler. 
        var tagsTaxonomyPart = tagsTaxonomy.As<TaxonomyPart>();
        var termContentItem = await contentManager.NewAsync(tagsTaxonomyPart.TermContentType);
        termContentItem.DisplayText = tagName;
        termContentItem.Alter<TitlePart>(part => part.Title = tagName);
        termContentItem.Weld<TermPart>();
        termContentItem.Alter<TermPart>(t => { t.TaxonomyContentItemId = tagsTaxonomy.ContentItemId; });
        var updateContentContext = new UpdateContentContext(termContentItem);
        await contentHandlers.InvokeAsync((handler, context) => handler.UpdatingAsync(context), updateContentContext, logger);
        await contentHandlers.Reverse().InvokeAsync((handler, context) => handler.UpdatedAsync(context), updateContentContext, logger);
        tagsTaxonomy.Alter<TaxonomyPart>(part => part.Terms.Add(termContentItem));

        return termContentItem;
    }

    public record RequestModel(string[] Tags);
}