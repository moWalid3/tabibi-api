using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Tabibi.API.Configurations;
using Tabibi.API.DTOs.Auth;

namespace Tabibi.API.Services
{
    public sealed class TokenProvider(IOptions<JwtAuthOptions> options)
    {
        private readonly JwtAuthOptions jwtAuthOptions = options.Value;

        public AccessTokensDto Create(TokenRequestDto tokenRequestDto)
        {
            return new AccessTokensDto(GenerateToken(tokenRequestDto), GenerateRefreshToken());
        }

        private string GenerateToken(TokenRequestDto tokenRequestDto)
        {
            Claim[] claims = [
                new(ClaimTypes.NameIdentifier, tokenRequestDto.UserId),
                new(ClaimTypes.Email, tokenRequestDto.Email),
                ..tokenRequestDto.Roles.Select(role => new Claim(ClaimTypes.Role, role))
            ];

            SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(jwtAuthOptions.Key));

            SigningCredentials signingCredentials = new(securityKey, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Issuer = jwtAuthOptions.Issuer,
                Audience = jwtAuthOptions.Audience,
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = signingCredentials,
                Expires = DateTime.UtcNow.AddMinutes(jwtAuthOptions.ExpirationInMinutes)
            };

            string accessToken = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

            return accessToken;
        }

        private static string GenerateRefreshToken()
        {
            byte[] guidBytes = Encoding.UTF8.GetBytes(Guid.CreateVersion7().ToString());
            byte[] randomBytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String([.. guidBytes, .. randomBytes]);
        }
    }
}
