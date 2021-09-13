using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.EntityFramework.Sample.DbContexts;
using Firebend.AutoCrud.EntityFramework.Sample.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Firebend.AutoCrud.EntityFramework.Sample
{
    public class SampleHostedService : BackgroundService
    {
        private readonly IEntityCreateService<Guid, Person> _createService;
        private readonly ILogger<SampleHostedService> _logger;
        private readonly IPersonReadRepository _readService;
        private readonly IEntitySearchService<Guid, Person, EntitySearchRequest> _searchService;
        private readonly JsonSerializer _serializer;
        private readonly IEntityUpdateService<Guid, Person> _updateService;

        public SampleHostedService(IServiceProvider serviceProvider, ILogger<SampleHostedService> logger)
        {
            _logger = logger;

            using var scope = serviceProvider.CreateScope();
            _createService = scope.ServiceProvider.GetService<IEntityCreateService<Guid, Person>>();
            _updateService = scope.ServiceProvider.GetService<IEntityUpdateService<Guid, Person>>();
            _readService = scope.ServiceProvider.GetService<IPersonReadRepository>();
            _searchService = scope.ServiceProvider.GetService<IEntitySearchService<Guid, Person, EntitySearchRequest>>();

            var context = scope.ServiceProvider.GetService<AppDbContext>();

            if (context == null)
            {
                const string msg = "Could nto resolve ef context";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            context.Database.EnsureCreated();

            if (_createService == null)
            {
                const string msg = "Could not resolve create service";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            if (_updateService == null)
            {
                const string msg = "Could not resolve update service";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            if (_readService == null)
            {
                const string msg = "Could not resolve read service";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            if (_searchService == null)
            {
                const string msg = "Could not resolve search service";
                _logger.LogError(msg);
                throw new Exception(msg);
            }

            _serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Sample...");

            try
            {
                var entity = await _createService.CreateAsync(
                    new Person
                    {
                        FirstName = $"First Name -{DateTimeOffset.UtcNow}",
                        LastName = $"Last Name -{DateTimeOffset.UtcNow}",
                        Pets = new List<Pet> { new Pet { Id = Guid.NewGuid(), Name = "Mr. Bojangles" } }
                    }, cancellationToken);
                LogObject("Entity added....");

                entity.FirstName = $"{entity.FirstName} - updated";
                var updated = await _updateService.UpdateAsync(entity, cancellationToken);
                LogObject("Entity updated...");

                var patch = new JsonPatchDocument<Person>();
                patch.Add(x => x.FirstName, $"{updated.FirstName} - patched");
                var patched = await _updateService.PatchAsync(updated.Id, patch, cancellationToken);
                LogObject("Entity patched...");

                var read = await _readService.GetByKeyAsync(patched.Id, cancellationToken);
                LogObject("Entity Read...", read);

                var search = await _searchService.PageAsync(new EntitySearchRequest { Search = "First", PageNumber = 1, PageSize = 10, DoCount = true },
                    cancellationToken);
                LogObject("Page....", search);

                var all = await _readService.GetAllAsync(cancellationToken);
                LogObject("All Entities....", all);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sample");
            }
        }

        private void LogObject(string message, object entity = null)
        {
            // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
            _logger.LogInformation(message);

            if (entity != null)
            {
                _serializer.Serialize(Console.Out, entity);
                Console.WriteLine();
            }
        }
    }
}
