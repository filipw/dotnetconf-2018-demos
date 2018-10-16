using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AspNetCore.Authentication.Embedded
{
    public static class IdentityServerBuilderTestExtensions
    {
        public static IIdentityServerBuilder AddTestClients(this IIdentityServerBuilder builder) =>
            builder.AddInMemoryClients(new[] {
                new Client
                {
                    ClientId = "dummy_m2m_client",
                    ClientSecrets =
                    {
                        new Secret("super_secret".Sha256())
                    },
                    AllowedGrantTypes = new[]
                    {
                        GrantType.ClientCredentials
                    },
                    AllowedScopes = new[]
                    {
                        "orders.manage",
                        "orders.view"
                    }
                },
                new Client
                {
                    ClientId = "dummy_user_app_client",
                    ClientSecrets =
                    {
                        new Secret("super_secret".Sha256())
                    },
                    AllowedGrantTypes = new[]
                    {
                        GrantType.ResourceOwnerPassword
                    },
                    AllowedScopes = new[]
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "orders.view",
                        "orders.place"
                    },
                    AllowOfflineAccess = true
                }
            });

        public static IIdentityServerBuilder AddTestResources(this IIdentityServerBuilder builder) =>
            builder.AddInMemoryApiResources(new[]
            {
                new ApiResource("dev.shop.api")
                {
                    Scopes =
                    {
                        new Scope("orders.manage"),
                        new Scope("orders.view"),
                        new Scope("orders.place")
                    },
                    Enabled = true,
                    UserClaims = new[]
                    {
                        JwtClaimTypes.GivenName,
                        JwtClaimTypes.FamilyName,
                        JwtClaimTypes.Email
                    }
                },
            }).AddInMemoryIdentityResources(new IdentityResource[] {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            });

        public static IIdentityServerBuilder AddInMemoryUsers(this IIdentityServerBuilder builder)
        {
            builder.Services.AddSingleton<IResourceOwnerPasswordValidator, InMemoryUserResourceOwnerPasswordValidator>();
            return builder;
        }
    }

    public class InMemoryUserResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private List<TestUser> _testUsers = new List<TestUser>
        {
            new TestUser
            {
                SubjectId = "d01440edc505489e904878ce9738865a",
                Username = "filip",
                Password = "super_secret",
                Claims = new HashSet<Claim>(new ClaimComparer())
                {
                    new Claim(JwtClaimTypes.GivenName, "Filip"),
                    new Claim(JwtClaimTypes.FamilyName, "W"),
                    new Claim(JwtClaimTypes.Email, "filip@w.com"),
                }
            }
        };

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (ValidateCredentials(context.UserName, context.Password, out var user))
            {
                context.Result = new GrantValidationResult(user.SubjectId, OidcConstants.AuthenticationMethods.Password, user.Claims, "embedded_idp");
            }

            return Task.CompletedTask;
        }

        private bool ValidateCredentials(string userName, string password, out TestUser user)
        {
            user = _testUsers.FirstOrDefault(x => x.Username == userName && x.Password == password);
            return user != null;
        }

        private class TestUser
        {
            public string SubjectId { get; set; }

            public string Username { get; set; }

            public string Password { get; set; }

            public ICollection<Claim> Claims { get; set; } 
        }
    }
}
