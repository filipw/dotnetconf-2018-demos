using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Authentication.Embedded.Models;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.Authentication.Embedded
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IContactRepository, InMemoryContactRepository>();
            services.AddMvcCore().
                AddDataAnnotations().
                AddJsonFormatters().
                AddAuthorization(o =>
                {
                    o.AddPolicy("ViewContacts", p => p.RequireAuthenticatedUser().RequireClaim("scope", "contacts.view"));
                    o.AddPolicy("ManageContacts", p => p.RequireAuthenticatedUser().RequireClaim("scope", "contacts.manage"));
                });

            // set up embedded identity server
            services.AddIdentityServer().
                AddTestClients().
                AddTestResources().
                AddInMemoryUsers().
                AddDeveloperSigningCredential();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, o =>
                {
                    o.Authority = "https://localhost:44356/openid";
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Map("/openid", id => {
                // use embedded identity server to issue tokens
                id.UseIdentityServer();
            });

            app.Map("/api", api => {
                // consume the JWT tokens in the API
                api.UseAuthentication();
                api.UseMvc();
            });
        }
    }
}
