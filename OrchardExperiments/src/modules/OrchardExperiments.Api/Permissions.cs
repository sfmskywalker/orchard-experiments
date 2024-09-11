using JetBrains.Annotations;
using OrchardCore;
using OrchardCore.Security.Permissions;

namespace OrchardExperiments.Api;

[UsedImplicitly]
public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission RestApiAccess = new("RestApiAccess", "Access to Orchard Core REST API");

    private static readonly IEnumerable<Permission> AllPermissions =
    [
        RestApiAccess
    ];
    
    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult(AllPermissions);
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new()
        {
            Name = OrchardCoreConstants.Roles.Authenticated,
            Permissions =
            [
                RestApiAccess,
            ],
        }
    ];
}
