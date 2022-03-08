using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[Route("api/token")]
[AllowAnonymous]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public TokenController(IConfiguration config)
    {
        _configuration = config;
    }

    [HttpPost]
    public IActionResult Post([FromBody] UserInfoPostDto userData)
    {
        if (userData is { Email: { }, Password: { } })
        {
            var user = GetUser(userData.Email, userData.Password);

            if (user != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
                        new Claim("UserId", user.UserId.ToString()),
                        new Claim(JwtRegisteredClaimNames.Email, user.Email)
                    };

                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:key"]);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new { Token = tokenHandler.WriteToken(token), Message = "Success" });
            }
            else
            {
                return BadRequest("Invalid credentials");
            }
        }
        else
        {
            return BadRequest();
        }
    }

    private static UserInfo GetUser(string email, string _) => new()
    {
        UserId = Guid.NewGuid(),
        UserName = "John",
        Email = email,
        CreatedDate = DateTime.Now
    };
}
