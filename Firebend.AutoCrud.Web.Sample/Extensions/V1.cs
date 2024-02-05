using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

public class V1 : IAutoCrudApiVersion
{
    public int Version => 1;
    public string Name => "Api V1";
}
