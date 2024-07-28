using System.Text.Json.Nodes;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardExperiments.Api.Extensions;

namespace OrchardExperiments.Api.Controllers.Content
{
    [Route("api/content/{id}")]
    [IgnoreAntiforgeryToken, AllowAnonymous]
    [ApiController]
    public class Patch(IAuthenticationService authenticationService, IAuthorizationService authorizationService, IContentManager contentManager) : ControllerBase
    {
        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> HandleAsync(string id, JsonNode patch)
        {
            var authenticateResult = await authenticationService.AuthenticateAsync(HttpContext, "Api");
            if (authenticateResult.Succeeded) 
                HttpContext.User = authenticateResult.Principal;
            
            if (!await authorizationService.AuthorizeAsync(User, Permissions.RestApiAccess))
                return this.ChallengeOrForbid("Api");

            var contentItem = await contentManager.GetAsync(id);
            contentItem.Merge(patch, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });
            await contentManager.UpdateAsync(contentItem);

            return new ObjectResult(contentItem);
        }
    }
}