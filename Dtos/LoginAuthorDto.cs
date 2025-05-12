using System;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class LoginAuthorDto
{
    [Required(ErrorMessage = "Author name is required")]
    [MaxLength(50, ErrorMessage = "Author name cannot exceed 50 characters")]
    public required string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public required string Password { get; set; } = string.Empty;
}
