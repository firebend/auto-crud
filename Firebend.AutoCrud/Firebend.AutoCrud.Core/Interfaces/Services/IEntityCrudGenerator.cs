using Firebend.AutoCrud.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudGenerator
    {
        void Generate(IServiceCollection collection, EntityCrudBuilder builder);
    }
}