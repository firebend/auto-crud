using System;

namespace Firebend.AutoCrud.Core.Models;

public class ServiceRegistration : Registration
{
    public Type ServiceType { get; set; }
}
