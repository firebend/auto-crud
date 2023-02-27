using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Firebend.AutoCrud.ChangeTracking.Web;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.CustomFields.Web;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Abstractions;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Sample.Authorization;
using Firebend.AutoCrud.Web.Sample.Controllers;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Models;
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
using Microsoft.OpenApi.Models;

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
                .AddSingleton<IApiVersion, V1>()
                .AddSingleton<IApiVersion, V2>()
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
                //TODO TS: add an extension to do add api versioning, add versioned api explorer, and add swagger gen
                .AddApiVersioning(o =>
                {
                    o.ReportApiVersions = false;
                    o.AssumeDefaultVersionWhenUnspecified = false;

                    var provider = services.BuildServiceProvider();
                    var versions = provider.GetServices<IApiVersion>();

                    var allControllerTypes = services
                        .Where(x => x.ServiceType.IsAssignableTo(typeof(IAutoCrudController)))
                        .Select(x => x.ServiceType)
                        .ToList();

                    foreach (var version in versions.OrderBy(x => x.Version).ThenBy(x => x.MinorVersion))
                    {
                        // TODO TS
                        // use reflection to determine if there is a matching later version
                        // if not, back fill that version
                        // if so, mark this one as deprecated
                        // https://referbruv.com/blog/integrating-aspnet-core-api-versions-with-swagger-ui/

                        var type = typeof(AbstractEntityControllerBase<>).MakeGenericType(version.GetType());
                        var controllers = allControllerTypes.Where(x => x.IsAssignableTo(type)).ToList();

                        foreach (var controller in controllers)
                        {
                            o.Conventions.Controller(controller).HasApiVersion(new ApiVersion(version.Version, version.MinorVersion));
                        }
                    }
                })
                .AddVersionedApiExplorer(o =>
                {
                    o.GroupNameFormat = "'v'VVV";
                    o.SubstituteApiVersionInUrl = true;
                })
                .AddSampleMassTransit(configuration)
                .AddRouting()
                .AddSwaggerGen(o =>
                {
                    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        o.SwaggerDoc(
                            description.GroupName,
                            new OpenApiInfo
                            {
                                Title = $"Firebend Auto Crud Web Sample {description.GroupName}",
                                Version = description.ApiVersion.ToString()
                            });
                    }
                })
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
