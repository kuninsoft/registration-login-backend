using System.ComponentModel.DataAnnotations;

namespace RegistrationAndLoginApi.Models;

public class LoginRequestDto
{
    [Required] public string Username { get; set; } = null!;
    [Required] public string Password { get; set; } = null!;
}