using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Firebend.AutoCrud.IntegrationTests.TestConstants;

namespace Firebend.AutoCrud.IntegrationTests;

[TestClass]
public static class TestRunner
{
    public static string Token { get; set; }

    [TestInitialize]
    public static async Task TestInitialize()
    {
        await Authenticate();
    }

    public static async Task Authenticate(UserInfoPostDto userInfo = null) => Token = await TestFunctions.GetAuthToken(userInfo ?? TestUser);
}
