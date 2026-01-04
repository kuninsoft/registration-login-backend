using RegistrationAndLoginApi.DataAccess.Entities;

namespace RegistrationAndLoginApi.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password);
}