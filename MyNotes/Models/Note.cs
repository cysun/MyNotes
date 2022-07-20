using System.ComponentModel.DataAnnotations;

namespace MyNotes.Models;

public class Note
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Subject { get; set; }

    public string Content { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime Updated { get; set; } = DateTime.UtcNow;

    public DateTime? Published { get; set; }
    public bool IsPublic => Published != null && Published < DateTime.UtcNow;

    public int ViewCount { get; set; }

    public string Summary { get; set; }
    public bool IsBlog { get; set; }

    public bool Deleted { get; set; }
}
