using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure
{
    public class TokenService (IOptions<JwtSettings> jwtSettings) : ITokenService
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public string CreateToken(User user)
        {
            ArgumentNullException.ThrowIfNull(user);


            if (string.IsNullOrEmpty(user.Id.ToString()))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(user));
            }

            if (string.IsNullOrEmpty(user.UserName))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(user));
            }

            Console.WriteLine("Generating Token");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}
