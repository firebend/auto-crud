using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Swagger;

public class V1 : IAutoCrudApiVersion
{
    public int Version => 1;
    public string Name => "Version 1";
}
