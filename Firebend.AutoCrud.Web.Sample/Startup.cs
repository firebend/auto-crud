using System;
using System.IdentityModel.Tokens.Jwt;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework;
using Firebend.AutoCrud.Mongo;
using Firebend.AutoCrud.Web.Implementations.Authorization.Requirements;
using Firebend.AutoCrud.Web.Sample.Authorization.Handlers;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.DomainEvents;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Tenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Firebend.AutoCrud.Web.Sample
{
    public static class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = false,
                    SignatureValidator = delegate(string token,
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
                .AddDbContext<PersonDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("SqlServer"))
                )
                .UsingMongoCrud(configuration.GetConnectionString("Mongo"), true, mongo => mongo.AddMongoPerson())
                .UsingEfCrud(ef =>
                {
                    ef.AddEfPerson(configuration)
                        .AddEfPets(configuration)
                        .WithDomainEventContextProvider<SampleDomainEventContextProvider>();
                })
                .AddSampleMassTransit(configuration)
                .AddRouting()
                .AddSwaggerGen()
                .AddFirebendAutoCrudApiBehaviors()
                .AddScoped<IDistributedLockService, CustomLockService>()
                .AddControllers()
                .AddNewtonsoftJson()
                .AddFirebendAutoCrudWeb(services);

            services.Configure<ApiBehaviorOptions>(o => o.SuppressInferBindingSourcesForParameters = true);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(ReadAllAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new ReadAllAuthorizationRequirement()));
                options.AddPolicy(ReadAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new ReadAuthorizationRequirement()));
                options.AddPolicy(CreateAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new CreateAuthorizationRequirement()));
                options.AddPolicy(CreateMultipleAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new CreateMultipleAuthorizationRequirement()));
                options.AddPolicy(UpdateAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new UpdateAuthorizationRequirement()));
                options.AddPolicy(DeleteAuthorizationRequirement.DefaultPolicy,
                    policy => policy.Requirements.Add(new DeleteAuthorizationRequirement()));
            });
            services.AddSingleton<IAuthorizationHandler, ReadAllAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, ReadAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, CreateMultipleAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, UpdateAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, DeleteAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.Use(async (context, next) =>
            // {
            //     context.Request.Headers.Add("Authorization",
            //         "Bearer eyJhbGciOiJIUzI1NiJ9.eyJSb2xlIjoiQWRtaW4iLCJJc3N1ZXIiOiJJc3N1ZXIiLCJVc2VybmFtZSI6IkphdmFJblVzZSIsImV4cCI6MTY0NTgwMDQwMSwiaWF0IjoxNjQ1ODAwNDAxfQ.XGjDqgMLK-D_X5EZmpeFqslflX6QxEfhCibPLwALP2I");
            //     await next(context);
            // });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => await context.Response.WriteAsync("Hello World!"));

                endpoints.MapControllers();
            });

            app.UseSwagger(opt => opt.RouteTemplate = "/open-api/{documentName}/open-api.json");

            app.UseSwaggerUI(opt => opt.SwaggerEndpoint("/open-api/v1/open-api.json", "Firebend Auto Crud Web Sample"));
        }
    }
}
