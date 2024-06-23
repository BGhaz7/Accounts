using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Accounts.Service.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Accounts.Service.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _secretKey = "sGQ7+cHIYRyCJoq1l0F9utfBhCG4jxDVq9DKhrWyXys=";

        public string GenerateJwtToken(string username, List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int ExtractIdFromJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId);
            if (userIdClaim == null)
            {
                throw new InvalidOperationException("Token does not contain a user ID.");
            }
            return int.Parse(userIdClaim.Value);
        }
    }
}