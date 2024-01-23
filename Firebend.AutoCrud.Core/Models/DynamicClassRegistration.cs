using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.Abstractions.Builders;
using Firebend.AutoCrud.Core.Models.ClassGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Models;

public abstract class Registration
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}

public class DynamicClassRegistration : Registration
{
    public string Signature { get; set; }

    public IEnumerable<PropertySet> Properties { get; set; }

    public Type Interface { get; set; }
}

public class InstanceRegistration : Registration
{
    public object Instance { get; set; }
}

public class ServiceRegistration : Registration
{
    public Type ServiceType { get; set; }
}

public class BuilderRegistration : Registration
{
    public BaseBuilder Builder { get; set; }
}
