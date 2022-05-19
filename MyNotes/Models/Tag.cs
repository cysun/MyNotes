using System.ComponentModel.DataAnnotations;

namespace MyNotes.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string Label { get; set; }

    public int NoteCount { get; set; }

    public DateTime LastUsed { get; set; } = DateTime.UtcNow;

    public bool Retired { get; set; }
}
