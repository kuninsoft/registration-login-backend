using RegistrationAndLoginApi.DataAccess.Entities;

namespace RegistrationAndLoginApi.Auth;

public interface IAuthService
{
    (string accessToken, string refreshToken) IssueTokens(User user);
    (string accessToken, string refreshToken) RefreshTokens(string refreshToken);
}