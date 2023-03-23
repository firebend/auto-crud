using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.CustomFields.Web;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Sample.Authorization;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Firebend.AutoCrud.Web.Sample
{
    public static class Startup
    {
        private const string AuthorizationHeaderKey = "Authorization";

        private const string AuthorizationHeaderValue =
            "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJBdXRvQ3J1ZEpXVFNlcnZpY2VBY2Nlc3NUb2tlbiIsImp0aSI6ImY3ZDI4MmE1LTRhOTItNDBjYS05MzE4LWY0MWFiOTA2YzY0ZSIsImlhdCI6MTY1NDA0MDQzNiwiVXNlcklkIjoiYmVmZDk2MDUtNjk3Yi00Y2M3LWJhN2EtNDBkNDRiYjliZjZjIiwiZW1haWwiOiJ0bnNjaG5laWRlckBnbWFpbC5jb20iLCJuYmYiOjE2NTQwNDA0MzYsImV4cCI6MTY1NDA0NDAzNiwiaXNzIjoiQXV0b0NydWQiLCJhdWQiOiJBdXRvQ3J1ZENsaWVudCJ9.fvFV3WmIAxMOqDA1ToE-WGrTr6BL-_hydS9HOrf89_4";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Do not use this code in production
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false,
                    SignatureValidator = delegate (string token,
                        TokenValidationParameters _)
                    {
                        var jwt = new JwtSecurityToken(token);
                        return jwt;
                    },
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero,
                    RequireSignedTokens = false
                };
            });

            services
                .AddScoped<ITenantEntityProvider<int>, SampleTenantProvider>()
                .AddHttpContextAccessor()
                .AddDbContext<PersonDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("SqlServer")))
                .AddDbContext<PersonDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("SqlServer"))
                )
                .UsingMongoCrud(mongo => mongo
                    .WithMigrationConnectionString(configuration.GetConnectionString("Mongo"))
                    .AddMongoPerson(configuration))
                .UsingEfCrud(ef =>
                {
                    ef.AddEfPerson(configuration)
                        .AddEfPets(configuration)
                        .WithDomainEventContextProvider<SampleDomainEventContextProvider>();
                })
                .AddSampleMassTransit(configuration)
                .AddRouting()
                .AddAutoCrudOpenApi(description => $"Firebend Auto Crud Web Sample {description.GroupName}", forwardNonDeprecatedEndpoints: true)
                .AddFirebendAutoCrudApiBehaviors()
                .AddScoped<IDistributedLockService, CustomLockService>()
                .AddControllers()
                .AddNewtonsoftJson()
                .AddFirebendAutoCrudWeb(services)
                .Services
                .AddDefaultResourceAuthorizationRequirements()
                .AddDefaultChangeTrackingResourceAuthorizationRequirement()
                .AddDefaultCustomFieldsResourceAuthorizationRequirement()
                .AddResourceAuthorizationHandlers();

            services.AddScoped<DataAuthService>();
            services.Configure<ApiBehaviorOptions>(o => o.SuppressInferBindingSourcesForParameters = true);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                if (context.Request.Headers.All(h => h.Key != AuthorizationHeaderKey))
                {
                    context.Request.Headers.Add(AuthorizationHeaderKey, AuthorizationHeaderValue);
                }

                await next(context);
            });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => await context.Response.WriteAsync("Hello World!"));

                endpoints.MapControllers();
            });

            app.UseSwagger(opt => opt.RouteTemplate = "/open-api/{documentName}/open-api.json");

            app.UseSwaggerUI(opt =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    opt.SwaggerEndpoint($"/open-api/{description.GroupName}/open-api.json", $"Firebend Auto Crud Web Sample {description.GroupName}");
                }
            });
        }
    }
}
