using System.ComponentModel.DataAnnotations;

namespace RegistrationAndLoginApi.Models;

public class RegisterRequestDto
{
    [Required] public string FullName { get; set; } = null!;
    [Required][MaxLength(50)] public string Username { get; set; } = null!;
    [Required][MinLength(6)] public string Password { get; set; } = null!;
}