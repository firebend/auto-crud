using System.Text;
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
using Microsoft.AspNetCore.Authentication.Cookies;
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
            // should we just mock a token and log in the user?
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidIssuer = configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
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
                options.AddPolicy("ReadAll", policy => policy.Requirements.Add(new ReadAllAuthorizationRequirement()));
            });
            services.AddSingleton<IAuthorizationHandler, CreateAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

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
