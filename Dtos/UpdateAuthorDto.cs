using System;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.Dtos;

public class UpdateAuthorDto
{
    [Required(ErrorMessage = "Author name is required")]
    [MaxLength(50, ErrorMessage = "Author name cannot exceed 50 characters")]
    public required string AuthorName { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Email is required"), EmailAddress]
    [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public required string Email { get; set; }

    [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters")]
    public string Role { get; set; } = "Guest";
}
