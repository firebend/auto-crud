using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.IntegrationTests;

public class TestConstants
{
    public const string BaseUrl = "http://localhost:5020/api";
    public const string AuthenticationUrl = $"{BaseUrl}/token";
    public static readonly UserInfoPostDto TestUser = new() {Email = "developer@test.com", Password = "password"};
    public static readonly UserInfoPostDto UnauthorizedTestUser = new() {Email = "NotAuthorized@test.com", Password = "password"};
}
