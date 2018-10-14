using System.Threading.Tasks;
using AspNetCore.Authentication.Embedded.Models;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;

namespace AspNetCore.Authentication.Embedded
{
    public class SelfOnlyHandler : AuthorizationHandler<SelfOnlyRequirement, Order>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       SelfOnlyRequirement requirement, Order orderResource)
        {
            if (!context.User.HasClaim(c => c.Type == JwtClaimTypes.Subject && c.Issuer == "https://localhost:44356/openid"))
            {
                // we have our own M2M context
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            if (context.User.FindFirst(c => c.Type == JwtClaimTypes.Email)?.Value == orderResource.OrderedBy)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
