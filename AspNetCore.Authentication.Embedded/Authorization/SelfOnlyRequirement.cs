using Microsoft.AspNetCore.Authorization;

namespace AspNetCore.Authentication.Embedded
{
    public class SelfOnlyRequirement : IAuthorizationRequirement
    {
        public static SelfOnlyRequirement Instance { get; } = new SelfOnlyRequirement();
    }
}
