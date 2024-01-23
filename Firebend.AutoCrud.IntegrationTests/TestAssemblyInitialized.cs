using Flurl.Http;
using Flurl.Http.Newtonsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public class TestAssemblyInit
{
    [AssemblyInitialize]
    public static void AssemblyInit(TestContext _)
    {
        FlurlHttp.Clients.UseNewtonsoft();
    }
}
