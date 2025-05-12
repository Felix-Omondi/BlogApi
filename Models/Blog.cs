using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogApi.Models;

public class Blog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public required string Title { get; set; }

    [Required]
    public required string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int AuthorId { get; set; }

    [JsonIgnore]
    public Author? Author { get; set; }
}