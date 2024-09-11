using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;

namespace OrchardExperiments.Api.Controllers.Content;

[IgnoreAntiforgeryToken, AllowAnonymous]
[ApiController]
public class Patch(IAuthenticationService authenticationService, IAuthorizationService authorizationService, IContentManager contentManager) : ControllerBase
{
    [HttpPatch("api/content-items/{id}")]
    public async Task<IActionResult> HandleAsync(string id, RequestModel request)
    {
        var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, Schemes.Api);
        if (authenticateResult.Succeeded) 
            HttpContext.User = authenticateResult.Principal;
            
        if (!await authorizationService.AuthorizeAsync(User, Permissions.RestApiAccess))
            return this.ChallengeOrForbid(Schemes.Api);
        
        var contentItem = await contentManager.GetAsync(id, VersionOptions.DraftRequired);
            
        if (contentItem == null)
            return NotFound();
            
        if (!await authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            return this.ChallengeOrForbid(Schemes.Api);
            
        var patch = request.Patch;
        contentItem.Merge(patch, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
        await contentManager.UpdateAsync(contentItem);
            
        if (request.Publish)
            await contentManager.PublishAsync(contentItem);
        else
            await contentManager.SaveDraftAsync(contentItem);

        return new ObjectResult(contentItem);
    }
    
    public record RequestModel(JsonNode Patch, bool Publish);
}