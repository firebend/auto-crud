using System.Collections.Concurrent;
using System.Threading.Tasks;
using Firebend.AutoCrud.IntegrationTests.Models;
using Firebend.AutoCrud.Web.Sample.Models;
using FluentAssertions;
using Flurl.Http;
using static Firebend.AutoCrud.IntegrationTests.TestConstants;

namespace Firebend.AutoCrud.IntegrationTests;

public static class TestFunctions
{
    public static IFlurlRequest WithAuth(this string url, string token = null) =>
        url.WithHeader("Authorization",
            $"Bearer {token ?? TestRunner.Token}");

    public static async Task<string> GetAuthToken(UserInfoPostDto userInfo)
    {
        async Task<string> FetchAuthTokenAsync(string email)
        {
            var response = await AuthenticationUrl.PostJsonAsync(userInfo);

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseModel = await response.GetJsonAsync<TokenResponseModel>();

            responseModel.Should().NotBeNull();
            responseModel.Token.Should().NotBeNull();

            return responseModel.Token;
        }

        return await TokenCache.Dict.GetOrAdd(userInfo.Email, FetchAuthTokenAsync);
    }

    private static class TokenCache
    {
        internal static ConcurrentDictionary<string, Task<string>> Dict { get; } = new();
    }
}
