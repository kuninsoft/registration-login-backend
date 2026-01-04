using System.ComponentModel.DataAnnotations;

namespace RegistrationAndLoginApi.DataAccess.Entities;

public class User
{
    [Key] public Guid Id { get; set; }
    
    public string FullName { get; set; } = null!;
    public string Username { get; init; } = null!;
    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = Roles.User;
}