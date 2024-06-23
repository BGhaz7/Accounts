using System.Security.Claims;
namespace Accounts.Service.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(string username, List<Claim> claims);
        int ExtractIdFromJwtToken(string token);
    }
}


