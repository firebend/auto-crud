using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudGenerator
    {
        List<EntityBuilder> Builders { get; }
        
        IServiceCollection ServiceCollection { get; }
        
        void Generate();
    }
}