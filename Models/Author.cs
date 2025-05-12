using System;
using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models;

public class Author
{
    public int Id { get; set; }

    [Required]
    public required string AuthorName { get; set; }

    [Required]
    public required byte[] PasswordHash { get; set; }

    [Required]
    public required byte[] PasswordSalt { get; set; }

    [Required, EmailAddress]
    [MaxLength(256)]
    public required string Email { get; set; }

    [Required]
    [MaxLength(50)]
    public string Role { get; set; } = "Guest";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
