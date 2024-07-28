using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace OrchardExperiments.Api.Extensions;

[PublicAPI]
public static class ControllerBaseExtensions
{
    public static ActionResult ChallengeOrForbid(this ControllerBase controller)
    {
        return controller.User.Identity?.IsAuthenticated ?? false ? controller.Forbid() : controller.Challenge();
    }

    /// <summary>
    /// Returns the proper actionresult for unauthorized or unauthenticated users
    /// with the specified authenticationSchemes.
    /// Will return a forbid when the user is authenticated.
    /// Will return a challenge when the user is not authenticated.
    /// If authentication schemes are specified, will return a challenge to them.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="authenticationSchemes">The authentication schemes to challenge.</param>
    /// <returns>The proper actionresult based upon if the user is authenticated.</returns>
    public static ActionResult ChallengeOrForbid(this ControllerBase controller, params string[] authenticationSchemes)
    {
        return controller.User?.Identity?.IsAuthenticated ?? false ? controller.Forbid(authenticationSchemes) : controller.Challenge(authenticationSchemes);
    }
}