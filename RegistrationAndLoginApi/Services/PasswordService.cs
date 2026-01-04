using Microsoft.AspNetCore.Identity;
using RegistrationAndLoginApi.DataAccess.Entities;

namespace RegistrationAndLoginApi.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();
    
    public string HashPassword(User user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(User user, string password)
    {
        PasswordVerificationResult result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

        return result == PasswordVerificationResult.Success;
    }
}