using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Interfaces.Services;

public interface IEntityCrudGenerator
{
    List<BaseBuilder> Builders { get; }

    IServiceCollection ServiceCollection { get; }

    IServiceCollection Generate();
}
