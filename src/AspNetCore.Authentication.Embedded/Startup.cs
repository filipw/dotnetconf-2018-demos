using System;
using System.Collections.Generic;
using System.Linq;
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
using Newtonsoft.Json.Converters;

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
            services.AddSingleton<OrderService>();
            services.AddMvcCore().
                AddDataAnnotations().
                AddJsonFormatters().
                AddAuthorization(o =>
                {
                    o.AddPolicy("ViewOrders", p => p.RequireAuthenticatedUser().RequireClaim("scope", "orders.view"));
                    o.AddPolicy("ManageOrders", p => p.RequireAuthenticatedUser().RequireClaim("scope", "orders.manage"));
                    o.AddPolicy("PlaceOrders", p => p.RequireAuthenticatedUser().RequireClaim("scope", "orders.place"));
                }).
                AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter
                    {
                        CamelCaseText = true
                    });
                });

            services.AddSingleton<IAuthorizationHandler, SelfOnlyHandler>();

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
                    o.SupportedTokens = SupportedTokens.Jwt;
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

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
