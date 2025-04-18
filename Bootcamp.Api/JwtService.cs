using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bootcamp.Api.Models.Db;
using Microsoft.IdentityModel.Tokens;
using YandexFoundationModels.ApiClient.Dto;

namespace Bootcamp.Api;

public interface IJwtService
{
    string GenerateToken(User user);
}

public class JwtService(IConfiguration configuration) : IJwtService
{
    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(configuration["Jwt:ExpireMinutes"])),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}