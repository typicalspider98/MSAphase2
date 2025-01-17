// backend/Services/JwtTokenService.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using backend.Models;

namespace backend.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.UserName))
                throw new ArgumentNullException(nameof(user), "User or UserName cannot be null.");

            var keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
                throw new InvalidOperationException("JWT Key is not configured properly.");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
