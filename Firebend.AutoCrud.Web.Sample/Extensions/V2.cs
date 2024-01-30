using Firebend.AutoCrud.Core.Interfaces;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

public class V2 : IAutoCrudApiVersion
{
    public int Version => 2;
    public string Name => "Api V2";
}
