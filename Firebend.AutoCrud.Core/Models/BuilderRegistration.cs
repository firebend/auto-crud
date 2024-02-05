using Firebend.AutoCrud.Core.Abstractions.Builders;

namespace Firebend.AutoCrud.Core.Models;

public class BuilderRegistration : Registration
{
    public BaseBuilder Builder { get; set; }
}
