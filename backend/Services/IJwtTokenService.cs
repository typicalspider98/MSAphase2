// backend/Services/IJwtTokenService.cs
using backend.Models;
namespace backend.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
