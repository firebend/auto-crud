using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Models;

public abstract class Registration
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}
