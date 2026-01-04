using System.ComponentModel.DataAnnotations;

namespace RegistrationAndLoginApi.Models;

public class RefreshRequestDto
{
    [Required] public string RefreshToken { get; set; } = null!;
}