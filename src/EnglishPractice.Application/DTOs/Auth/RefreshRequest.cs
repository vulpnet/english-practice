using System.ComponentModel.DataAnnotations;
namespace EnglishPractice.Application.DTOs.Auth;
public class RefreshRequest
{
    [Required] public string RefreshToken { get; set; } = string.Empty;
}
