using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Interfaces.Services;

public interface IEntityCrudGenerator
{
    public List<BaseBuilder> Builders { get; }

    public IServiceCollection Services { get; }

    public IServiceCollection Generate();
}
