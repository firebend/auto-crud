#region

using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace Firebend.AutoCrud.Core.Interfaces.Services
{
    public interface IEntityCrudGenerator
    {
        List<EntityBuilder> Builders { get; }

        IServiceCollection ServiceCollection { get; }

        IServiceCollection Generate();
    }
}