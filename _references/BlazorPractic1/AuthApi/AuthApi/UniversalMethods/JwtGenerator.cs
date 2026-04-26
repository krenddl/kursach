using Microsoft.IdentityModel.Tokens;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthApi.UniversalMethods
{
    public class JwtGenerator
    {
        private readonly string _secretKey;

        public JwtGenerator(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Key"] ?? throw new Exception("JWT not found");
        }

        public string GenerateToken(int userId, int roleId)
        {
            var claims = new[]
            {
                new Claim("userId", userId.ToString()),
                new Claim("roleId", roleId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
